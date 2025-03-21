using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AdaptiveCards;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soenneker.AdaptiveCard.Util.Abstract;
using Soenneker.Extensions.DateTime;
using Soenneker.Extensions.String;
using Soenneker.Utils.Environment;
using Soenneker.Utils.TimeZones;

namespace Soenneker.AdaptiveCard.Util;

///<inheritdoc cref="IAdaptiveCardUtil"/>
public class AdaptiveCardUtil : IAdaptiveCardUtil
{
    private readonly ILogger<AdaptiveCardUtil> _logger;
    private readonly IConfiguration _config;
    private const int _maxErrorBytes = 27 * 1024;

    public AdaptiveCardUtil(ILogger<AdaptiveCardUtil> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public AdaptiveCards.AdaptiveCard Build(string title, string? summary = null, Dictionary<string, string?>? facts = null, Exception? exception = null,
        string? additionalBody = null)
    {
        AdaptiveCards.AdaptiveCard card = CreateCard();

        AddHeader(card, title, summary);
        AddFacts(card, facts);
        AddTextBlock(card, exception?.ToString());
        AddTextBlock(card, additionalBody);
        AddFooter(card);

        return card;
    }

    public AdaptiveCards.AdaptiveCard BuildTable<T>(string title, List<T> items, string? summary = null)
    {
        AdaptiveCards.AdaptiveCard card = CreateCard();

        if (items.Count > 0)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            AddTableHeader(card, title, summary, properties);

            foreach (T item in items)
            {
                var row = new AdaptiveColumnSet();

                foreach (PropertyInfo prop in properties)
                {
                    string value = prop.GetValue(item)?.ToString() ?? string.Empty;

                    row.Columns.Add(new AdaptiveColumn
                    {
                        Items =
                        [
                            new AdaptiveTextBlock(value) {Wrap = true}
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
            ["msteams"] = new {width = "Full"}
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
        if (facts == null || facts.Count < 1)
            return;

        var factSet = new AdaptiveFactSet();

        foreach ((string key, string? value) in facts)
        {
            if (!value.IsNullOrEmpty())
                factSet.Facts.Add(new AdaptiveFact(key, value));
        }

        card.Body.Add(factSet);
    }

    private void AddTextBlock(AdaptiveCards.AdaptiveCard card, string? content)
    {
        if (content.IsNullOrEmpty())
            return;

        int textBytes = Encoding.UTF8.GetByteCount(content);
        if (textBytes > _maxErrorBytes)
        {
            _logger.LogError("Truncating large text block for MS Teams. Size: {Size} bytes", textBytes);
            content = content[..Math.Min(5000, content.Length)];
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

        var headerRow = new AdaptiveColumnSet {Spacing = AdaptiveSpacing.ExtraLarge};

        foreach (PropertyInfo prop in properties)
        {
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
        var environment = _config.GetValue<string>("Environment");
        var projectName = _config.GetValue<string>("ProjectName");

        AddFooterText(card, environment);
        AddFooterText(card, projectName);

        try
        {
            string machineName = EnvironmentUtil.GetMachineName();
            AddFooterText(card, machineName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machine name");
        }

        string timestamp = DateTime.UtcNow.ToTzDateTimeFormat(Tz.Eastern);
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