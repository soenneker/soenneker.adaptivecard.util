using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AdaptiveCards;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soenneker.AdaptiveCard.Util.Abstract;
using Soenneker.AdaptiveCard.Util.Utils;
using Soenneker.Extensions.DateTimeOffsets;
using Soenneker.Extensions.String;
using Soenneker.Utils.Environment;
using Soenneker.Utils.TimeZones;

namespace Soenneker.AdaptiveCard.Util;

/// <inheritdoc cref="IAdaptiveCardUtil"/>
public sealed class AdaptiveCardUtil : IAdaptiveCardUtil
{
    private readonly ILogger<AdaptiveCardUtil> _logger;

    // Cache these; reading config repeatedly is extra work and can allocate depending on providers.
    private readonly string? _environment;
    private readonly string? _projectName;

    private const int _maxErrorBytes = 27 * 1024;
    private const int _truncateChars = 5000;

    // Static UTF8 instance (Encoding.UTF8 property is already cached, but keep local reference).
    private static readonly Encoding _utf8 = Encoding.UTF8;

    // Avoid per-card anonymous object allocation; keep same JSON shape ("width": "Full").
    private static readonly MsTeamsCardProps _msTeamsProps = new();

    public AdaptiveCardUtil(ILogger<AdaptiveCardUtil> logger, IConfiguration config)
    {
        _logger = logger;
        _environment = config.GetValue<string>("Environment");
        _projectName = config.GetValue<string>("ProjectName");
    }

    public AdaptiveCards.AdaptiveCard Build(string title, string? summary = null, Dictionary<string, string?>? facts = null, Exception? exception = null,
        string? additionalBody = null)
    {
        AdaptiveCards.AdaptiveCard card = CreateCard();

        AddHeader(card, title, summary);
        AddFacts(card, facts);

        // exception.ToString() can be big; we only materialize if exception exists.
        if (exception is not null)
            AddTextBlock(card, exception.ToString());

        AddTextBlock(card, additionalBody);
        AddFooter(card);

        return card;
    }

    public AdaptiveCards.AdaptiveCard BuildTable<T>(string title, List<T> items, string? summary = null)
    {
        AdaptiveCards.AdaptiveCard card = CreateCard();

        if (items.Count > 0)
        {
            TableMeta<T> meta = TableCache<T>.Meta;
            AddTableHeader(card, title, summary, meta.Properties);

            // Prefer for-loop over foreach to avoid enumerator overhead (small, but free).
            for (int i = 0; i < items.Count; i++)
            {
                T item = items[i];
                var row = new AdaptiveColumnSet();

                Func<T, object?>[] getters = meta.Getters;

                for (int p = 0; p < getters.Length; p++)
                {
                    object? raw = getters[p](item);
                    string value = raw?.ToString() ?? string.Empty;

                    row.Columns.Add(new AdaptiveColumn
                    {
                        Items =
                        [
                            new AdaptiveTextBlock(value) { Wrap = true }
                        ]
                    });
                }

                card.Body.Add(row);
            }
        }

        AddFooter(card);
        return card;
    }

    private static AdaptiveCards.AdaptiveCard CreateCard() => new(new AdaptiveSchemaVersion(1, 2))
    {
        AdditionalProperties =
        {
            ["msteams"] = _msTeamsProps
        }
    };

    private static void AddHeader(AdaptiveCards.AdaptiveCard card, string title, string? summary)
    {
        card.Body.Add(new AdaptiveTextBlock
        {
            Text = title,
            Size = AdaptiveTextSize.Medium,
            Weight = AdaptiveTextWeight.Bolder,
            Wrap = true
        });

        if (!summary.IsNullOrEmpty())
        {
            card.Body.Add(new AdaptiveTextBlock
            {
                Text = summary!,
                Size = AdaptiveTextSize.Small,
                Wrap = true
            });
        }
    }

    private static void AddFacts(AdaptiveCards.AdaptiveCard card, Dictionary<string, string?>? facts)
    {
        if (facts is null || facts.Count == 0)
            return;

        var factSet = new AdaptiveFactSet();

        foreach (KeyValuePair<string, string?> kvp in facts)
        {
            string? value = kvp.Value;

            if (!value.IsNullOrEmpty())
                factSet.Facts.Add(new AdaptiveFact(kvp.Key, value));
        }

        card.Body.Add(factSet);
    }

    private void AddTextBlock(AdaptiveCards.AdaptiveCard card, string? content)
    {
        if (content.IsNullOrEmpty())
            return;

        // Fast paths to avoid GetByteCount() in the common case.
        // Worst-case UTF-8 is 4 bytes per char. If we're under maxBytes/4, we are definitely safe.
        int len = content.Length;
        if (len > (_maxErrorBytes / 4))
        {
            // If length alone already exceeds max bytes, it must be too big (UTF8 bytes >= chars).
            bool tooBig = len > _maxErrorBytes;

            // Otherwise compute exact bytes only when we have to.
            if (!tooBig)
            {
                int textBytes = _utf8.GetByteCount(content);
                tooBig = textBytes > _maxErrorBytes;

                if (tooBig)
                    _logger.LogError("Truncating large text block for MS Teams. Size: {Size} bytes", textBytes);
            }
            else
            {
                // We know it's too big without counting bytes; still log something useful.
                _logger.LogError("Truncating large text block for MS Teams. Content length: {Length} chars", len);
            }

            if (tooBig)
                content = content[..Math.Min(_truncateChars, len)];
        }

        card.Body.Add(new AdaptiveTextBlock
        {
            Text = content,
            Size = AdaptiveTextSize.Small,
            Wrap = true
        });
    }

    private static void AddTableHeader(AdaptiveCards.AdaptiveCard card, string title, string? summary, PropertyInfo[] properties)
    {
        AddHeader(card, title, summary);

        var headerRow = new AdaptiveColumnSet { Spacing = AdaptiveSpacing.ExtraLarge };

        for (int i = 0; i < properties.Length; i++)
        {
            PropertyInfo prop = properties[i];

            headerRow.Columns.Add(new AdaptiveColumn
            {
                Items =
                [
                    new AdaptiveTextBlock(prop.Name)
                    {
                        Wrap = true,
                        Weight = AdaptiveTextWeight.Bolder
                    }
                ]
            });
        }

        card.Body.Add(headerRow);
    }

    private void AddFooter(AdaptiveCards.AdaptiveCard card)
    {
        AddFooterText(card, _environment);
        AddFooterText(card, _projectName);

        try
        {
            string machineName = EnvironmentUtil.GetMachineName();
            AddFooterText(card, machineName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machine name");
        }

        string timestamp = DateTimeOffset.UtcNow.ToTzDateTimeFormat(Tz.Eastern);
        AddFooterText(card, timestamp);
    }

    private static void AddFooterText(AdaptiveCards.AdaptiveCard card, string? text)
    {
        if (text.IsNullOrEmpty())
            return;

        card.Body.Add(new AdaptiveTextBlock
        {
            Text = text,
            Size = AdaptiveTextSize.Small,
            IsSubtle = true,
            Spacing = AdaptiveSpacing.Small
        });
    }
}