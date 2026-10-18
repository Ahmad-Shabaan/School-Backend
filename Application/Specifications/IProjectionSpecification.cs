using System.Linq.Expressions;

namespace Application.Specifications;

public interface IProjectionSpecification<T, TResult> where T : class
{
    Expression<Func<T, bool>>? Criteria { get; }

    List<Expression<Func<T, object>>> Includes { get; }

    Expression<Func<T, object>>? OrderBy { get; }

    Expression<Func<T, object>>? OrderByDescending { get; }

    int Take { get; }

    int Skip { get; }

    bool IsPagingEnabled { get; }

    Expression<Func<T, TResult>> Projection { get; }
}
