﻿
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserID { get; set; }
    public string Role { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}