using System.ComponentModel.DataAnnotations;

namespace DocumentManagementService.ViewModels
{
    public class ContainerViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string? Name { get; set; }
    }
}
