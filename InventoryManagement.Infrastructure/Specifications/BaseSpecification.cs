using InventoryManagement.Domain.Specifications;
using System.Linq.Expressions;

namespace InventoryManagement.Infrastructure.Specifications
{
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        private readonly List<Expression<Func<T, object>>> _includes = new();
        private readonly List<string> _includeStrings = new();

        protected BaseSpecification()
        {
        }

        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        public Expression<Func<T, bool>> Criteria { get; private set; }
        public List<Expression<Func<T, object>>> Includes => _includes;
        public List<string> IncludeStrings => _includeStrings;
        public Expression<Func<T, object>> OrderBy { get; private set; }
        public Expression<Func<T, object>> OrderByDescending { get; private set; }
        public Expression<Func<T, object>> ThenBy { get; private set; }
        public Expression<Func<T, object>> ThenByDescending { get; private set; }
        public Expression<Func<T, object>> GroupBy { get; private set; }

        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; }
        public bool IsDistinct { get; private set; }
        public bool AsNoTracking { get; private set; } = true;

        protected void AddCriteria(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        protected void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        {
            OrderByDescending = orderByDescExpression;
        }

        protected void ApplyThenBy(Expression<Func<T, object>> thenByExpression)
        {
            ThenBy = thenByExpression;
        }

        protected void ApplyThenByDescending(Expression<Func<T, object>> thenByDescExpression)
        {
            ThenByDescending = thenByDescExpression;
        }

        protected void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
        {
            GroupBy = groupByExpression;
        }

        protected void ApplyDistinct()
        {
            IsDistinct = true;
        }

        protected void ApplyNoTracking()
        {
            AsNoTracking = true;
        }
    }
}
