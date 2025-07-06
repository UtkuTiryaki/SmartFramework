using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SmartFramework.CQRS;

public static class CqrsExtensions
{
    public static IServiceCollection AddCqrs(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length is 0) throw new ArgumentNullException(nameof(assemblies), "Assemblies cannot be null or empty.");

        services.AddScoped<IMessageRouter, MessageRouter>();
        services.RegisterHandlersWithResponse(assemblies, typeof(ICommandHandler<,>), typeof(ICommand<>));
        services.RegisterHandlersWithResponse(assemblies, typeof(IQueryHandler<,>), typeof(IQuery<>));
        services.RegisterHandlersWithResponse(assemblies, typeof(IStreamQueryHandler<,>), typeof(IStreamQuery<>));
        services.RegisterHandlers(assemblies, typeof(ICommandHandler<>), typeof(ICommand<>));
        services.RegisterHandlers(assemblies, typeof(IEventHandler<>), typeof(IEvent));
        
        return services;
    }
    
    private static IServiceCollection RegisterHandlers(this IServiceCollection services, Assembly[] assemblies, Type handlerInterfaceType, Type messageType)
    {
        var handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces().Any(i => IsHandlerInterface(i, handlerInterfaceType)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => IsHandlerInterface(i, handlerInterfaceType))
                .ToList();

            foreach (var interfaceType in interfaces)
            {
                var messageTypeParam = interfaceType.GetGenericArguments()[0];
                if (messageType.IsAssignableFrom(messageTypeParam))
                {
                    services.AddTransient(interfaceType, handlerType);
                }
            }
        }
        
        return services;
    }
    
    private static IServiceCollection RegisterHandlersWithResponse(this IServiceCollection services, Assembly[] assemblies, Type handlerInterfaceType, Type messageType)
    {
        var handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces().Any(i => IsHandlerInterfaceWithResponse(i, handlerInterfaceType)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => IsHandlerInterfaceWithResponse(i, handlerInterfaceType))
                .ToList();

            foreach (var interfaceType in interfaces)
            {
                var messageTypeParam = interfaceType.GetGenericArguments()[0];
                var isMessageHandlerInterface = messageType.IsGenericTypeDefinition
                    ? messageTypeParam.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == messageType)
                    : messageType.IsAssignableFrom(messageTypeParam);

                if (isMessageHandlerInterface)
                {
                    services.AddTransient(interfaceType, handlerType);
                }
            }
        }
        
        return services;
    }
    
    private static bool IsHandlerInterface(Type type, Type handlerInterfaceType)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == handlerInterfaceType;
    }

    private static bool IsHandlerInterfaceWithResponse(Type type, Type handlerInterfaceType)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == handlerInterfaceType;
    }
}