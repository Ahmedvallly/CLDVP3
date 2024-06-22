using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace KhumaloCraftFinal.Models
{
    public class PaymentViewModel
    {
        [Required]
        [Display(Name = "Credit Card Number")]
        [RegularExpression(@"^(\d{12,19})$", ErrorMessage = "Invalid credit card number")]
        public string CardNumber { get; set; }

        [Required]
        [Display(Name = "Expiry Date (MM/YY)")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Invalid expiry date format")]
        public string ExpiryDate { get; set; }
    }
}