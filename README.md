# integration-client-dotnet

.Net 4.7 demo.

.Net MVC does not support Code Authorization flow, but it is possible to do a Hybrid flow, with some manual additions.

See this stackoverflow for example: https://stackoverflow.com/questions/33661935/owin-middleware-for-openid-connect-code-flow-flow-type-authorizationcode

## Samesite
For .Net 4.5, see: https://stackoverflow.com/questions/62576470/how-to-set-samesite-value-to-none-in-net-4-5-2 

## Umbraco
See https://www.zeroseven.com.au/Blog/2017/May-2017/Developer-Tip-Aspnet-Identity for inspiration.

## .Net 4.5
```
<package id="Microsoft.AspNet.Identity.Core" version="2.2.2" targetFramework="net451" />
<package id="Microsoft.AspNet.Identity.Owin" version="2.2.2" targetFramework="net451" />
<package id="Microsoft.AspNet.Mvc" version="5.2.7" targetFramework="net451" />
<package id="Microsoft.AspNet.Razor" version="3.2.7" targetFramework="net451" />
<package id="Microsoft.AspNet.WebPages" version="3.2.7" targetFramework="net451" />
<package id="Microsoft.CodeDom.Providers.DotNetCompilerPlatform" version="2.0.1.0" targetFramework="net451" />
<package id="Microsoft.IdentityModel.JsonWebTokens" version="6.12.2" targetFramework="net452" />
<package id="Microsoft.IdentityModel.Logging" version="6.12.2" targetFramework="net452" />
<package id="Microsoft.IdentityModel.Protocols" version="6.12.2" targetFramework="net452" />
<package id="Microsoft.IdentityModel.Protocols.OpenIdConnect" version="6.12.2" targetFramework="net452" />
<package id="Microsoft.IdentityModel.Tokens" version="6.12.2" targetFramework="net452" />
<package id="Microsoft.Owin" version="4.2.0" targetFramework="net452" />
<package id="Microsoft.Owin.Host.SystemWeb" version="4.0.1" targetFramework="net451" />
<package id="Microsoft.Owin.Security" version="4.2.0" targetFramework="net452" />
<package id="Microsoft.Owin.Security.Cookies" version="4.0.1" targetFramework="net451" />
<package id="Microsoft.Owin.Security.OAuth" version="4.0.1" targetFramework="net451" />
<package id="Microsoft.Owin.Security.OpenIdConnect" version="4.2.0" targetFramework="net452" />
<package id="Microsoft.Web.Infrastructure" version="1.0.0.0" targetFramework="net451" />
<package id="Modernizr" version="2.6.2" targetFramework="net451" />
<package id="Newtonsoft.Json" version="12.0.2" targetFramework="net452" />
<package id="Owin" version="1.0" targetFramework="net451" />
<package id="System.IdentityModel.Tokens.Jwt" version="6.12.2" targetFramework="net452" /> 
```

## MitID testusers
See https://pp.mitid.dk/test-tool/frontend/#/create-identity for administration of MitID test users.

## Postman collection for simple OIDC requests
1: Get authorization code via a flow, e.g. using https://openidconnect.net (see postman example URL in postman collection below)
2: Use postman to invoke Token endpoint and Userinfo endpoint

https://github.com/Signaturgruppen-A-S/nets-eID-broker-demo/blob/main/postman/Nets%20eID%20Broker%20(PP).postman_collection.json
