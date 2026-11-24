using MedLink.Application.DTOs.Common.ProviderDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Common
{
    public interface IProviderAvailabilityService
    {
        Task<IReadOnlyList<GetAvailabilityDto>> GetMyAvailabilityAsync(string userId);
        Task SetMyAvailabilityAsync(string userId, EditAvailabilityDto dto);
    }
}