[![](https://img.shields.io/nuget/v/soenneker.adaptivecard.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.adaptivecard.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.adaptivecard.util/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.adaptivecard.util/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.adaptivecard.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.adaptivecard.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.adaptivecard.util/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.adaptivecard.util/actions/workflows/codeql.yml)

# Soenneker.AdaptiveCard.Util

A small builder for creating consistent [Adaptive Cards](https://adaptivecards.io/) for Microsoft Teams, alerting pipelines, and other Adaptive Card hosts.

It provides two operations:

- `Build` creates a title/summary card with optional facts, exception details, and additional text.
- `BuildTable` turns the public properties of a list of objects into a header row and data rows.

Both operations add diagnostic footer fields when available: environment, project name, machine name, and an Eastern Time timestamp.

## Installation

```bash
dotnet add package Soenneker.AdaptiveCard.Util
```

## Registration

Register the builder with the lifetime used by your application:

```csharp
using Soenneker.AdaptiveCard.Util.Registrars;

builder.Services.AddAdaptiveCardUtilAsSingleton();
```

Scoped registration is also available:

```csharp
builder.Services.AddAdaptiveCardUtilAsScoped();
```

Both methods register `IAdaptiveCardUtil` only when it has not already been registered, so an application can supply its own implementation before calling the registrar.

## Build a diagnostic card

Inject `IAdaptiveCardUtil`, assemble the content, and pass the resulting `AdaptiveCard` to the client responsible for delivering or serializing it:

```csharp
using System.Diagnostics;
using Soenneker.AdaptiveCard.Util.Abstract;

public sealed class FailureCardFactory
{
    private readonly IAdaptiveCardUtil _cardUtil;

    public FailureCardFactory(IAdaptiveCardUtil cardUtil)
    {
        _cardUtil = cardUtil;
    }

    public AdaptiveCards.AdaptiveCard Create(
        string orderId,
        Exception exception)
    {
        var facts = new Dictionary<string, string?>
        {
            ["Order"] = orderId,
            ["Region"] = "us-east",
            ["Correlation ID"] = Activity.Current?.Id
        };

        return _cardUtil.Build(
            title: "Order processing failed",
            summary: "The order worker stopped before completion.",
            facts: facts,
            e: exception,
            additionalBody: "Review the worker logs before retrying the order.");
    }
}
```

The resulting card uses Adaptive Card schema 1.2 and includes the Microsoft Teams `msteams.width = Full` property. Fact entries with a null or empty value are omitted. Exception and additional-body text wrap automatically.

To stay within practical Teams payload limits, text blocks larger than 27 KiB in UTF-8 are truncated to at most 5,000 characters and the truncation is logged.

## Configure footer values

`Environment` and `ProjectName` are read from `IConfiguration`. For example, in `appsettings.json`:

```json
{
  "Environment": "Production",
  "ProjectName": "Order Processor"
}
```

Only non-empty configured values are included. Machine name and the current Eastern Time timestamp are added automatically.

## Build a table card

`BuildTable<T>` uses each public property name as a column heading and each property value as a cell:

```csharp
using Soenneker.AdaptiveCard.Util.Abstract;

public sealed record DeploymentRow(
    string Service,
    string Version,
    string Status);

static AdaptiveCards.AdaptiveCard CreateDeploymentCard(
    IAdaptiveCardUtil cardUtil)
{
    var rows = new List<DeploymentRow>
    {
        new("API", "4.12.0", "Healthy"),
        new("Worker", "4.12.0", "Degraded")
    };

    return cardUtil.BuildTable(
        title: "Deployment status",
        items: rows,
        summary: "Production services");
}
```

Property getters are cached per item type, so repeated calls with the same `T` do not repeat the reflection setup. Cell values are produced with `ToString()`; null values become empty cells.

Keep table models intentionally small. Adaptive Cards do not provide responsive HTML-style tables, and every public property becomes a column. Project data into a dedicated row type when you need to control column names, order, or formatting.

`BuildTable` adds its title and column headings only when `items` contains at least one row. Handle an empty result separately when the card must explicitly say that no records were found.

## API

| Method | Purpose |
| --- | --- |
| `Build(string title, string? summary, Dictionary<string, string?>? facts, Exception? e, string? additionalBody)` | Creates a general diagnostic or informational card. |
| `BuildTable<T>(string title, List<T> items, string? summary)` | Creates a property-based table card from a typed list. |
| `AddAdaptiveCardUtilAsSingleton()` | Registers `IAdaptiveCardUtil` as a singleton. |
| `AddAdaptiveCardUtilAsScoped()` | Registers `IAdaptiveCardUtil` as scoped. |
