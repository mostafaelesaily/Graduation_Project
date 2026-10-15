using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Domain.Entities.Common.File
{
    public class StoredFile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
