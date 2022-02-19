using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProfilesApi.Repositories;
using Amazon.S3;
using Amazon;
using Nest;
using System;

namespace ProfilesApi
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        // readonly ILogger<Startup> _logger;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            // add comment
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddControllers();
            services.AddSingleton<ProfilesRepository>();

            // Add s3
            var s3Client = new AmazonS3Client(RegionEndpoint.USEast1);
            services.AddSingleton<IAmazonS3>(s3Client);

            // Add es
            var esSettings = new ConnectionSettings(new Uri("https://i12sa6lx3y:v75zs8pgyd@classifieds-4537380016.us-east-1.bonsaisearch.net:443"))
                .DefaultIndex("classified_profiles")
                .EnableDebugMode()
                .DisableDirectStreaming()
                .PrettyJson()
                /*.OnRequestDataCreated(data => {
                    Console.WriteLine($"ES: {data.PostData.ToString()}");
                })*/
                .OnRequestCompleted(apiCallDetails =>
                {
                    // _logger.LogDebug(apiCallDetails.ToString());
                    Console.WriteLine($"ES: {apiCallDetails.DebugInformation.ToString()}");
                });
            var esClient = new ElasticClient(esSettings);
            services.AddSingleton<IElasticClient>(esClient);

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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(MyAllowSpecificOrigins);
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
