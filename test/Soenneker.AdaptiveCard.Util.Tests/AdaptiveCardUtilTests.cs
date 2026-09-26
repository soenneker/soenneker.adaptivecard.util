using AwesomeAssertions;
using Soenneker.AdaptiveCard.Util.Abstract;
using Soenneker.Tests.HostedUnit;

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

        string serialized = adaptiveCard.ToJson();

        AdaptiveCards.AdaptiveCard result = AdaptiveCards.AdaptiveCard.FromJson(serialized).Card;

        result.Should().NotBeNull();
    }
}
