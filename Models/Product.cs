// Product.cs
using System.ComponentModel.DataAnnotations;


    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        public string ProductDescription { get; set; }
     public int StockCount { get; set; }

    [Required]
  
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    [DataType(DataType.Currency)]
    public decimal ProductPrice { get; set; }
    }
