using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Domain.Entities.Common
{
    public class ProviderLocations
    {
      public Guid Id { get; set; } = Guid.NewGuid();
      public Guid ProviderProfileId { get; set; }
      public string? BranchName { get; set; }
      public string Address { get; set; }
      public double? Latitude { get; set; }
      public double? Longitude { get; set; }
      public string PostalCode { get; set; }
      public string ServicePhoneNumber { get; set; }
      public bool IsActive { get; set; } 
      public ProviderProfile ProviderProfile { get; set; }
    }
}

