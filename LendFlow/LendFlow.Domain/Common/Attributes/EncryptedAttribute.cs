using System;

namespace LendFlow.Domain.Common.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class EncryptedAttribute : Attribute
{
}
