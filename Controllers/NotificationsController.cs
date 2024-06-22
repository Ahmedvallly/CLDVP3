using Microsoft.AspNetCore.Mvc;

public class NotificationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public NotificationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        int userId = HttpContext.Session.GetInt32("UserID") ?? 0;
        var notifications = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.NotificationId)
            .ToList();

        return View(notifications);
    }

    [HttpPost]
    public IActionResult ClearNotifications()
    {
        int userId = HttpContext.Session.GetInt32("UserID") ?? 0;
        var notifications = _context.Notifications.Where(n => n.UserId == userId);
        _context.Notifications.RemoveRange(notifications);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult ClearNotification(int id)
    {
        int userId = HttpContext.Session.GetInt32("UserID") ?? 0;
        var notification = _context.Notifications.FirstOrDefault(n => n.NotificationId == id && n.UserId == userId);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }
}