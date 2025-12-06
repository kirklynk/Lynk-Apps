using System.ComponentModel.DataAnnotations;

namespace DocumentManagementService.Domain
{
    internal record Tag(Guid Id, [Required] string Name);
}
