using System;
using System.Linq;

namespace Guildly.Common.Database.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> SkipAndTake<TEntity>(this IQueryable<TEntity> query, int? skip, int? take) 
        where TEntity : class
    {
        if (skip != null)
        {
            if (skip.Value < 0)
            {
                throw new InvalidOperationException("Skip value must be greater than or equal to 0.");
            }
            query = query.Skip(skip.Value);
        }

        if (take != null)
        {
            if (take.Value <= 0)
            {
                throw new InvalidOperationException("Take value must be greater than 0.");
            }
            query = query.Take(take.Value);
        }

        return query;
    }
}