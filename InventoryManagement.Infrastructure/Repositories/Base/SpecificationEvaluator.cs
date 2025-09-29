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
    public static class SpecificationEvaluator<TEntity> where TEntity : class
    {
        /// <summary>
        /// Applies the specification to the query
        /// </summary>
        /// <param name="inputQuery">The base query</param>
        /// <param name="specification">The specification to apply</param>
        /// <returns>Query with specification applied</returns>
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> specification)
        {
            var query = inputQuery;

            // Apply criteria (WHERE clause)
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // Apply includes
            query = specification.Includes
                .Aggregate(query, (current, include) => current.Include(include));

            // Apply string-based includes (for nested includes)
            query = specification.IncludeStrings
                .Aggregate(query, (current, include) => current.Include(include));

            // Apply ordering
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            // Apply secondary ordering if exists


            // Apply grouping
            if (specification.GroupBy != null)
            {
                query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
            }

            // Apply distinct
            if (specification.IsDistinct)
            {
                query = query.Distinct();
            }

            // Apply paging (should be last)
            if (specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip)
                             .Take(specification.Take);
            }

            return query;
        }

    }
}
