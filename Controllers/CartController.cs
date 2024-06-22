using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using KhumaloCraftFinal.Models;

namespace KhumaloCraftFinal.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.Find(id);
            var cartJson = HttpContext.Session.GetString("Cart") ?? "[]";
            var cart = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrderItem>>(cartJson);
            var cartItem = cart.FirstOrDefault(i => i.ProductId == id);
            if (cartItem == null)
            {
                cart.Add(new OrderItem
                {
                    ProductId = product.ProductId,
                    Product = product,
                    Quantity = 1,
                    Price = product.ProductPrice
                });
            }
            else
            {
                cartItem.Quantity++;
            }
            cartJson = Newtonsoft.Json.JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", cartJson);
            return RedirectToAction("ProductList", "Products");
        }

        public IActionResult Cart()
        {
            var cartJson = HttpContext.Session.GetString("Cart") ?? "[]";
            var cart = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrderItem>>(cartJson);
            return View(cart);
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cartJson = HttpContext.Session.GetString("Cart") ?? "[]";
            var cart = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrderItem>>(cartJson);
            var cartItem = cart.FirstOrDefault(i => i.ProductId == id);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }
            cartJson = Newtonsoft.Json.JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", cartJson);
            return RedirectToAction("Cart");
        }

        public IActionResult IncreaseQuantity(int id)
        {
            var cartJson = HttpContext.Session.GetString("Cart") ?? "[]";
            var cart = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrderItem>>(cartJson);
            var cartItem = cart.FirstOrDefault(i => i.ProductId == id);
            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            cartJson = Newtonsoft.Json.JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", cartJson);
            return RedirectToAction("Cart");
        }

        public IActionResult DecreaseQuantity(int id)
        {
            var cartJson = HttpContext.Session.GetString("Cart") ?? "[]";
            var cart = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrderItem>>(cartJson);
            var cartItem = cart.FirstOrDefault(i => i.ProductId == id);
            if (cartItem != null && cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
            }
            cartJson = Newtonsoft.Json.JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", cartJson);
            return RedirectToAction("Cart");
        }
    }
}