using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using NetsBrokerIntegration.DotNet;
using Newtonsoft.Json.Linq;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(Startup))]

namespace NetsBrokerIntegration.DotNet
{
    public class Startup
    {
        private readonly string _clientId = ConfigurationManager.AppSettings["ClientId"];
        private readonly string _redirectUri = ConfigurationManager.AppSettings["RedirectUri"];
        private readonly string _postLogoutRedirectUri = ConfigurationManager.AppSettings["PostLogoutRedirectUri"];
        private readonly string _authority = ConfigurationManager.AppSettings["Authority"];
        private readonly string _clientSecret = ConfigurationManager.AppSettings["ClientSecret"];
        private readonly string _promptMode = ConfigurationManager.AppSettings["PromptMode"];

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Home/Signin"),
                CookieSameSite = SameSiteMode.None
            });
            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            _ = app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Authority = _authority,
                RedirectUri = _redirectUri,
                PostLogoutRedirectUri = _postLogoutRedirectUri,
                ResponseType = OpenIdConnectResponseType.CodeIdToken,
                Scope = "openid mitid nemid",
                SaveTokens = true,

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = notification =>
                    {
                        if (string.Equals(notification.ProtocolMessage.Error, "access_denied", StringComparison.Ordinal))
                        {
                            notification.HandleResponse();

                            notification.Response.Redirect("/Home/Error" + notification.Request.QueryString);
                        }

                        return Task.FromResult(0);
                    },

                    // Retrieve an access token from the remote token endpoint
                    // using the authorization code received during the current request.
                    AuthorizationCodeReceived = async notification =>
                    {
                        using (var client = new HttpClient())
                        {
                            var configuration = await notification.Options.ConfigurationManager.GetConfigurationAsync(notification.Request.CallCancelled);
                            var tokenEndpointResult = await ExchangeCodeForTokens(notification, client, configuration);

                            var idToken = tokenEndpointResult.Value<string>(OpenIdConnectParameterNames.IdToken);
                            var accessToken = tokenEndpointResult.Value<string>(OpenIdConnectParameterNames.AccessToken);

                            notification.AuthenticationTicket.Identity.AddClaim(new Claim(
                                type: OpenIdConnectParameterNames.IdToken,
                                value: idToken));
                            notification.AuthenticationTicket.Identity.AddClaim(new Claim(
                                type: OpenIdConnectParameterNames.AccessToken,
                                value: accessToken));

                            var userInfoEndpointResult = await UserInfoEndpointClaims(notification, client, configuration, accessToken);

                            //Security note: It is important to verify that the sub claim from ID token matches the sub claim in the UserInfo response
                            var userinfoSub = userInfoEndpointResult["sub"].Value<string>();
                            var idTokenSub = notification.AuthenticationTicket.Identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                            if (userinfoSub == idTokenSub)
                            {
                                //add claims from UserInfo endpoint to identity
                                foreach(var entry in userInfoEndpointResult)
                                {
                                    if(!notification.AuthenticationTicket.Identity.HasClaim(c => c.Type == entry.Key))
                                    {
                                        notification.AuthenticationTicket.Identity.AddClaim(new Claim(
                                        type: entry.Key,
                                        value: entry.Value.ToString()));
                                    }
                                }
                            }
                        }
                    },

                    // Attach the id_token stored in the authentication cookie to the logout request.
                    RedirectToIdentityProvider = notification =>
                    {
                        if (notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.Logout)
                        {
                            var token = notification.OwinContext.Authentication.User?.FindFirst(OpenIdConnectParameterNames.IdToken);
                            if (token != null)
                            {
                                notification.ProtocolMessage.IdTokenHint = token.Value;
                            }
                        }

                        if (notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                        {
                            if(_promptMode != null)
                            {
                                notification.ProtocolMessage.Parameters.Add("prompt", _promptMode);
                            }
                        }

                        return Task.CompletedTask;
                    }
                }
            });
        }

        private static async Task<JObject> ExchangeCodeForTokens(AuthorizationCodeReceivedNotification notification, HttpClient client, OpenIdConnectConfiguration configuration)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, configuration.TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    [OpenIdConnectParameterNames.ClientId] = notification.Options.ClientId,
                    [OpenIdConnectParameterNames.ClientSecret] = notification.Options.ClientSecret,
                    [OpenIdConnectParameterNames.Code] = notification.ProtocolMessage.Code,
                    [OpenIdConnectParameterNames.GrantType] = "authorization_code",
                    [OpenIdConnectParameterNames.RedirectUri] = notification.Options.RedirectUri
                })
            };

            var response = await client.SendAsync(request, notification.Request.CallCancelled);
            response.EnsureSuccessStatusCode();

            var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            return payload;
        }


        private static async Task<JObject> UserInfoEndpointClaims(AuthorizationCodeReceivedNotification notification, HttpClient client, OpenIdConnectConfiguration configuration, string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, configuration.UserInfoEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await client.SendAsync(request, notification.Request.CallCancelled);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var payload = JObject.Parse(responseString);
            return payload;
        }
    }
}
