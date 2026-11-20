using AutoMapper;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Services.Common;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MedLink.Test.Services
{
    public class ProviderManagementServiceTest
    {
        private readonly Mock<IUnitOfWork> unitOfWorkMock = new();
        private readonly Mock<IMapper> mapperMock = new();
        private readonly Mock<UserManager<AppUser>> userManagerMock;
        private readonly Mock<IFileService> fileServiceMock = new();
        private readonly Mock<ILogger<ProviderManagementService>> loggerMock = new();

        private readonly Mock<IGenaricRepo<ProviderProfile>> providerProfileRepoMock = new();
        private readonly Mock<IGenaricRepo<MedicalRecord>> medicalRecordRepoMock = new();
        private readonly Mock<IGenaricRepo<Review>> reviewRepoMock = new();
        private readonly Mock<IGenaricRepo<ProviderService>> providerServiceRepoMock = new();
        private readonly Mock<IGenaricRepo<StoredFile>> storedFileRepoMock = new();
        private readonly Mock<IGenaricRepo<ProviderLocations>> locationRepoMock = new();
        private readonly Mock<IGenaricRepo<ProviderAvailability>> availabilityRepoMock = new();

        private readonly ProviderManagementService sut;

        public ProviderManagementServiceTest()
        {
            var storeMock = new Mock<IUserStore<AppUser>>();
            userManagerMock = new Mock<UserManager<AppUser>>(
                storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            unitOfWorkMock.Setup(u => u.Repository<ProviderProfile>()).Returns(providerProfileRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Repository<MedicalRecord>()).Returns(medicalRecordRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Repository<Review>()).Returns(reviewRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Repository<ProviderService>()).Returns(providerServiceRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Repository<StoredFile>()).Returns(storedFileRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Repository<ProviderLocations>()).Returns(locationRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Repository<ProviderAvailability>()).Returns(availabilityRepoMock.Object);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            availabilityRepoMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderAvailability>>()))
                .ReturnsAsync(new List<ProviderAvailability>());
            unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => new AppUser { Id = id });
            userManagerMock.Setup(x => x.IsInRoleAsync(It.IsAny<AppUser>(), "User"))
                .ReturnsAsync(true);
            userManagerMock.Setup(x => x.IsInRoleAsync(It.IsAny<AppUser>(), "Provider"))
                .ReturnsAsync(true);

            mapperMock.Setup(m => m.Map<ReviewDto>(It.IsAny<Review>()))
                .Returns((Review r) => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    PatientId = r.PatientId,
                    ProviderProfileId = r.ProviderProfileId
                });

            mapperMock.Setup(m => m.Map<Review>(It.IsAny<CreateReviewDto>()))
                .Returns((CreateReviewDto dto) => new Review
                {
                    Rating = dto.Rating,
                    Comment = dto.Comment
                });

            mapperMock.Setup(m => m.Map<ProviderAvailability>(It.IsAny<ProviderAvailabilityDto>()))
                .Returns((ProviderAvailabilityDto dto) => new ProviderAvailability
                {
                    DayOfWeek = dto.DayOfWeek,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime
                });

            mapperMock.Setup(m => m.Map<MedicalRecordDto>(It.IsAny<MedicalRecord>()))
                .Returns((MedicalRecord r) => new MedicalRecordDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    PatientId = r.PatientId,
                    ProviderProfileId = r.ProviderProfileId
                });

            mapperMock.Setup(m => m.Map<MedicalRecord>(It.IsAny<CreateMedicalRecordDto>()))
                .Returns((CreateMedicalRecordDto dto) => new MedicalRecord
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    Diagnosis = dto.Diagnosis,
                    Notes = dto.Notes,
                    RecordDate = dto.RecordDate
                });

            mapperMock.Setup(m => m.Map<IReadOnlyList<MedicalRecordDto>>(It.IsAny<IEnumerable<MedicalRecord>>()))
                .Returns((IEnumerable<MedicalRecord> records) => records.Select(r => new MedicalRecordDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    PatientId = r.PatientId,
                    ProviderProfileId = r.ProviderProfileId
                }).ToList());

            sut = new ProviderManagementService(
                unitOfWorkMock.Object,
                mapperMock.Object,
                userManagerMock.Object,
                fileServiceMock.Object,
                loggerMock.Object);
        }

        [Fact]
        public async Task CreateReviewAsync_ValidRequest_ReturnsReview()
        {
            var patientId = "patient-1";
            var providerId = Guid.NewGuid();
            var dto = new CreateReviewDto
            {
                ProviderProfileId = providerId,
                Rating = 5,
                Comment = "Excellent"
            };

            providerProfileRepoMock.Setup(r => r.GetByIdAsync(providerId))
                .ReturnsAsync(new ProviderProfile { Id = providerId, verificationStatus = VerificationStatus.Approved });

            var result = await sut.CreateReviewAsync(patientId, dto);

            Assert.Equal(5, result.Rating);
            Assert.Equal(patientId, result.PatientId);
            Assert.Equal(providerId, result.ProviderProfileId);
            reviewRepoMock.Verify(r => r.AddAsync(It.IsAny<Review>()), Times.Once);
        }

        [Fact]
        public async Task GetMyMedicalRecordsAsync_ReturnsOnlyOwnRecords()
        {
            var patientId = "patient-1";
            var records = new List<MedicalRecord>
            {
                new MedicalRecord { Id = Guid.NewGuid(), PatientId = patientId, Title = "Mine" },
                new MedicalRecord { Id = Guid.NewGuid(), PatientId = "other", Title = "Other" }
            };
            medicalRecordRepoMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<MedicalRecord>>()))
                .ReturnsAsync((ISpecification<MedicalRecord> spec) => records.Where(r => spec.Criteria == null || spec.Criteria.Compile()(r)).ToList());

            var result = await sut.GetMyMedicalRecordsAsync(patientId);

            Assert.Single(result);
            Assert.Equal("Mine", result[0].Title);
        }

        [Fact]
        public async Task CreateReviewAsync_InvalidRating_ThrowsBadRequest()
        {
            var dto = new CreateReviewDto
            {
                ProviderProfileId = Guid.NewGuid(),
                Rating = 0,
                Comment = "Bad"
            };

            await Assert.ThrowsAsync<MedLink.Application.Exceptions.BadRequestException>(
                () => sut.CreateReviewAsync("patient-1", dto));
        }

        [Fact]
        public async Task CreateReviewAsync_MissingProvider_ThrowsNotFound()
        {
            var providerId = Guid.NewGuid();
            var dto = new CreateReviewDto
            {
                ProviderProfileId = providerId,
                Rating = 5,
                Comment = "Good"
            };

            providerProfileRepoMock.Setup(r => r.GetByIdAsync(providerId))
                .ReturnsAsync((ProviderProfile?)null);

            await Assert.ThrowsAsync<MedLink.Application.Exceptions.NotFoundException>(
                () => sut.CreateReviewAsync("patient-1", dto));
        }

        [Fact]
        public async Task CreateMedicalRecordAsync_UnapprovedProvider_ThrowsForbidden()
        {
            var providerId = Guid.NewGuid();
            var profile = new ProviderProfile
            {
                Id = providerId,
                userId = "provider-1",
                verificationStatus = VerificationStatus.Pending
            };
            providerProfileRepoMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(profile);

            var dto = new CreateMedicalRecordDto
            {
                PatientId = "patient-1",
                Title = "Record",
                RecordDate = DateTime.UtcNow,
                Description = "Description",
                Diagnosis = "Diagnosis"
            };

            await Assert.ThrowsAsync<MedLink.Application.Exceptions.ForbiddenException>(
                () => sut.CreateMedicalRecordAsync("provider-1", dto));
        }

        [Fact]
        public async Task CreateMedicalRecordAsync_MissingPatient_ThrowsNotFound()
        {
            var providerId = Guid.NewGuid();
            var profile = new ProviderProfile
            {
                Id = providerId,
                userId = "provider-1",
                verificationStatus = VerificationStatus.Approved
            };
            providerProfileRepoMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(profile);
            userManagerMock.Setup(x => x.FindByIdAsync("missing-patient"))
                .ReturnsAsync((AppUser?)null);

            var dto = new CreateMedicalRecordDto
            {
                PatientId = "missing-patient",
                Title = "Record",
                RecordDate = DateTime.UtcNow,
                Description = "Description",
                Diagnosis = "Diagnosis"
            };

            await Assert.ThrowsAsync<MedLink.Application.Exceptions.NotFoundException>(
                () => sut.CreateMedicalRecordAsync("provider-1", dto));
        }

        [Fact]
        public async Task GetMedicalRecordAsync_UnauthorizedUser_ThrowsForbidden()
        {
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord
            {
                Id = recordId,
                PatientId = "patient-1",
                ProviderProfileId = Guid.NewGuid(),
                Title = "Record"
            };
            medicalRecordRepoMock.Setup(r => r.GetByIdAsync(recordId))
                .ReturnsAsync(record);
            providerProfileRepoMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(new List<ProviderProfile>());

            await Assert.ThrowsAsync<MedLink.Application.Exceptions.ForbiddenException>(
                () => sut.GetMedicalRecordAsync(recordId, "other-user"));
        }

        [Fact]
        public async Task UpdateServiceAsync_NonOwner_ThrowsNotFound()
        {
            var providerId = Guid.NewGuid();
            var profile = new ProviderProfile { Id = providerId, userId = "provider-1" };
            providerProfileRepoMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(new List<ProviderProfile> { profile });

            var service = new ProviderService
            {
                Id = Guid.NewGuid(),
                ProviderProfileId = Guid.NewGuid(),
                ServiceName = "Service"
            };
            providerServiceRepoMock.Setup(r => r.GetByIdAsync(service.Id))
                .ReturnsAsync(service);

            var dto = new UpdateProviderServiceDto
            {
                ServiceName = "Updated",
                Price = 10,
                IsActive = true
            };

            await Assert.ThrowsAsync<MedLink.Application.Exceptions.NotFoundException>(
                () => sut.UpdateServiceAsync("provider-1", service.Id, dto));
        }

        [Fact]
        public async Task DeleteLocationAsync_NonOwner_ThrowsNotFound()
        {
            var providerId = Guid.NewGuid();
            var profile = new ProviderProfile { Id = providerId, userId = "provider-1" };
            providerProfileRepoMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(new List<ProviderProfile> { profile });

            var location = new ProviderLocations
            {
                Id = Guid.NewGuid(),
                ProviderProfileId = Guid.NewGuid(),
                Address = "Address"
            };
            locationRepoMock.Setup(r => r.GetByIdAsync(location.Id))
                .ReturnsAsync(location);

            await Assert.ThrowsAsync<MedLink.Application.Exceptions.NotFoundException>(
                () => sut.DeleteLocationAsync("provider-1", location.Id));
        }

        [Fact]
        public async Task UploadDocumentAsync_NullFile_ThrowsBadRequest()
        {
            await Assert.ThrowsAsync<MedLink.Application.Exceptions.BadRequestException>(
                () => sut.UploadDocumentAsync("provider-1", null!));
        }

        [Fact]
        public async Task CreateMedicalRecordAsync_NonPatientTarget_ThrowsForbidden()
        {
            var providerId = Guid.NewGuid();
            var profile = new ProviderProfile
            {
                Id = providerId,
                userId = "provider-1",
                verificationStatus = VerificationStatus.Approved
            };
            providerProfileRepoMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(profile);
            userManagerMock.Setup(x => x.FindByIdAsync("non-patient"))
                .ReturnsAsync(new AppUser { Id = "non-patient" });
            userManagerMock.Setup(x => x.IsInRoleAsync(It.Is<AppUser>(u => u.Id == "non-patient"), "User"))
                .ReturnsAsync(false);

            var dto = new CreateMedicalRecordDto
            {
                PatientId = "non-patient",
                Title = "Record",
                RecordDate = DateTime.UtcNow,
                Description = "Description",
                Diagnosis = "Diagnosis"
            };

            await Assert.ThrowsAsync<MedLink.Application.Exceptions.ForbiddenException>(
                () => sut.CreateMedicalRecordAsync("provider-1", dto));
        }

        [Fact]
        public async Task GetMedicalRecordAsync_NonPatientNonProviderRole_ThrowsForbidden()
        {
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord
            {
                Id = recordId,
                PatientId = "patient-1",
                ProviderProfileId = Guid.NewGuid(),
                Title = "Record"
            };
            medicalRecordRepoMock.Setup(r => r.GetByIdAsync(recordId))
                .ReturnsAsync(record);
            providerProfileRepoMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync((ProviderProfile?)null);
            userManagerMock.Setup(x => x.FindByIdAsync("other-user"))
                .ReturnsAsync(new AppUser { Id = "other-user" });
            userManagerMock.Setup(x => x.IsInRoleAsync(It.Is<AppUser>(u => u.Id == "other-user"), It.IsAny<string>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<MedLink.Application.Exceptions.ForbiddenException>(
                () => sut.GetMedicalRecordAsync(recordId, "other-user"));
        }

        [Fact]
        public async Task GetMyMedicalRecordsAsync_NonPatient_ThrowsForbidden()
        {
            userManagerMock.Setup(x => x.FindByIdAsync("non-patient"))
                .ReturnsAsync(new AppUser { Id = "non-patient" });
            userManagerMock.Setup(x => x.IsInRoleAsync(It.Is<AppUser>(u => u.Id == "non-patient"), "User"))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<MedLink.Application.Exceptions.ForbiddenException>(
                () => sut.GetMyMedicalRecordsAsync("non-patient"));
        }

        [Fact]
        public async Task UploadDocumentAsync_EmptyFileName_ThrowsBadRequest()
        {
            var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("");
            fileMock.Setup(f => f.Length).Returns(1);
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");

            await Assert.ThrowsAsync<MedLink.Application.Exceptions.BadRequestException>(
                () => sut.UploadDocumentAsync("provider-1", fileMock.Object));
        }

        [Fact]
        public async Task SetMyAvailabilityAsync_PersistenceFailure_RollsBackTransaction()
        {
            var profile = new ProviderProfile { Id = Guid.NewGuid(), userId = "provider-1", verificationStatus = VerificationStatus.Approved };
            providerProfileRepoMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(profile);
            availabilityRepoMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderAvailability>>()))
                .ReturnsAsync(new List<ProviderAvailability>
                {
                    new ProviderAvailability { Id = Guid.NewGuid(), ProviderProfileId = profile.Id, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10) }
                });
            unitOfWorkMock.Setup(u => u.CommitTransactionAsync())
                .ThrowsAsync(new Exception("database failure"));

            var dto = new SetProviderAvailabilityDto
            {
                Intervals = new List<ProviderAvailabilityDto>
                {
                    new ProviderAvailabilityDto { DayOfWeek = DayOfWeek.Tuesday, StartTime = TimeSpan.FromHours(10), EndTime = TimeSpan.FromHours(11) }
                }
            };

            await Assert.ThrowsAsync<Exception>(() => sut.SetMyAvailabilityAsync("provider-1", dto));
            unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }

        [Theory]
        [InlineData(null, "Description", "Diagnosis")]
        [InlineData("Title", null, "Diagnosis")]
        [InlineData("Title", "Description", null)]
        public async Task CreateMedicalRecordAsync_MissingRequiredField_ThrowsBadRequest(string? title, string? description, string? diagnosis)
        {
            var dto = new CreateMedicalRecordDto
            {
                PatientId = "patient-1",
                Title = title!,
                Description = description!,
                Diagnosis = diagnosis!,
                RecordDate = DateTime.UtcNow
            };

            await Assert.ThrowsAsync<MedLink.Application.Exceptions.BadRequestException>(
                () => sut.CreateMedicalRecordAsync("provider-1", dto));
        }

        [Fact]
        public async Task CreateMedicalRecordAsync_DescriptionTooLong_ThrowsBadRequest()
        {
            var dto = new CreateMedicalRecordDto
            {
                PatientId = "patient-1",
                Title = "Title",
                Description = new string('a', 501),
                Diagnosis = "Diagnosis",
                RecordDate = DateTime.UtcNow
            };

            await Assert.ThrowsAsync<MedLink.Application.Exceptions.BadRequestException>(
                () => sut.CreateMedicalRecordAsync("provider-1", dto));
        }

        [Fact]
        public async Task CreateMedicalRecordAsync_OptionalNotesAllowed_ReturnsRecord()
        {
            var providerId = Guid.NewGuid();
            var profile = new ProviderProfile
            {
                Id = providerId,
                userId = "provider-1",
                verificationStatus = VerificationStatus.Approved
            };
            providerProfileRepoMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(profile);
            userManagerMock.Setup(x => x.FindByIdAsync("patient-1"))
                .ReturnsAsync(new AppUser { Id = "patient-1" });
            userManagerMock.Setup(x => x.IsInRoleAsync(It.IsAny<AppUser>(), "User"))
                .ReturnsAsync(true);

            var dto = new CreateMedicalRecordDto
            {
                PatientId = "patient-1",
                Title = "Title",
                Description = "Description",
                Diagnosis = "Diagnosis",
                Notes = null,
                RecordDate = DateTime.UtcNow
            };

            var result = await sut.CreateMedicalRecordAsync("provider-1", dto);

            Assert.Equal("Title", result.Title);
        }
    }
}