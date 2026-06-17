using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Auth.DTOs
{
    public class RegisterRequest
    {
        //FullName
        [StringLength(19, MinimumLength = 0,
            ErrorMessage = "User name must be shorter than 20 characters!")]
        public string FullName { get; set; } = string.Empty;

        //email
        [Required(ErrorMessage = "Email must not empty!")]
        [RegularExpression(
         @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
          ErrorMessage = "Invalid email format!"
            )]
        public string Email { get; set; } = string.Empty;

        //password
        [Required(ErrorMessage = "Password must not be empty!")]
        [StringLength(19, MinimumLength = 9,
            ErrorMessage = "Passwords must be between 9 and 19 characters!")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).*$",
            ErrorMessage = "Password must contain at least one uppercase letter and at least one number!")]
        public string Password { get; set; } = string.Empty;

        //confirm password
        [Required(ErrorMessage = "The password confirmation field cannot be left blank!")]
        [Compare("Password", ErrorMessage = "Confirm password doesn't match!!")]
        public string ConfirmPassword { get; set; } = string.Empty;

        //public string Role { get; set; } = string.Empty;
    }
}
