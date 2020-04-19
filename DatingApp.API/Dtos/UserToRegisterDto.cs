using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserToRegisterDto
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        [StringLength(15,MinimumLength=4, ErrorMessage="4 to 15 characters are required!")]
        public string Password { get; set; }
    }
}