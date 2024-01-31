using System.ComponentModel.DataAnnotations;

namespace Part_2_CreateUser.Models.Authentication.SignUp
{
    public class RegisterUser
    {
        [Required(ErrorMessage = "UserName is required !")]
        public string UserName { get; set; } = null;
        [EmailAddress]
        [Required(ErrorMessage = "Email is required !")]
        public string Email { get; set; } = null;
        [Required(ErrorMessage = "Password is required !")]
        public string Password { get; set; } = null;
    }
}
