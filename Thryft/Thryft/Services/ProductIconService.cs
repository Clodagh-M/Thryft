using Thryft.Models;

namespace Thryft.Services
{
    public class ProductIconService
    {
        public string GetProductIcon(string category, Colour? color = null)
        {
            var iconClass = category?.ToLower() switch
            {
                "clothing" or "shirt" or "t-shirt" => "fa-shirt",
                "dress" => "fa-vest",
                "pants" or "trousers" => "fa-user",
                "shoes" or "sneakers" => "fa-shoe-prints",
                "socks" => "fa-socks",
                "hat" or "cap" => "fa-hat-cowboy",
                "jewelry" or "earrings" => "fa-gem",
                "bag" or "purse" => "fa-bag-shopping",
                "watch" => "fa-clock",
                "glasses" or "sunglasses" => "fa-glasses",
                "electronics" => "fa-laptop",
                "book" => "fa-book",
                "sports" => "fa-basketball-ball",
                "accessories" => "fa-brands fa-black-tie",
                _ => "fa-cube"
            };

            var colorClass = GetColorClass(color);
            return $"fas {iconClass} {colorClass}";
        }

        private string GetColorClass(Colour? color)
        {
            return color switch
            {
                Colour.Red => "fa-red",
                Colour.Blue => "fa-blue",
                Colour.Green => "fa-green",
                Colour.Black => "fa-black",
                Colour.White => "fa-white",
                Colour.Yellow => "fa-yellow",
                Colour.Pink => "fa-pink",
                Colour.Purple => "fa-purple",
                Colour.Orange => "fa-orange",
                Colour.Grey => "fa-gray",
                Colour.Brown => "fa-brown",
                Colour.Navy => "fa-navy",
                Colour.Teal => "fa-teal",
                Colour.Maroon => "fa-maroon",
                Colour.Beige => "fa-beige",
                _ => "fa-primary"
            };
        }

        public string GetColorStyle(Colour? color)
        {
            return color switch
            {
                Colour.Red => "color: #f44336;",
                Colour.Blue => "color: #2196f3;",
                Colour.Green => "color: #4caf50;",
                Colour.Black => "color: #000000;",
                Colour.White => "color: #ffffff; border: 1px solid #ccc;",
                Colour.Yellow => "color: #ffeb3b;",
                Colour.Pink => "color: #e91e63;",
                Colour.Purple => "color: #9c27b0;",
                Colour.Orange => "color: #ff9800;",
                Colour.Grey => "color: #9e9e9e;",
                Colour.Brown => "color: #795548;",
                Colour.Navy => "color: #001f3f;",
                Colour.Teal => "color: #39cccc;",
                Colour.Maroon => "color: #85144b;",
                Colour.Beige => "color: #f5f5dc;",
                _ => "color: var(--mud-palette-primary);"
            };
        }
    }
}