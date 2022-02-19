using System;
using AdsApi.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using FluentValidation;
using FluentValidation.AspNetCore;
using AdsApi.Validators;
using Shared.Models;
using AdsApi.Helpers;
using Shared.Helpers;
using Amazon.S3;
using Amazon;

namespace AdsApi
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
            services
                .AddControllers()
                .AddFluentValidation();

            // Add fluent Validators
            services.AddTransient<IValidator<Ad>, AdValidator>();

            // Add s3
            var s3Client = new AmazonS3Client(RegionEndpoint.USEast1);
            services.AddSingleton<IAmazonS3>(s3Client);

            services.AddSingleton<AdsRepository>();
            services.AddSingleton<FeaturesRepository>();
            services.AddSingleton<AdTypesRepository>();

            var esSettings = new ConnectionSettings(new Uri(Configuration.GetSection("ElasticSearchSettings")["ConnectionString"]))
                .DefaultIndex("classified_ads")
                .EnableDebugMode()
                .DisableDirectStreaming()
                .PrettyJson()
                .OnRequestCompleted(apiCallDetails =>
                {
                    Console.WriteLine($"ES: {apiCallDetails.DebugInformation.ToString()}");
                });
            var esClient = new ElasticClient(esSettings);

            services.AddSingleton<IElasticClient>(esClient);

            services.AddSingleton<AdAttributesHelper>();
            services.AddSingleton<AdIndexerHelper>();

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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
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
