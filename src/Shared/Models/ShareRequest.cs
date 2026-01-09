using Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Models
{
    public class ShareRequest
    {
        public Guid Id { get; set; }

        [Required]
        public Guid? ReferenceId
        {
            get; set;
        }

        public string? Name { get; set; }
       
        public string? Description { get; set; }
        
        [Required]
        public string? Owner { get; set; }
        public List<string> Recipients { get; set; } = new List<string>();
        public EntityType EntityType { get; set; }

    }
}
