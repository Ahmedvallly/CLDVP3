using KhumaloCraftFinal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;


namespace KhumaloCraftFinal.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Checkout()
        {
            var cartJson = HttpContext.Session.GetString("Cart") ?? "[]";
            var cart = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrderItem>>(cartJson);
            return View(cart);
        }

        [HttpPost]
        public IActionResult Checkout(string street, string city, string state, string zipCode, string country)
        {
            HttpContext.Session.SetString("Street", street);
            HttpContext.Session.SetString("City", city);
            HttpContext.Session.SetString("State", state);
            HttpContext.Session.SetString("ZipCode", zipCode);
            HttpContext.Session.SetString("Country", country);

            return RedirectToAction("Payment");
        }
        public static string GetStatusText(Order.OrderStatus status)
        {
            switch (status)
            {
                case Order.OrderStatus.Pending:
                    return "Pending";
                case Order.OrderStatus.Shipped:
                    return "Shipped";
                case Order.OrderStatus.Delivered:
                    return "Delivered";
                default:
                    return "Unknown";
            }
        }

        public IActionResult Payment()
        {
            return View(new PaymentViewModel());
        }

        [HttpPost]
        public IActionResult ProcessPayment(PaymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate credit card expiry date
                var expiryParts = model.ExpiryDate.Split('/');
                var expiryMonth = int.Parse(expiryParts[0]);
                var expiryYear = int.Parse(expiryParts[1]) + 2000; // Assuming 2-digit year format
                var expiryDate = new DateTime(expiryYear, expiryMonth, 1);
                if (expiryDate < DateTime.Now)
                {
                    ModelState.AddModelError("ExpiryDate", "Credit card has expired");
                    return View("Payment", model);
                }
                // Process payment logic here
                bool paymentSuccessful = ProcessPayment(model.CardNumber, model.ExpiryDate);
                if (paymentSuccessful)
                {
                    // Retrieve address and cart information from session
                    var street = HttpContext.Session.GetString("Street");
                    var city = HttpContext.Session.GetString("City");
                    var state = HttpContext.Session.GetString("State");
                    var zipCode = HttpContext.Session.GetString("ZipCode");
                    var country = HttpContext.Session.GetString("Country");
                    var cartJson = HttpContext.Session.GetString("Cart") ?? "[]";
                    var cart = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrderItem>>(cartJson);
                    if (cart != null && cart.Count > 0)
                    {
                        int userId = HttpContext.Session.GetInt32("UserID").Value;
                        string shippingAddress = $"{street}, {city}, {state}, {zipCode}, {country}";
                        // Create a new Order
                        var order = new Order
                        {
                            UserId = userId,
                            ShippingAddress = shippingAddress,
                            TrackingNumber = GenerateTrackingNumber(),
                            Status = Order.OrderStatus.Pending,
                            OrderItems = new List<OrderItem>()
                        };
                        // Add OrderItems to the Order
                        foreach (var item in cart)
                        {
                            var product = _context.Products.Find(item.ProductId);
                            if (product != null && product.StockCount >= item.Quantity)
                            {
                                product.StockCount -= item.Quantity;
                                order.OrderItems.Add(new OrderItem
                                {
                                    ProductId = item.ProductId,
                                    Quantity = item.Quantity,
                                    Price = item.Price
                                });
                            }
                            else
                            {
                                ModelState.AddModelError("", $"Product {product.ProductName} is out of stock or insufficient quantity.");
                                return View("Payment", model);
                            }
                        }
                        // Set the TotalPrice
                        order.TotalPrice = order.OrderItems.Sum(oi => oi.Quantity * oi.Price);
                        _context.Orders.Add(order);
                        _context.SaveChanges();

                        // Create a notification for the new order
                        var notification = new Notification
                        {
                            UserId = userId,
                            Id = order.Id,
                            NotificationType = "OrderPlaced"
                        };
                        _context.Notifications.Add(notification);
                        _context.SaveChanges();

                        // Clear the cart
                        HttpContext.Session.SetString("Cart", "[]");
                        return RedirectToAction("Success", "Orders");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Payment failed");
                    return View("Payment", model);
                }
            }
            return View("Payment", model);
        }
        private string GenerateTrackingNumber()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpPost]
        public IActionResult PlaceOrder(string street, string city, string state, string zipCode, string country)
        {
            var cartJson = HttpContext.Session.GetString("Cart") ?? "[]";
            var cart = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrderItem>>(cartJson);
            if (cart != null && cart.Count > 0)
            {
                int userId = HttpContext.Session.GetInt32("UserID").Value;
                string shippingAddress = $"{street}, {city}, {state}, {zipCode}, {country}";
                // Create a new Order
                var order = new Order

                {
                    UserId = userId,
                    TrackingNumber = GenerateTrackingNumber(),
                    ShippingAddress = shippingAddress,
                    Status = Order.OrderStatus.Pending,
                    OrderItems = new List<OrderItem>()
                };
                // Add OrderItems to the Order
                foreach (var item in cart)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });
                }
                // Set the TotalPrice
                order.TotalPrice = order.OrderItems.Sum(oi => oi.Quantity * oi.Price);
                _context.Orders.Add(order);
                _context.SaveChanges();
                // Clear the cart
                HttpContext.Session.SetString("Cart", "[]");
                return RedirectToAction("Success", "Orders");
            }
            return RedirectToAction("Error", "Orders");
        }
        public IActionResult AdminOrderStatus()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return Unauthorized();
            }

            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToList();

            return View("OrderStatusPage", orders);
        }
        [HttpPost]
        public IActionResult UpdateOrderStatus(int id, Order.OrderStatus newStatus)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return Unauthorized();
            }

            var order = _context.Orders.Find(id);
            if (order != null)
            {
                order.Status = newStatus;

                // Create a new notification
                var notification = new Notification
                {
                    UserId = order.UserId,
                    Id = order.Id,
                    NotificationType = GetNotificationTypeForStatus(newStatus)
                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();
            }

            return RedirectToAction("AdminOrderStatus");
        }

        private string GetNotificationTypeForStatus(Order.OrderStatus status)
        {
            switch (status)
            {
                case Order.OrderStatus.Pending:
                    return "OrderPending";
                case Order.OrderStatus.Shipped:
                    return "OrderShipped";
                case Order.OrderStatus.Delivered:
                    return "OrderDelivered";
                default:
                    return "OrderStatusChanged";
            }
        }
        public IActionResult Success()
        {
            return View();
        }
        public IActionResult OrderStatus()
        {
            return View();
        }
        [HttpPost]
        public IActionResult OrderStatus(string trackingNumber)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.TrackingNumber == trackingNumber);

            if (order == null)
            {
                ModelState.AddModelError("", "Invalid tracking number.");
                return View();
            }

            var viewModel = new OrderStatusViewModel
            {
                TrackingNumber = order.TrackingNumber,
                Status = order.Status, // No need for OrderStatus prefix
                TotalPrice = order.TotalPrice,
                OrderItems = order.OrderItems.ToList()
            };

            return View(viewModel);
        }


        public IActionResult Error()
        {
            return View();
        }

        // ... (existing code)

        private bool ProcessPayment(string cardNumber, string expiryDate)
        {
            // Here, you would implement your payment processing logic
            // using a third-party payment gateway or service.
            // For simplicity, we'll assume the payment is always successful.
            return true;
        }
        public IActionResult PastOrders()
        {
            int userId = HttpContext.Session.GetInt32("UserID") ?? 0;

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToList();

            return View(orders);
        }

        public IActionResult AdminOrders()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return Unauthorized();
            }

            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToList();

            return View(orders);
        }
    }
}