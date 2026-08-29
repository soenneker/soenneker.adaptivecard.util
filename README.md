[![](https://img.shields.io/nuget/v/soenneker.adaptivecard.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.adaptivecard.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.adaptivecard.util/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.adaptivecard.util/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.adaptivecard.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.adaptivecard.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.adaptivecard.util/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.adaptivecard.util/actions/workflows/codeql.yml)

# Soenneker.AdaptiveCard.Util

Provides utility methods for building Adaptive Cards for Microsoft Teams and other services.

## Install

```bash
dotnet add package Soenneker.AdaptiveCard.Util
```

## Quick start

```csharp
using Soenneker.AdaptiveCard.Util.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddAdaptiveCardUtilAsSingleton();
```

Adds `IAdaptiveCardUtil` as a singleton service.

## What you get

- `IAdaptiveCardUtil` — Provides utility methods for building Adaptive Cards for Microsoft Teams and other services.
- `AdaptiveCardUtilRegistrar` — A utility library for Adaptive Card construction.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IAdaptiveCardUtil.Build(title, summary, facts, e, additionalBody)` | Builds a general-purpose adaptive card with optional summary, facts, exception details, and additional text. | A fully constructed `AdaptiveCard` object. |
| `IAdaptiveCardUtil.BuildTable(title, items, summary)` | Builds an adaptive card with a tabular layout based on a list of strongly typed items. | A fully constructed `AdaptiveCard` representing a table. |
| `AdaptiveCardUtilRegistrar.AddAdaptiveCardUtilAsSingleton(services)` | Adds `IAdaptiveCardUtil` as a singleton service. | The same service collection, so additional registrations can be chained. |
| `AdaptiveCardUtilRegistrar.AddAdaptiveCardUtilAsScoped(services)` | Adds `IAdaptiveCardUtil` as a scoped service. | The same service collection, so additional registrations can be chained. |
