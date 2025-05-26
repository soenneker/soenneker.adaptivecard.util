using AwesomeAssertions;
using Soenneker.AdaptiveCard.Util.Abstract;
using Soenneker.Enums.JsonLibrary;
using Soenneker.Tests.FixturedUnit;
using Soenneker.Utils.Json;
using Xunit;

namespace Soenneker.AdaptiveCard.Util.Tests;

[Collection("Collection")]
public class AdaptiveCardUtilTests : FixturedUnitTest
{
    private readonly IAdaptiveCardUtil _util;

    public AdaptiveCardUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IAdaptiveCardUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }

    [Fact]
    public void AdaptiveCard_should_serialize_and_deserialize_with_newtonsoft()
    {
        AdaptiveCards.AdaptiveCard adaptiveCard = _util.Build(Faker.Commerce.ProductName());

        string serialized = JsonUtil.Serialize(adaptiveCard, libraryType: JsonLibraryType.Newtonsoft)!;

        var result = JsonUtil.Deserialize<AdaptiveCards.AdaptiveCard>(serialized, JsonLibraryType.Newtonsoft);

        result.Should().NotBeNull();
    }
}
