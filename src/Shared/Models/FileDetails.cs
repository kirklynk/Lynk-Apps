using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public class FileDetails
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid Id { get; set; }
        public FileDetails? Parent { get; set; }
    }
}
