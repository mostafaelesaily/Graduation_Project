using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Domain.Entities.Common.Identity
{
    public class AppUser : IdentityUser
    {
        public bool IsActice { get; set; } = true;
        public ICollection<RefreshTokens> RefreshTokens { get; set; }
        public ICollection<Notifications> notifications { get; set; }
        public ProviderProfile providerProfile { get; set; }
    }
}
