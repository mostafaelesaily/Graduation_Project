using MedLink.Application.DTOs.Common.PaginatedDTOs;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Interfaces.Common
{
    public interface IAdminService
    {
        Task<PaginatedResultDto<GetProviderRequestDto>> GetAllProviderRequest(int pageNum , int pageSize);
        Task AcceptProviderAsync(Guid providerId);
        Task RejectProviderAsync(Guid providerId);

    }
}
