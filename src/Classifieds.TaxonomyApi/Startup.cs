using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaxonomyApi.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using TaxonomyApi.Validators;
using Shared.Models;
using Amazon.S3;
using Amazon;

namespace TaxonomyApi
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
            services.AddControllers()
                .AddFluentValidation();

            // Add fluent Validators
            services.AddTransient<IValidator<Vocabulary>, VocabularyValidator>();
            services.AddTransient<IValidator<Term>, TermValidator>();

            // Add s3
            var s3Client = new AmazonS3Client(RegionEndpoint.USEast1);
            services.AddSingleton<IAmazonS3>(s3Client);

            // Add Repos
            services.AddSingleton<VocabulariesRepository>();

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
            app.UseCors(MyAllowSpecificOrigins);
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
