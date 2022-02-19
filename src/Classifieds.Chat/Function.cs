using Microsoft.AspNetCore.Hosting;
using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Http.Features;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.AspNetCoreServer.Internal;
using Newtonsoft.Json;
using System.Collections.Generic;
using Chat.Models;
using System.Text;
using System.IO;

namespace Chat
{
    public class Function : APIGatewayProxyFunction
    {

        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .UseStartup<Startup>();
        }

        protected override void MarshallRequest(InvokeFeatures features, APIGatewayProxyRequest apiGatewayRequest, ILambdaContext lambdaContext)
        {

            if(apiGatewayRequest.HttpMethod == null)
            {
                apiGatewayRequest.Path = "/";
                apiGatewayRequest.Resource = "/";
                apiGatewayRequest.HttpMethod = "GET";

                lambdaContext.Logger.LogLine("REQUEST: " + JsonConvert.SerializeObject(apiGatewayRequest));
                lambdaContext.Logger.LogLine("CONTEXT: " + JsonConvert.SerializeObject(lambdaContext));
                lambdaContext.Logger.LogLine($"Route Key: {apiGatewayRequest.RequestContext.RouteKey}");

                if (apiGatewayRequest.QueryStringParameters != null && apiGatewayRequest.QueryStringParameters.ContainsKey("token"))
                {
                    var token = apiGatewayRequest.QueryStringParameters["token"];
                    apiGatewayRequest.Headers.Add("Authorization", $"Bearer {token}");
                    apiGatewayRequest.MultiValueHeaders.Add("Authorization", new List<string> { $"Bearer {token}" });
                }

                if (apiGatewayRequest.RequestContext.RouteKey == "messages")
                {
                    var evt = JsonConvert.DeserializeObject<MessagesEvent>(apiGatewayRequest.Body);
                    apiGatewayRequest.QueryStringParameters = new Dictionary<string, string> {
                    { "recipientId", evt.RecipientId }
                };
                    apiGatewayRequest.MultiValueQueryStringParameters = new Dictionary<string, IList<string>> {
                    { "recipientId", new List<string> { evt.RecipientId } }
                };
                }
            }

            lambdaContext.Logger.LogLine("REQUEST: " + JsonConvert.SerializeObject(apiGatewayRequest));
            lambdaContext.Logger.LogLine("CONTEXT: " + JsonConvert.SerializeObject(lambdaContext));

            base.MarshallRequest(features, apiGatewayRequest, lambdaContext);
        }

        protected override void PostMarshallRequestFeature(IHttpRequestFeature aspNetCoreRequestFeature, APIGatewayProxyRequest apiGatewayRequest, ILambdaContext lambdaContext)
        {
            if(apiGatewayRequest.RequestContext.EventType != null)
            {
                if (apiGatewayRequest.RequestContext.EventType == "CONNECT")
                {
                    aspNetCoreRequestFeature.Path = "/connect";

                }
                else if (apiGatewayRequest.RequestContext.EventType == "DISCONNECT")
                {
                    aspNetCoreRequestFeature.Path = "/disconnect";

                }
                else if (apiGatewayRequest.RequestContext.RouteKey == "conversations")
                {
                    aspNetCoreRequestFeature.Path = "/chatconversations";

                }
                else if (apiGatewayRequest.RequestContext.RouteKey == "message")
                {
                    var evt = JsonConvert.DeserializeObject<MessageEvent>(apiGatewayRequest.Body);
                    var byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evt.Message));
                    var stream = new MemoryStream(byteArray);
                    aspNetCoreRequestFeature.Path = "/chatmessage";
                    aspNetCoreRequestFeature.Body = stream;
                    aspNetCoreRequestFeature.Headers.Add("Content-Type", "application/json");
                    aspNetCoreRequestFeature.Headers.Add("Accepts", "application/json");
                    aspNetCoreRequestFeature.Method = "POST";

                }
                else if (apiGatewayRequest.RequestContext.RouteKey == "messages")
                {
                    aspNetCoreRequestFeature.Path = $"/chatmessages";
                }
            }

            if (apiGatewayRequest.RequestContext.EventType == null)
            {
                aspNetCoreRequestFeature.PathBase = "/chat/";
                lambdaContext.Logger.LogLine("CONTEXT: " + JsonConvert.SerializeObject(lambdaContext));

                // The minus one is ensure path is always at least set to `/`
                aspNetCoreRequestFeature.Path =
                    aspNetCoreRequestFeature.Path.Substring(aspNetCoreRequestFeature.PathBase.Length - 1);
                lambdaContext.Logger.LogLine($"Path: {aspNetCoreRequestFeature.Path}, PathBase: {aspNetCoreRequestFeature.PathBase}");
            }

        }

    }
}