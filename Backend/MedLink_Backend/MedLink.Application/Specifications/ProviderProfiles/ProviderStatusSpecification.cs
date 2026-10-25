using MedLink.Domain.Entities.Common;
using MedLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Specifications.ProviderProfiles
{
    public class ProviderStatusSpecification : BaseSpecification<ProviderProfile> 
    {
        public ProviderStatusSpecification
            (
             int pageNum , 
             int pageSize
            
            ) : base(provider =>
             provider.verificationStatus == VerificationStatus.Pending ||
             provider.verificationStatus == VerificationStatus.Rejected
            )
        {
            AddInclude(provider => provider.user);

            ApplyPaging(
            (pageNum - 1) * pageSize,
            pageSize);

        }
    }
}
