using System.ComponentModel.DataAnnotations;

namespace PlagiarismUI.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}