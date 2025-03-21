using Soenneker.AdaptiveCard.Util.Abstract;
using Soenneker.Tests.FixturedUnit;
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
}
