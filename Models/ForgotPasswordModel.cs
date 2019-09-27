using System.ComponentModel.DataAnnotations;

namespace idsrv4testaspid.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}