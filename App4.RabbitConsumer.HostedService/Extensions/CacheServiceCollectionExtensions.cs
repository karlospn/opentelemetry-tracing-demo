using System;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CacheServiceCollectionExtensions
    {
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
