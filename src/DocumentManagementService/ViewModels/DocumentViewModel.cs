using System.ComponentModel.DataAnnotations;

namespace DocumentManagementService.ViewModels
{
    public class DocumentViewModel
    {
        public Guid? Id { get; internal set; }

        [Required]
        public string Name { get; set; }

        public bool IsFolder { get; set; }

        public DateTime? ModifiedOn { get; set; }
    }
}
