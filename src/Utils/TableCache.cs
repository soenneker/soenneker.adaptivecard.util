using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Soenneker.AdaptiveCard.Util.Utils;

/// <summary>
/// Per-T table metadata cache: properties + compiled getters.
/// Eliminates PropertyInfo.GetValue reflection calls in the hot loop.
/// </summary>
internal static class TableCache<T>
{
    public static readonly TableMeta<T> Meta = Create();

    private static TableMeta<T> Create()
    {
        // You could optionally filter out indexers and non-readable props; keeping simple + safe.
        PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

        var getters = new Func<T, object?>[props.Length];

        for (int i = 0; i < props.Length; i++)
        {
            PropertyInfo p = props[i];

            // Build: (T x) => (object?)x.Prop
            ParameterExpression param = Expression.Parameter(typeof(T), "x");
            MemberExpression propAccess = Expression.Property(param, p);

            UnaryExpression box = Expression.Convert(propAccess, typeof(object));
            getters[i] = Expression.Lambda<Func<T, object?>>(box, param)
                                   .Compile();
        }

        return new TableMeta<T>
        {
            Properties = props,
            Getters = getters
        };
    }
}