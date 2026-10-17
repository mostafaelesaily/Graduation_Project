using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Interfaces.Common
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string Key);
        Task setAsync<T>
            (
            string key , 
            T value,
            TimeSpan? slidingExpiration = null ,
            TimeSpan? absoluteExpiration = null
            );
       Task RemoveAsync(string Pattern);
       Task<bool> ExistsAsync(string key);

        Task<T?> GetOrSetAsync<T>
            (string key, Func<Task<T?>> factory,
            TimeSpan? slidingExpiration = null,
            TimeSpan? absoluteExpiration = null);
    }
}
