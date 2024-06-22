using Microsoft.AspNetCore.Mvc;
using System.Linq;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;

    public AccountController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Signup()
    {
        return View("~/Views/Authentication/Signup.cshtml");
    }

    [HttpPost]
    public IActionResult Signup(User user)
    {
        if (!ModelState.IsValid)
        {
            // If the model state is not valid, return the view with the current user data to display validation errors
            return View("~/Views/Authentication/Signup.cshtml", user);
        }

        // Check if email already exists
        if (_db.Users.Any(u => u.Email == user.Email))
        {
            ModelState.AddModelError("Email", "Email already exists.");
            return View("~/Views/Authentication/Signup.cshtml", user);
        }

        // Set the default Role
        user.Role = "User";

        _db.Users.Add(user);
        _db.SaveChanges();

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View("~/Views/Authentication/Login.cshtml");
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        User user = _db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

        if (user != null)
        {
            // Store the UserID in the session for later use
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Role", user.Role);
            return RedirectToAction("Index", "Home");
        }

        return View();

    }
    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("UserID");
        HttpContext.Session.Remove("Role");
        return RedirectToAction("Login");
    }
}