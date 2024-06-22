using KhumaloCraftFinal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhumaloCraftFinal.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ProductList()
        {
            var products = _context.Products.Where(p => p.StockCount > 0).ToList();
            return View(products);
        }

        // GET: Products
        public IActionResult Index()
        {
            string userRole = HttpContext.Session.GetString("Role");
            if (userRole != "Admin")
            {
                return Unauthorized();
            }

            var products = _context.Products.ToList();
            return View(products);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            string userRole = HttpContext.Session.GetString("Role");
            if (userRole != "Admin")
            {
                return Unauthorized();
            }

            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("ProductId,ProductName,ProductDescription,ProductPrice,StockCount")] Product product)
        {
            string userRole = HttpContext.Session.GetString("Role");
            if (userRole != "Admin")
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                _context.Add(product);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public IActionResult Edit(int? id)
        {
            string userRole = HttpContext.Session.GetString("Role");
            if (userRole != "Admin")
            {
                return Unauthorized();
            }

            if (id == null)
            {
                return NotFound();
            }

            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("ProductId,ProductName,ProductDescription,ProductPrice,StockCount")] Product product)
        {
            string userRole = HttpContext.Session.GetString("Role");
            if (userRole != "Admin")
            {
                return Unauthorized();
            }

            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = _context.Products.Find(id);
                    if (existingProduct != null)
                    {
                        existingProduct.ProductName = product.ProductName;
                        existingProduct.ProductDescription = product.ProductDescription;
                        existingProduct.ProductPrice = product.ProductPrice;
                        existingProduct.StockCount = product.StockCount;
                    }
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public IActionResult Delete(int? id)
        {
            string userRole = HttpContext.Session.GetString("Role");
            if (userRole != "Admin")
            {
                return Unauthorized();
            }

            if (id == null)
            {
                return NotFound();
            }

            var product = _context.Products
                .FirstOrDefault(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            string userRole = HttpContext.Session.GetString("Role");
            if (userRole != "Admin")
            {
                return Unauthorized();
            }

            var product = _context.Products.Find(id);
            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateStockCount(int id, int stockCount)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                product.StockCount = stockCount;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}