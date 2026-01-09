using System.ComponentModel.DataAnnotations;

namespace DocumentManagementService.Domain
{
    internal record LynkTag(Guid Id, [Required] string Name);
}
