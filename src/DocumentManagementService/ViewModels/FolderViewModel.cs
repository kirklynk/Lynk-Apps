using System.ComponentModel.DataAnnotations;

namespace DocumentManagementService.ViewModels
{
    public class FolderViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string? Name { get; set; }
    }
}
