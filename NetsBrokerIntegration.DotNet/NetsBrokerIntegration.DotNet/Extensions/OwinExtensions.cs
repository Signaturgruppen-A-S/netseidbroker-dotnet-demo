using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetsBrokerIntegration.DotNet.Extensions
{
    public static class OwinExtensions
    {
        public static async Task<IDictionary<string, string>> GetExternalAuthenticationProperties(IOwinContext owinContext)
        {
            var externalLogin = await owinContext.Authentication.AuthenticateAsync(DefaultAuthenticationTypes.ExternalCookie);
            return externalLogin.Properties.Dictionary;
        }
    }
}