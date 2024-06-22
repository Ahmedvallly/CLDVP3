using System.ComponentModel.DataAnnotations;

public class Order
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ShippingAddress { get; set; }
    public decimal TotalPrice { get; set; }
    [Required, StringLength(8)]
    public string TrackingNumber { get; set; }
    public OrderStatus Status { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public enum OrderStatus
    {
        Pending,
        Shipped,
        Delivered
    }
}

