using System;

namespace LendFlow.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object key)
        : base($"Entity \"{entity}\" ({key}) was not found.")
    {
    }
}
