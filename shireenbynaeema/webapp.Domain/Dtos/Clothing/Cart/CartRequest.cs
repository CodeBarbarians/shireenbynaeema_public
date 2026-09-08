namespace Domain
{
    public class CartItem_AddEdit
    {
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class CartItem_UpdateQuantity
    {
        public int Quantity { get; set; }
    }

    public class CartDto
    {
        public Guid Id { get; set; }
        public List<CartItemDto> Items { get; set; } = [];
        public decimal Subtotal { get; set; }
    }

    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductVariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int Stock { get; set; }
    }
}
