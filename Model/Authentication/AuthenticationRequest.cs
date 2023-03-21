using System.ComponentModel.DataAnnotations;

namespace SCGame{

    public class AuthenticationRequest{

        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}