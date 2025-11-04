using Thryft.Services;
using Thryft.Models;
using Xunit;

namespace Thryft.Tests.Services
{
    public class ProductIconServiceTests
    {
        private readonly ProductIconService _svc = new ProductIconService();

        [Theory]
        [InlineData("Clothing", Colour.Red, "fa-tshirt")]
        [InlineData("dress", null, "fa-female")]
        [InlineData("shoes", Colour.Blue, "fa-shoe-prints")]
        [InlineData("unknowncategory", null, "fa-cube")]
        public void GetProductIcon_ReturnsExpectedIcon(string category, Colour? color, string expectedIcon)
        {
            var result = _svc.GetProductIcon(category, color);
            Assert.Contains(expectedIcon, result);
            Assert.Contains("fas", result);
        }

        [Fact]
        public void GetColorStyle_ReturnsColorCss_ForKnownColor()
        {
            var css = _svc.GetColorStyle(Colour.Green);
            Assert.Contains("color:", css);
            Assert.Contains("#4caf50", css);
        }

        [Fact]
        public void GetColorStyle_DefaultsToPalette_WhenNull()
        {
            var css = _svc.GetColorStyle(null);
            Assert.Contains("var(--mud-palette-dark)", css);
        }
    }
}