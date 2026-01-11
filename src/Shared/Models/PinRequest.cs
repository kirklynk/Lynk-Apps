using Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Models
{
    public class PinRequest
    {
        [Required]
        public EntityType Entity { get; set; }

        [Required]
        public Guid ReferenceId { get; set; }
    }
}
