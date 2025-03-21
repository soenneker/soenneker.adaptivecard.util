using System;
using System.Collections.Generic;

namespace Soenneker.AdaptiveCard.Util.Abstract;

/// <summary>
/// Provides utility methods for building Adaptive Cards for Microsoft Teams and other services.
/// </summary>
public interface IAdaptiveCardUtil
{
    /// <summary>
    /// Builds a general-purpose adaptive card with optional summary, facts, exception details, and additional text.
    /// </summary>
    /// <param name="title">The title to display at the top of the card.</param>
    /// <param name="summary">An optional summary or subtitle to include.</param>
    /// <param name="facts">A dictionary of key-value pairs to show as facts.</param>
    /// <param name="e">An optional exception to include with truncated stack trace if too large.</param>
    /// <param name="additionalBody">Optional additional text to add to the card body.</param>
    /// <returns>A fully constructed <see cref="AdaptiveCard"/> object.</returns>
    AdaptiveCards.AdaptiveCard Build(string title, string? summary = null, Dictionary<string, string?>? facts = null, Exception? e = null, string? additionalBody = null);

    /// <summary>
    /// Builds an adaptive card with a tabular layout based on a list of strongly typed items.
    /// </summary>
    /// <typeparam name="T">The type of the items to include in the table.</typeparam>
    /// <param name="title">The title of the table.</param>
    /// <param name="items">The list of items to render as rows in the card.</param>
    /// <param name="summary">An optional summary or subtitle to include.</param>
    /// <returns>A fully constructed <see cref="AdaptiveCard"/> representing a table.</returns>
    AdaptiveCards.AdaptiveCard BuildTable<T>(string title, List<T> items, string? summary = null);
}