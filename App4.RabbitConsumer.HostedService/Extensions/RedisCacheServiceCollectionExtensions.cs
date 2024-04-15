using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisCacheServiceCollectionExtensions
    {
        public static void AddRedisConnectionMultiplexer(this IServiceCollection services,
            string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            var multiplexer = ConnectionMultiplexer.Connect(connectionString) as IConnectionMultiplexer;
            services.TryAddSingleton(multiplexer);
        }
    }
}
