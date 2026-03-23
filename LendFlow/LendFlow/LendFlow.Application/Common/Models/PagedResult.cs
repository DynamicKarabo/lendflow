using System.Collections.Generic;

namespace LendFlow.Application.Common.Models;

public record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
);
