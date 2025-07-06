using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SmartFramework.Domain.Persistence;

public static class DomainPersistenceExtensions
{
    public static IServiceCollection AddDomainPersistence<TContext>(this IServiceCollection services, string connectionString) where TContext : AbstractContext
    {
        services.AddDbContext<TContext>(options => options.UseNpgsql(connectionString));
        return services;
    }

    public static IApplicationBuilder UseEventualConsistencyMiddleware<TContext>(this IApplicationBuilder app)
        where TContext : AbstractContext
    {
        return app.UseMiddleware<EventualConsistencyMiddleware<TContext>>();
    } 
}