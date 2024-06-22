public class OrderStatusViewModel
{
    public string TrackingNumber { get; set; }
    public Order.OrderStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderItem> OrderItems { get; set; }
}