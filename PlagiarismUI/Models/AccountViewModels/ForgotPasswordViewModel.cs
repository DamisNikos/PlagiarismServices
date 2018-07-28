using System.ComponentModel.DataAnnotations;

namespace PlagiarismUI.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}