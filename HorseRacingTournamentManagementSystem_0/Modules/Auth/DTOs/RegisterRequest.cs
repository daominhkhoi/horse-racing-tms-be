using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Auth.DTOs
{
    public class RegisterRequest
    {

        [Required(ErrorMessage = "Email must not empty!")]
        [RegularExpression(
         @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
          ErrorMessage = "Invalid email format!"
            )]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password must not empty!")]
        [StringLength(19, MinimumLength = 9, ErrorMessage = "Passwords must be longer than 8 characters and shorter than 20 characters!")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "The password confirmation field cannot be left blank!")]
        [Compare("Password", ErrorMessage = "Confirm password doesn't match!!")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
