using Thryft.Models;

namespace Thryft.Services
{
    public class ProductIconService
    {
        public string GetProductIcon(string category, Colour? color = null)
        {
            var iconClass = category?.ToLower() switch
            {
                "clothing" or "shirt" or "t-shirt" => "fa-tshirt",
                "dress" => "fa-female",
                "pants" or "trousers" => "fa-user",
                "shoes" or "sneakers" => "fa-shoe-prints",
                "socks" => "fa-socks",
                "hat" or "cap" => "fa-hat-cowboy",
                "jewelery" or "earrings" => "fa-gem",
                "bag" or "purse" => "fa-bag-shopping",
                "watch" => "fa-clock",
                "glasses" or "sunglasses" => "fa-glasses",
                "electronics" => "fa-laptop",
                "book" => "fa-book",
                "sports" => "fa-basketball-ball",
                "accessories" => "fa-vest",
                "jacket" => "fa-solid fa-temperature-half", 
                "jumper" => "fa-shirt", 
                _ => "fa-cube"
            };

            var colorClass = GetColorClass(color);
            return $"fas {iconClass} {colorClass}";
        }

        private string GetColorClass(Colour? color)
        {
            return color switch
            {
                Colour.Red => "#f44336",
                Colour.Blue => "#2196f3",
                Colour.Green => "#4caf50",
                Colour.Black => "#000000",
                Colour.White => "#ffffff",
                Colour.Yellow => "#ffeb3b",
                Colour.Pink => "#e91e63",
                Colour.Purple => "#9c27b0",
                Colour.Orange => "#ff9800",
                Colour.Grey => "#9e9e9e",
                Colour.Brown => "#795548",
                Colour.Navy => "#001f3f",
                Colour.Teal => "#39cccc",
                Colour.Maroon => "#85144b",
                Colour.Beige => "#f5f5dc",
                Colour.Gold => "#FFD700",
                Colour.Silver => "#C0C0C0",
                Colour.RoseGold => "#B76E79",
                Colour.Multicolour => "#FF69B4", // Bright/fun color to represent multiple colors
                Colour.Assorted => "#9370DB",    // Medium purple - distinct but neutral
                _ => "#ffffff"
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
                Colour.White => "color: #ffffff;",
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
                Colour.Gold => "color: #FFD700;",
                Colour.Silver => "color: #C0C0C0;",
                Colour.RoseGold => "color: #B76E79;",
                Colour.Multicolour => "color: #FF69B4;", // Bright/fun color to represent multiple colors
                Colour.Assorted => "color: #9370DB;",    // Medium purple - distinct but neutral
                _ => "color: var(--mud-palette-dark);"
            };
        }
    }
}