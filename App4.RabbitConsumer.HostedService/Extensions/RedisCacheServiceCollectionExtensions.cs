using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

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

        public static void AddRedisFusionCacheService(this IServiceCollection services,
            string connectionString,
            string instanceName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(instanceName);
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            services.AddFusionCache()
                .WithSerializer(new FusionCacheSystemTextJsonSerializer())
                .WithBackplane(
                    new RedisBackplane(new RedisBackplaneOptions { Configuration = connectionString })
                );
        }
    }
}
