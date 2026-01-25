using Shared.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace DocumentManagementService.Domain
{
    internal class LynkShare
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ReferenceId { get; set; }
       
        [Required]
        public string? Owner { get; set; }

        public EntityType EntityType { get; set; }
        [Required]
        public Guid UserSubscriptionId { get; set; }

    }
}
