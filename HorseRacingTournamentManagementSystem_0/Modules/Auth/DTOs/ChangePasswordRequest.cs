using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Auth.DTOs
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Old password must not be empty!")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password must not be empty!")]
        [StringLength(19, MinimumLength = 9, 
            ErrorMessage = "Passwords must be between 9 and 19 characters!")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).*$", 
            ErrorMessage = "Password must contain at least one uppercase letter and at least one number!")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "The password confirmation field cannot be left blank!")]
        [Compare("NewPassword", ErrorMessage = "Confirm password doesn't match!!")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
