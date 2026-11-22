using System;

namespace NSSLC.Server.Content.Prototypes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class PrototypeAttribute : Attribute
{
  public PrototypeAttribute(string type)
  {
    Type = type ?? throw new ArgumentNullException(nameof(type));
  }

  public string Type { get; }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
internal sealed class IdDataFieldAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
internal sealed class DataFieldAttribute : Attribute
{
  public DataFieldAttribute(bool required = false)
  {
    Required = required;
  }

  public bool Required { get; }
}

