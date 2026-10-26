using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MedLink.Application.Interfaces.Common
{
    public interface ISpecification<T> where T : class
    {
        // Where :  
        Expression<Func<T,bool>>? Criteria { get; }
        // Includes : 
        List<Expression<Func<T, object>>> Includes { get; }
        // Order-By : 
        Expression<Func<T, object>>? OrderBy { get; }
        // Order-By-Descending :
        Expression<Func<T, object>>? OrderByDescending { get; }
        // Then-By-Descending (secondary/tie-breaker ordering) :
        Expression<Func<T, object>>? ThenByDescending { get; }
        // Paggenation :
        int Skip { get; }

        int Take { get; }

        bool IsPagingEnabled { get; }

    }
}
