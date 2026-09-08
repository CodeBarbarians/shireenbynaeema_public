namespace Domain
{
    using SharedServices;

    public class Product_AddEdit
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public Guid CategoryId { get; set; }
    public string Brand { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public string SizeChartJson { get; set; } = "[]";
    public List<VariantDto>? Variants { get; set; }
}

public class VariantDto
{
    public Guid? Id { get; set; }
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int BookedStock { get; set; }
    public decimal Price { get; set; }
    public string SKU { get; set; } = string.Empty;
}

public class ProductListRequest : ListRequest
{
    public string? Search { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsFeatured { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}

public class Product_Listing
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public List<VariantDto> Variants { get; set; } = [];
    public List<ProductImageDto> Images { get; set; } = [];
    public string SizeChartJson { get; set; } = "[]";
    public long? CreatedOn { get; set; }
}

public class ProductImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    }
}
