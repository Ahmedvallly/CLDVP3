using System.ComponentModel.DataAnnotations;

public class Notification
{
    [Key]
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public int Id { get; set; }
    public string NotificationType { get; set; }
}