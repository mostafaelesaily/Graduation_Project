using MedLink.Application.DTOs.Common.PaginatedDTOs;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Common
{
    public interface IAdminService
    {
        Task<PaginatedResultDto<GetProviderRequestDto>> GetAllProviderRequest(
            int pageNum,
            int pageSize,
            string? search = null,
            VerificationStatus? status = null);
        Task<IReadOnlyList<GetProviderDocumentDto>> GetProviderDocumentsAsync(Guid providerId);
        Task AcceptProviderAsync(Guid providerId);
        Task RejectProviderAsync(Guid providerId, string? rejectionReason = null);
    }
}
