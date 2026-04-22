using AwesomeAssertions;
using Soenneker.AdaptiveCard.Util.Abstract;
using Soenneker.Enums.JsonLibrary;
using Soenneker.Tests.HostedUnit;
using Soenneker.Utils.Json;

namespace Soenneker.AdaptiveCard.Util.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class AdaptiveCardUtilTests : HostedUnitTest
{
    private readonly IAdaptiveCardUtil _util;

    public AdaptiveCardUtilTests(Host host) : base(host)
    {
        _util = Resolve<IAdaptiveCardUtil>(true);
    }

    [Test]
    public void Default()
    {

    }

    [Test]
    public void AdaptiveCard_should_serialize_and_deserialize_with_newtonsoft()
    {
        AdaptiveCards.AdaptiveCard adaptiveCard = _util.Build(Faker.Commerce.ProductName());

        string serialized = JsonUtil.Serialize(adaptiveCard, libraryType: JsonLibraryType.Newtonsoft)!;

        var result = JsonUtil.Deserialize<AdaptiveCards.AdaptiveCard>(serialized, JsonLibraryType.Newtonsoft);

        result.Should().NotBeNull();
    }
}
