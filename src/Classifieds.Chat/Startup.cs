using System;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Cassandra;
using Cassandra.Mapping;
using Chat.Mappings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Chat.Repositories;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Diagnostics;
using Amazon.CognitoIdentityProvider;

namespace Chat
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddControllers();
            services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_z8PhK3D8V";
                    options.TokenValidationParameters = new TokenValidationParameters {
                        // IssuerSigningKey = SigningKey
                        ValidIssuer = "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_z8PhK3D8V",
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateLifetime = true,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.FromMinutes(0)
                    };
                });
            // Diagnostics.CassandraTraceSwitch.Level = System.Diagnostics.TraceLevel.Info;
            // Trace.Listeners.Add(new ConsoleTraceListener());
            MappingConfiguration.Global.Define<ChatMappings>();
            var pathToCAFile = $"{Directory.GetCurrentDirectory()}/AmazonRootCA1.pem";
            X509Certificate2[] certs = new X509Certificate2[] { new X509Certificate2(pathToCAFile, "amazon") };
            X509Certificate2Collection certificateCollection = new X509Certificate2Collection(certs);
            var options = new Cassandra.SSLOptions(SslProtocols.Tls11, true, ValidateServerCertificate);
            options.SetCertificateCollection(certificateCollection);
            options.SetHostNameResolver((ipAddress) => "cassandra.us-east-1.amazonaws.com");
            var cluster = Cluster
                .Builder()
                .WithCredentials("username", "pass")
                .WithPort(9142)
                .AddContactPoint("cassandra.us-east-1.amazonaws.com")
                .WithSSL(options)
                .WithLoadBalancingPolicy(new TokenAwarePolicy(new DCAwareRoundRobinPolicy("us-east-1")))
                .WithExecutionProfiles(p => p
                    .WithProfile("insert", opts => opts
                        .WithConsistencyLevel(ConsistencyLevel.LocalQuorum)
                    )
                    .WithProfile("delete", opts => opts
                        .WithConsistencyLevel(ConsistencyLevel.LocalQuorum)
                    )
                )
                .Build();
            services.AddSingleton<ICluster>(cluster);
            services.AddSingleton<ChatRepository>();
            var cognitoClient = new AmazonCognitoIdentityProviderClient();
            services.AddSingleton<IAmazonCognitoIdentityProvider>(cognitoClient);
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RegisteredUser", builder =>
                {
                    builder.RequireAuthenticatedUser();
                    builder.RequireClaim("client_id", "3lkidn79q4jq8nahkecsguleij");
                });
            });
            services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                builder =>
                {
                    builder.AllowAnyOrigin();
                    builder.AllowAnyMethod();
                    builder.AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseAuthentication();
            app.UseCors(MyAllowSpecificOrigins);
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

    }
}
