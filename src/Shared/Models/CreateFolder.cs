using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Models
{
    public class CreateContainer
    {
        [Required(ErrorMessage ="Name is required.")]
        public string Name { get; set; }

        public Guid? ParentId { get; set; }
    }
}