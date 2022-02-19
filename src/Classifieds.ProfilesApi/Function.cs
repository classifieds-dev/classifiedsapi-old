using Microsoft.AspNetCore.Hosting;
using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Http.Features;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace ProfilesApi
{
    public class Function : APIGatewayProxyFunction
    {

        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .UseStartup<Startup>();
        }

        protected override void PostMarshallRequestFeature(IHttpRequestFeature aspNetCoreRequestFeature, APIGatewayProxyRequest apiGatewayRequest, ILambdaContext lambdaContext)
        {
            aspNetCoreRequestFeature.PathBase = "/profiles/";
            lambdaContext.Logger.LogLine("REQUEST: " + JsonConvert.SerializeObject(apiGatewayRequest));
            lambdaContext.Logger.LogLine("CONTEXT: " + JsonConvert.SerializeObject(lambdaContext));

            // The minus one is ensure path is always at least set to `/`
            aspNetCoreRequestFeature.Path =
                aspNetCoreRequestFeature.Path.Substring(aspNetCoreRequestFeature.PathBase.Length - 1);
            lambdaContext.Logger.LogLine($"Path: {aspNetCoreRequestFeature.Path}, PathBase: {aspNetCoreRequestFeature.PathBase}");
        }
    }
}