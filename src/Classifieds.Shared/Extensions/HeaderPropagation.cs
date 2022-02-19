using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Shared.Extensions
{
    public static class HeaderPropagationExtensions
    {
        public static IServiceCollection AddHeaderPropagation(this IServiceCollection services, Action<HeaderPropagationOptions> configure)
        {
            services.AddHttpContextAccessor();
            services.Configure(configure);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, HeaderPropagationMessageHandlerBuilderFilter>());
            return services;
        }

        public static IHttpClientBuilder AddHeaderPropagation(this IHttpClientBuilder builder, Action<HeaderPropagationOptions> configure)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.Configure(configure);
            builder.AddHttpMessageHandler((sp) =>
            {
                var options = sp.GetRequiredService<IOptions<HeaderPropagationOptions>>();
                var contextAccessor = sp.GetRequiredService<IHttpContextAccessor>();

                return new HeaderPropagationMessageHandler(options.Value, contextAccessor);
            });

            return builder;
        }
    }

    internal class HeaderPropagationMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly HeaderPropagationOptions _options;
        private readonly IHttpContextAccessor _contextAccessor;

        public HeaderPropagationMessageHandlerBuilderFilter(IOptions<HeaderPropagationOptions> options, IHttpContextAccessor contextAccessor)
        {
            _options = options.Value;
            _contextAccessor = contextAccessor;
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return builder =>
            {
                builder.AdditionalHandlers.Add(new HeaderPropagationMessageHandler(_options, _contextAccessor));
                next(builder);
            };
        }
    }

    public class HeaderPropagationOptions
    {
        public IList<string> HeaderNames { get; set; } = new List<string>();
    }

    public class HeaderPropagationMessageHandler : DelegatingHandler
    {
        private readonly HeaderPropagationOptions _options;
        private readonly IHttpContextAccessor _contextAccessor;
        public HeaderPropagationMessageHandler(HeaderPropagationOptions options, IHttpContextAccessor contextAccessor)
        {
            _options = options;
            _contextAccessor = contextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (_contextAccessor.HttpContext != null)
            {
                // REVIEW: This logic likely gets more fancy and allows mapping headers in more complex ways
                foreach (var headerName in _options.HeaderNames)
                {
                    // Get the incoming header value
                    var headerValue = _contextAccessor.HttpContext.Request.Headers[headerName];
                    if (StringValues.IsNullOrEmpty(headerValue))
                    {
                        continue;
                    }

                    request.Headers.TryAddWithoutValidation(headerName, (string[])headerValue);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}