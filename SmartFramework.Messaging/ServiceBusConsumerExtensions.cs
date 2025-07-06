using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SmartFramework.Messaging;

public static class ServiceBusConsumerExtensions
{
    public static IServiceCollection AddEventConsumer<T>(this IServiceCollection services,
        Action<SqsEventConsumerOptions<T>> configure, params Assembly[] assemblies) where T : class, IIntegrationEvent
    {
        services.Configure(configure);
        services.AddIntegrationEventHandlersForType<T>(assemblies);
        services.AddHostedService<ServiceBusConsumer<T>>();
        return services;
    }
    
    public static IServiceCollection AddEventConsumer<T>(
        this IServiceCollection services,
        string queueUrl,
        Action<SqsEventConsumerOptions<T>>? configure = null,
        params Assembly[] assemblies)
        where T : class, IIntegrationEvent
    {
        services.Configure<SqsEventConsumerOptions<T>>(options =>
        {
            options.QueueUrl = queueUrl;
            configure?.Invoke(options);
        });

        services.AddIntegrationEventHandlersForType<T>(assemblies);
        services.AddHostedService<ServiceBusConsumer<T>>();
        
        return services;
    }

    private static void AddIntegrationEventHandlersForType<TEvent>(this IServiceCollection services,
        params Assembly[] assemblies) where TEvent : IIntegrationEvent
    {
        if (assemblies == null || assemblies.Length == 0) throw new ArgumentNullException(nameof(assemblies));
        
        var eventType = typeof(TEvent);
        var handlerInterfaceType = typeof(IIntegrationEventHandler<TEvent>);

        var handlerTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false, IsInterface: false } &&
                           handlerInterfaceType.IsAssignableFrom(type))
            .ToArray();

        foreach (var handlerType in handlerTypes)
        {
            services.AddSingleton(handlerInterfaceType, handlerType);
        }
    }
}