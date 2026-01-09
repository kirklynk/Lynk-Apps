using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Models
{
    public class LoginModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        [PasswordPropertyText]
        public string Password { get; set; } = string.Empty;

    }
}
