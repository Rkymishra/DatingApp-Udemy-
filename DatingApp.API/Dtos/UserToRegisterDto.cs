using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos {
    public class UserToRegisterDto {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength (15, MinimumLength = 4, ErrorMessage = "4 to 15 characters are required!")]
        public string Password { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string KnownAs { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public UserToRegisterDto () {
            Created = DateTime.Now;
            LastActive = DateTime.Now;
        }
    }
}