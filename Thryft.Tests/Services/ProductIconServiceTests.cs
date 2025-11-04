using Thryft.Services;
using Thryft.Models;
using Xunit;

namespace Thryft.Tests.Services;

public class ProductIconServiceTests
{
    private readonly ProductIconService _service = new();

    [Theory]
    [InlineData("Clothing", Colour.Blue, "fa-tshirt")]
    [InlineData("dress", null, "fa-female")]
    [InlineData("SHOES", Colour.Red, "fa-shoe-prints")]
    [InlineData("unknown-category", null, "fa-cube")]
    public void GetProductIcon_ReturnsExpectedIcon(string category, Colour? color, string expectedIconPart)
    {
        // Act
        var result = _service.GetProductIcon(category, color);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(expectedIconPart, result);
        Assert.StartsWith("fas ", result);
    }

    [Theory]
    [InlineData(Colour.Blue, "color: #2196f3;")]
    [InlineData(Colour.Red, "color: #f44336;")]
    [InlineData(Colour.White, "color: #ffffff;")]
    [InlineData(null, "color: var(--mud-palette-dark);")]
    public void GetColorStyle_ReturnsExpectedCss(Colour? color, string expected)
    {
        var result = _service.GetColorStyle(color);
        Assert.Equal(expected, result);
    }
}