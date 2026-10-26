using MedLink.Application.Interfaces.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.infrastructure.Specifications
{
    public class SpecificationEvaluator<T> where T : class
    {
        public static IQueryable<T> GetQuery
            (
             IQueryable<T> inputQuery , 
             ISpecification<T> specification
            )
        {
            var query = inputQuery;
            if (specification.Criteria is not null)
            {
                query = query.Where(specification.Criteria);
            }
            query = specification.Includes.Aggregate(
                query,
                (current, include) => current.Include(include));
            if (specification.OrderBy is not null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }
            else if (specification.OrderByDescending is not null)
            {
                var orderedQuery = query.OrderByDescending(
                specification.OrderByDescending);

                if (specification.ThenByDescending is not null)
                {
                    orderedQuery = orderedQuery.ThenByDescending(
                        specification.ThenByDescending);
                }

                query = orderedQuery;
            }
            if (specification.IsPagingEnabled)
            {
                query = query
               .Skip(specification.Skip)
               .Take(specification.Take);
            }
            return query;
        }

        public static IQueryable<T> GetCountQuery(
            IQueryable<T> inputQuery,
            ISpecification<T> specification)
        {
            var query = inputQuery;
            if (specification.Criteria is not null)
            {
                query = query.Where(specification.Criteria);
            }
            return query;
        }
    }
}
