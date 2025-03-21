using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.AdaptiveCard.Util.Abstract;

namespace Soenneker.AdaptiveCard.Util.Registrars;

/// <summary>
/// A utility library for Adaptive Card construction
/// </summary>
public static class AdaptiveCardUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IAdaptiveCardUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddAdaptiveCardUtilAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<IAdaptiveCardUtil, AdaptiveCardUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IAdaptiveCardUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddAdaptiveCardUtilAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<IAdaptiveCardUtil, AdaptiveCardUtil>();

        return services;
    }
}
