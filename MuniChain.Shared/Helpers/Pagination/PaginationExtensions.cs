using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;
namespace Shared.Helpers.Pagination
{
    public static class PaginationExtensions
    {
        // using static System.Linq.Expressions.Expression;

        public static IQueryable<T> Search<T>(this IQueryable<T> source, string[] columnNames, string term)
        {
            if (string.IsNullOrEmpty(term)) { return source; }

            // T is a compile-time placeholder for the element type of the query.
            Type elementType = typeof(T);

            // Get all the string properties on this specific type.
            PropertyInfo[] stringProperties =
                elementType.GetProperties()
                    .Where(x => x.PropertyType == typeof(string) && columnNames.Contains(x.Name))
                    .ToArray();
            if (!stringProperties.Any()) { return source; }

            // Get the right overload of String.Contains
            MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

            // Create a parameter for the expression tree:
            // the 'x' in 'x => x.PropertyName.Contains("term")'
            // The type of this parameter is the query's element type
            ParameterExpression prm = Parameter(elementType);

            // Map each property to an expression tree node
            IEnumerable<Expression> expressions = stringProperties
                .Select(prp =>
                    // For each property, we have to construct an expression tree node like x.PropertyName.Contains("term")
                    Call(                  // .Contains(...) 
                        Property(          // .PropertyName
                            prm,           // x 
                            prp
                        ),
                        containsMethod,
                        Constant(term)     // "term" 
                    )
                );

            // Combine all the resultant expression nodes using ||
            Expression body = expressions
                .Aggregate(
                    (prev, current) => Or(prev, current)
                );

            // Wrap the expression body in a compile-time-typed lambda expression
            Expression<Func<T, bool>> lambda = Lambda<Func<T, bool>>(body, prm);

            // Because the lambda is compile-time-typed (albeit with a generic parameter), we can use it with the Where method
            return source.Where(lambda);
        }

        public async static Task InsertPaginationParametersInResponse<T>(this IQueryable<T> queryable, int recordsPerPage, dynamic retObject)
        {
            double count = await queryable.CountAsync();
            double totalPages = Math.Ceiling(count / recordsPerPage);

            retObject.TotalPages = int.Parse(totalPages.ToString());
            retObject.TotalRecords = int.Parse(count.ToString());
        }
        public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, int currentPage, int recordsPerPage)
        {
            return queryable
                .Skip((currentPage - 1) * recordsPerPage)
                .Take(recordsPerPage);
        }
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string sortField, string sortOrder)
        {
            if (!string.IsNullOrEmpty(sortField))
            {
                var queryElementTypeParam = Parameter(typeof(T));
                var memberAccess = PropertyOrField(queryElementTypeParam, sortField);
                var keySelector = Lambda(memberAccess, queryElementTypeParam);

                var orderBy = Call(
                    typeof(Queryable),
                    sortOrder.ToUpper() == "ASC" ? "OrderBy" : "OrderByDescending",
                    new Type[] { typeof(T), memberAccess.Type },
                    query.Expression,
                    Quote(keySelector));

                return query.Provider.CreateQuery<T>(orderBy);
            }
            return query;
        }
    }
}
