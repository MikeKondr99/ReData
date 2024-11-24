using FluentAssertions;
using ReData.Core;

namespace ReData.Domain.Tests;

public sealed class StringKeyTests
{
    [Fact]
    public void SameKeyWillOverrideValue()
    {
        Dictionary<StringKey, string> sut = new Dictionary<StringKey, string>();


        sut["A"] = "Upper";
        sut["a"] = "Lower";

        sut["A"].Should().Be("Lower");
    }

    [Fact]
    public void CanGetValueWithdifferentCaseKey()
    {
        Dictionary<StringKey, string> sut = new Dictionary<StringKey, string>();

        sut["Param"] = "45";

        sut["PARAM"].Should().Be("45");
    }
    
}