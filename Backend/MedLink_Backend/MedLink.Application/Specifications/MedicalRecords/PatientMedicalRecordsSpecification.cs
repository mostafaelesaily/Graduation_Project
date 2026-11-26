using MedLink.Domain.Entities.Medical_History;

namespace MedLink.Application.Specifications.MedicalRecords
{
    public class PatientMedicalRecordsSpecification : BaseSpecification<MedicalRecord>
    {
        // All medical records belonging to a patient, newest first.
        // Used to build the text context (Diagnosis + Description) for recommendations.
        public PatientMedicalRecordsSpecification(string patientId)
            : base(record => record.PatientId == patientId)
        {
            AddOrderByDescending(record => record.RecordDate);
        }
    }
}
