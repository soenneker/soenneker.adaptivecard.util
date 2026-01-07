using System;
using System.Reflection;

namespace Soenneker.AdaptiveCard.Util.Utils;

internal sealed class TableMeta<T>
{
    public required PropertyInfo[] Properties { get; init; }
    public required Func<T, object?>[] Getters { get; init; }
}