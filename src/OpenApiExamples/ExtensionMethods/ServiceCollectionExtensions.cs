using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenApiExamples.Abstractions;
using OpenApiExamples.Services;

namespace OpenApiExamples.ExtensionMethods;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApiExamples(
        this IServiceCollection services,
        Action<OpenApiExamplesOptions>? configureOptions = null
    )
    {
        services
            .AddOptions<OpenApiExamplesOptions>()
            .Configure<IServiceProvider>((options, sp) =>
            {
                var logger = sp.GetService<ILoggerFactory>()?
                    .CreateLogger("OpenApiExamples");

                var formatters = sp.GetServices<IOpenApiExamplesFormatter>();
                foreach (var formatter in formatters)
                {
                    foreach (var contentType in formatter.SupportedContentTypes)
                    {
                        if (options.Formatters.ContainsKey(contentType))
                        {
                            logger?.LogWarning(
                                "Formatter for content type {contentType} already exists. Overriding with {formatterName}.",
                                contentType,
                                formatter.GetType().Name
                            );
                        }

                        options.Formatters[contentType] = formatter;
                    }
                }

                // The app's serializer is the one ASP.NET Core generates schemas from, so examples follow
                // it by default and land in the same shape as the schema printed above them. Copied, not
                // shared: System.Text.Json freezes an instance the first time it serializes anything, and
                // configureOptions below may still want to add a converter.
                var appJsonOptions = sp.GetRequiredService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
                options.JsonSerializerOptions = new JsonSerializerOptions(appJsonOptions.Value.SerializerOptions);

                configureOptions?.Invoke(options);
            });


        services.AddTransient<IOpenApiExamplesWriter, OpenApiExamplesWriter>();
        services.AddTransient<IOpenApiExamplesFormatter, JsonOpenApiExamplesFormatter>();
        services.AddTransient<IOpenApiExamplesFormatter, XmlOpenApiExamplesFormatter>();
        return services;
    }

    public static IServiceCollection AddExamplesFormatter<T>(
        this IServiceCollection services)
        where T : class, IOpenApiExamplesFormatter
    {
        // ImplementationType, not ServiceType: formatters are registered against IOpenApiExamplesFormatter, so
        // ServiceType is never the concrete type and this guard would never fire.
        if (services.Any(s => s.ImplementationType == typeof(T)))
        {
            throw new InvalidOperationException($"Formatter type '{typeof(T).Name}' already exists.");
        }

        services.AddTransient<IOpenApiExamplesFormatter, T>();
        return services;
    }
}