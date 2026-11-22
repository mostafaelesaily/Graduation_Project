using MedLink.Domain.Entities.Common.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Domain.Entities.Medical_History
{
    public class MedicalRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public string Diagnosis { get; set; }
        public string Notes { get; set; }
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PatientId { get; set; } = string.Empty;
        public AppUser Patient { get; set; }
    }
}
