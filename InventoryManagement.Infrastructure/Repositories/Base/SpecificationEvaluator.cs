using InventoryManagement.Domain.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Infrastructure.Repositories.Base
{
    /// <summary>
    /// Evaluates specifications and builds queries with all the specified criteria
    /// </summary>
    public class SpecificationEvaluator<T> where T : class
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
        {
            var query = inputQuery;

            // Apply filtering
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // Apply ordering
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            // Apply grouping
            if (specification.GroupBy != null)
            {
                query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
            }

            // Apply includes
            query = specification.Includes
                .Aggregate(query, (current, include) => current.Include(include));

            // Apply string-based includes
            query = specification.IncludeStrings
                .Aggregate(query, (current, include) => current.Include(include));

            // Apply paging
            if (specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip)
                             .Take(specification.Take);
            }

            // Apply distinct
            if (specification.IsDistinct)
            {
                query = query.Distinct();
            }

            return query;
        }
    }
}
