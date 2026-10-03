using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Terraria.WorldStorage.Generators;

[Generator]
public sealed class WorldLoadApiCatalogGenerator : ISourceGenerator
{
  private const string AttributeMetadataName = "Terraria.WorldStorage.WorldLoadApiAttribute";
  private const string InterfaceMetadataName = "Terraria.WorldStorage.IWorldLoadApi`3";
  private const string SectionSchemaAttributeMetadataName =
    "Terraria.NonAuthoritative.Persistence.WorldPersistenceSectionSchemaAttribute";
  private const string CatalogClassName = "WorldLoadApiCatalog";

  private static readonly DiagnosticDescriptor InvalidDeclaration = new(
    "WLA001",
    "Invalid world load API declaration",
    "Type '{0}' must be a non-generic, accessible concrete class with exactly one " +
      "IWorldLoadApi<TContext, TSection, TPrepared> contract whose type arguments are " +
      "accessible, strongly typed, non-nullable types, and WorldLoadApi metadata",
    "WorldLoadApi",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor DuplicateApiId = new(
    "WLA002",
    "Duplicate world load API identity",
    "World load ApiId '{0}' is declared by more than one API",
    "WorldLoadApi",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor DuplicateSectionId = new(
    "WLA003",
    "Duplicate world load section identity",
    "World load SectionId '{0}' is consumed by more than one API",
    "WorldLoadApi",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor InvalidMetadata = new(
    "WLA004",
    "Invalid world load API metadata",
    "World load API '{0}' has invalid metadata: {1}",
    "WorldLoadApi",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor MissingDependency = new(
    "WLA005",
    "Missing world load commit dependency",
    "World load API '{0}' depends on unknown ApiId '{1}'",
    "WorldLoadApi",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor DependencyCycle = new(
    "WLA006",
    "World load commit dependency cycle",
    "World load API '{0}' participates in a CommitAfter dependency cycle",
    "WorldLoadApi",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor InvalidSectionSchema = new(
    "WLA007",
    "Invalid world persistence section schema",
    "World persistence section schema on '{0}' is invalid: {1}",
    "WorldLoadApi",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor DuplicateSchemaSectionId = new(
    "WLA008",
    "Duplicate world persistence section schema identity",
    "World persistence section schema id '{0}' is declared more than once",
    "WorldLoadApi",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor MissingSectionSchema = new(
    "WLA009",
    "Missing world persistence section schema",
    "World load API '{0}' consumes section '{1}' without a declared schema",
    "WorldLoadApi",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor SectionTypeMismatch = new(
    "WLA010",
    "World persistence section type mismatch",
    "World load API '{0}' declares section type '{1}', but schema '{2}' declares '{3}'",
    "WorldLoadApi",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor RequiredSectionHasNoConsumer = new(
    "WLA011",
    "Required world section has no consumer",
    "Required world persistence section '{0}' has no world load API consumer",
    "WorldLoadApi",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  private static readonly DiagnosticDescriptor SectionRequirementMismatch = new(
    "WLA012",
    "World persistence section requirement mismatch",
    "World load API '{0}' requirement does not match the schema requirement for section '{1}'",
    "WorldLoadApi",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);

  public void Initialize(GeneratorInitializationContext context)
  {
  }

  public void Execute(GeneratorExecutionContext context)
  {
    INamedTypeSymbol? apiInterface = context.Compilation.GetTypeByMetadataName(
      InterfaceMetadataName);
    if (apiInterface is null)
    {
      return;
    }

    List<SectionSchema> sectionSchemas = DiscoverSectionSchemas(context.Compilation, context);
    List<ApiDeclaration> declarations = DiscoverDeclarations(
      context.Compilation,
      apiInterface,
      context);

    bool hasErrors = ValidateSectionSchemas(
      declarations,
      sectionSchemas,
      context);
    hasErrors |= ValidateUniqueIdentities(declarations, context);
    hasErrors |= ValidateDependencies(declarations, context);
    if (hasErrors)
    {
      return;
    }

    if (declarations.Count == 0)
    {
      context.AddSource(
        "WorldLoadApiCatalog.g.cs",
        SourceText.From(GenerateCatalog(
            declarations,
            Array.Empty<ApiDeclaration>(),
            GetCatalogNamespace(context.Compilation.AssemblyName)),
          Encoding.UTF8));
      return;
    }

    ApiDeclaration[] preparationOrder = declarations
      .OrderBy(declaration => declaration.ApiId, StringComparer.Ordinal)
      .ToArray();
    if (!TryGetCommitOrder(declarations, context, out ApiDeclaration[] commitOrder))
    {
      return;
    }

    context.AddSource(
      "WorldLoadApiCatalog.g.cs",
      SourceText.From(GenerateCatalog(
          preparationOrder,
          commitOrder,
          GetCatalogNamespace(context.Compilation.AssemblyName)),
        Encoding.UTF8));
  }

  private static List<ApiDeclaration> DiscoverDeclarations(
    Compilation compilation,
    INamedTypeSymbol apiInterface,
    GeneratorExecutionContext context)
  {
    var declarations = new List<ApiDeclaration>();
    foreach (IAssemblySymbol assembly in EnumerateReferencedAssemblies(compilation))
    {
      foreach (INamedTypeSymbol type in EnumerateTypes(assembly.GlobalNamespace))
      {
        bool implementsContract = type.AllInterfaces.Any(interfaceType =>
          SymbolEqualityComparer.Default.Equals(interfaceType.OriginalDefinition, apiInterface));
        AttributeData? attribute = type.GetAttributes().FirstOrDefault(candidate =>
          candidate.AttributeClass?.ToDisplayString() == AttributeMetadataName);

        if (!implementsContract && attribute is null)
        {
          continue;
        }

        Location location = attribute?.ApplicationSyntaxReference?.GetSyntax().GetLocation()
          ?? type.Locations.FirstOrDefault()
          ?? Location.None;

        if (!implementsContract || attribute is null ||
            type.TypeKind != TypeKind.Class ||
            type.IsAbstract ||
            type.IsStatic ||
            type.Arity != 0 ||
            HasGenericContainingType(type) ||
            !compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
        {
          context.ReportDiagnostic(Diagnostic.Create(
            InvalidDeclaration,
            location,
            type.ToDisplayString()));
          continue;
        }

        INamedTypeSymbol[] contracts = type.AllInterfaces
          .Where(interfaceType => SymbolEqualityComparer.Default.Equals(
            interfaceType.OriginalDefinition,
            apiInterface))
          .ToArray();
        if (contracts.Length != 1 || contracts[0].TypeArguments.Any(argument =>
              IsInvalidContractTypeArgument(argument, compilation) ||
              !compilation.IsSymbolAccessibleWithin(argument, compilation.Assembly)))
        {
          context.ReportDiagnostic(Diagnostic.Create(
            InvalidDeclaration,
            location,
            type.ToDisplayString()));
          continue;
        }

        if (!TryReadMetadata(attribute, contracts[0], type, location, context,
              out ApiDeclaration declaration))
        {
          continue;
        }

        declarations.Add(declaration);
      }
    }

    return declarations;
  }

  private static List<SectionSchema> DiscoverSectionSchemas(
    Compilation compilation,
    GeneratorExecutionContext context)
  {
    var schemas = new List<SectionSchema>();
    string providerName = compilation.Assembly.Name;
    foreach (AttributeData attribute in compilation.Assembly.GetAttributes())
    {
      if (attribute.AttributeClass?.ToDisplayString() != SectionSchemaAttributeMetadataName)
      {
        continue;
      }

      Location location = attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation()
        ?? Location.None;
      if (attribute.ConstructorArguments.Length != 3)
      {
        ReportInvalidSectionSchema(
          context,
          location,
          providerName,
          "the schema arguments are incomplete");
        continue;
      }

      string sectionId = ReadString(attribute.ConstructorArguments[0]);
      ITypeSymbol? sectionType = attribute.ConstructorArguments[1].Value as ITypeSymbol;
      int requirement = ReadInt(attribute.ConstructorArguments[2]);
      if (string.IsNullOrWhiteSpace(sectionId) ||
          sectionType is null ||
          IsInvalidContractTypeArgument(sectionType, compilation) ||
          !compilation.IsSymbolAccessibleWithin(sectionType, compilation.Assembly) ||
          requirement is not 0 and not 1)
      {
        ReportInvalidSectionSchema(
          context,
          location,
          providerName,
          "section id, accessible section type, or requirement is invalid");
        continue;
      }

      schemas.Add(new SectionSchema(
        sectionId,
        sectionType,
        requirement,
        location));
    }

    return schemas;
  }

  private static bool ValidateSectionSchemas(
    List<ApiDeclaration> declarations,
    List<SectionSchema> sectionSchemas,
    GeneratorExecutionContext context)
  {
    bool hasErrors = false;
    var schemasById = new Dictionary<string, SectionSchema>(StringComparer.Ordinal);
    var duplicateSchemaIds = new HashSet<string>(StringComparer.Ordinal);
    foreach (IGrouping<string, SectionSchema> group in sectionSchemas.GroupBy(
               schema => schema.SectionId,
               StringComparer.Ordinal))
    {
      if (group.Count() > 1)
      {
        hasErrors = true;
        duplicateSchemaIds.Add(group.Key);
        foreach (SectionSchema schema in group)
        {
          context.ReportDiagnostic(Diagnostic.Create(
            DuplicateSchemaSectionId,
            schema.Location,
            schema.SectionId));
        }

        continue;
      }

      schemasById.Add(group.Key, group.Single());
    }

    foreach (ApiDeclaration declaration in declarations)
    {
      if (duplicateSchemaIds.Contains(declaration.SectionId))
      {
        continue;
      }

      if (!schemasById.TryGetValue(declaration.SectionId, out SectionSchema? schema))
      {
        hasErrors = true;
        context.ReportDiagnostic(Diagnostic.Create(
          MissingSectionSchema,
          declaration.Location,
          declaration.ApiId,
          declaration.SectionId));
        continue;
      }

      if (!SymbolEqualityComparer.Default.Equals(declaration.SectionType, schema.SectionType))
      {
        hasErrors = true;
        context.ReportDiagnostic(Diagnostic.Create(
          SectionTypeMismatch,
          declaration.Location,
          declaration.ApiId,
          declaration.SectionType.ToDisplayString(),
          schema.SectionId,
          schema.SectionType.ToDisplayString()));
      }

      if (declaration.Requirement != schema.Requirement)
      {
        hasErrors = true;
        context.ReportDiagnostic(Diagnostic.Create(
          SectionRequirementMismatch,
          declaration.Location,
          declaration.ApiId,
          declaration.SectionId));
      }
    }

    foreach (SectionSchema schema in sectionSchemas)
    {
      if (schema.Requirement == 0 &&
          !declarations.Any(declaration => declaration.SectionId == schema.SectionId))
      {
        hasErrors = true;
        context.ReportDiagnostic(Diagnostic.Create(
          RequiredSectionHasNoConsumer,
          schema.Location,
          schema.SectionId));
      }
    }

    return hasErrors;
  }

  private static void ReportInvalidSectionSchema(
    GeneratorExecutionContext context,
    Location location,
    string providerName,
    string reason)
  {
    context.ReportDiagnostic(Diagnostic.Create(
      InvalidSectionSchema,
      location,
      providerName,
      reason));
  }

  private static IEnumerable<IAssemblySymbol> EnumerateReferencedAssemblies(
    Compilation compilation)
  {
    var assemblies = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default)
    {
      compilation.Assembly
    };
    foreach (MetadataReference reference in compilation.References)
    {
      if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
      {
        assemblies.Add(assembly);
      }
    }

    return assemblies.OrderBy(
      assembly => assembly.Identity.ToString(),
      StringComparer.Ordinal);
  }

  private static IEnumerable<INamedTypeSymbol> EnumerateTypes(INamespaceSymbol namespaceSymbol)
  {
    foreach (INamespaceSymbol childNamespace in namespaceSymbol.GetNamespaceMembers()
               .OrderBy(child => child.MetadataName, StringComparer.Ordinal))
    {
      foreach (INamedTypeSymbol type in EnumerateTypes(childNamespace))
      {
        yield return type;
      }
    }

    foreach (INamedTypeSymbol type in namespaceSymbol.GetTypeMembers()
               .OrderBy(type => type.MetadataName, StringComparer.Ordinal))
    {
      foreach (INamedTypeSymbol nestedType in EnumerateTypeAndNestedTypes(type))
      {
        yield return nestedType;
      }
    }
  }

  private static IEnumerable<INamedTypeSymbol> EnumerateTypeAndNestedTypes(
    INamedTypeSymbol type)
  {
    yield return type;
    foreach (INamedTypeSymbol nestedType in type.GetTypeMembers()
               .OrderBy(nested => nested.MetadataName, StringComparer.Ordinal))
    {
      foreach (INamedTypeSymbol descendant in EnumerateTypeAndNestedTypes(nestedType))
      {
        yield return descendant;
      }
    }
  }

  private static bool HasGenericContainingType(INamedTypeSymbol type)
  {
    for (INamedTypeSymbol? containingType = type.ContainingType;
         containingType is not null;
         containingType = containingType.ContainingType)
    {
      if (containingType.Arity != 0)
      {
        return true;
      }
    }

    return false;
  }

  private static bool IsInvalidContractTypeArgument(
    ITypeSymbol type,
    Compilation compilation)
  {
    return type.IsRefLikeType ||
      type.TypeKind is TypeKind.Error or TypeKind.Dynamic or TypeKind.Pointer or
        TypeKind.FunctionPointer ||
      type.SpecialType is SpecialType.System_Object or SpecialType.System_Void ||
      type.NullableAnnotation == NullableAnnotation.Annotated ||
      type is INamedTypeSymbol namedType &&
        namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T ||
      IsWorldLoadBindingsType(type, compilation);
  }

  private static bool IsWorldLoadBindingsType(ITypeSymbol type, Compilation compilation)
  {
    INamedTypeSymbol? bindingsType = compilation.GetTypeByMetadataName(
      "Terraria.WorldStorage.WorldLoadApiBindings");
    INamedTypeSymbol? runtimeBindingsType = compilation.GetTypeByMetadataName(
      "Terraria.WorldStorage.WorldLoadApiRuntimeBindings");
    for (INamedTypeSymbol? current = type as INamedTypeSymbol;
         current is not null;
         current = current.BaseType)
    {
      if (SymbolEqualityComparer.Default.Equals(current, bindingsType) ||
          SymbolEqualityComparer.Default.Equals(current, runtimeBindingsType))
      {
        return true;
      }
    }

    return false;
  }

  private static bool TryReadMetadata(
    AttributeData attribute,
    INamedTypeSymbol contract,
    INamedTypeSymbol apiType,
    Location location,
    GeneratorExecutionContext context,
    out ApiDeclaration declaration)
  {
    declaration = null!;
    if (attribute.ConstructorArguments.Length < 6)
    {
      ReportInvalidMetadata(context, location, "<unknown>", "the attribute arguments are incomplete");
      return false;
    }

    string apiId = ReadString(attribute.ConstructorArguments[0]);
    string ownerId = ReadString(attribute.ConstructorArguments[1]);
    string sectionId = ReadString(attribute.ConstructorArguments[2]);
    int minimumVersion = ReadInt(attribute.ConstructorArguments[3]);
    int maximumVersion = ReadInt(attribute.ConstructorArguments[4]);
    int requirement = ReadInt(attribute.ConstructorArguments[5]);
    string[] dependencies = Array.Empty<string>();
    if (attribute.ConstructorArguments.Length > 6)
    {
      TypedConstant dependencyArgument = attribute.ConstructorArguments[6];
      if (dependencyArgument.Kind != TypedConstantKind.Array || dependencyArgument.IsNull)
      {
        ReportInvalidMetadata(context, location, ReadString(attribute.ConstructorArguments[0]),
          "CommitAfter must be a non-null string array");
        return false;
      }

      dependencies = dependencyArgument.Values.Select(ReadString).ToArray();
    }

    string displayApiId = string.IsNullOrWhiteSpace(apiId) ? "<empty>" : apiId;
    if (string.IsNullOrWhiteSpace(apiId) ||
        string.IsNullOrWhiteSpace(ownerId) ||
        string.IsNullOrWhiteSpace(sectionId))
    {
      ReportInvalidMetadata(context, location, displayApiId,
        "ApiId, OwnerId and SectionId must be non-empty");
      return false;
    }

    if (minimumVersion < 0 || maximumVersion < minimumVersion)
    {
      ReportInvalidMetadata(context, location, apiId,
        "the supported format version range is invalid");
      return false;
    }

    if (requirement is not 0 and not 1)
    {
      ReportInvalidMetadata(context, location, apiId,
        "the section requirement must be Required or Optional");
      return false;
    }

    if (dependencies.Any(string.IsNullOrWhiteSpace) ||
        dependencies.Distinct(StringComparer.Ordinal).Count() != dependencies.Length ||
        dependencies.Contains(apiId, StringComparer.Ordinal))
    {
      ReportInvalidMetadata(context, location, apiId,
        "CommitAfter must contain distinct, non-empty ApiIds and cannot reference itself");
      return false;
    }

    declaration = new ApiDeclaration(
      apiId,
      ownerId,
      sectionId,
      minimumVersion,
      maximumVersion,
      requirement,
      dependencies,
      apiType,
      contract.TypeArguments[0],
      contract.TypeArguments[1],
      contract.TypeArguments[2],
      location);
    return true;
  }

  private static string ReadString(TypedConstant value)
  {
    return value.Value as string ?? string.Empty;
  }

  private static int ReadInt(TypedConstant value)
  {
    return value.Value is null
      ? int.MinValue
      : Convert.ToInt32(value.Value, CultureInfo.InvariantCulture);
  }

  private static void ReportInvalidMetadata(
    GeneratorExecutionContext context,
    Location location,
    string apiId,
    string reason)
  {
    context.ReportDiagnostic(Diagnostic.Create(InvalidMetadata, location, apiId, reason));
  }

  private static bool ValidateUniqueIdentities(
    List<ApiDeclaration> declarations,
    GeneratorExecutionContext context)
  {
    bool hasErrors = false;
    foreach (IGrouping<string, ApiDeclaration> group in declarations.GroupBy(
               declaration => declaration.ApiId,
               StringComparer.Ordinal))
    {
      if (group.Count() > 1)
      {
        hasErrors = true;
        foreach (ApiDeclaration declaration in group)
        {
          context.ReportDiagnostic(Diagnostic.Create(
            DuplicateApiId,
            declaration.Location,
            declaration.ApiId));
        }
      }
    }

    foreach (IGrouping<string, ApiDeclaration> group in declarations.GroupBy(
               declaration => declaration.SectionId,
               StringComparer.Ordinal))
    {
      if (group.Count() > 1)
      {
        hasErrors = true;
        foreach (ApiDeclaration declaration in group)
        {
          context.ReportDiagnostic(Diagnostic.Create(
            DuplicateSectionId,
            declaration.Location,
            declaration.SectionId));
        }
      }
    }

    return hasErrors;
  }

  private static bool ValidateDependencies(
    List<ApiDeclaration> declarations,
    GeneratorExecutionContext context)
  {
    bool hasErrors = false;
    var apiIds = new HashSet<string>(declarations.Select(item => item.ApiId),
      StringComparer.Ordinal);
    foreach (ApiDeclaration declaration in declarations)
    {
      foreach (string dependency in declaration.CommitAfter)
      {
        if (!apiIds.Contains(dependency))
        {
          hasErrors = true;
          context.ReportDiagnostic(Diagnostic.Create(
            MissingDependency,
            declaration.Location,
            declaration.ApiId,
            dependency));
        }
      }
    }

    return hasErrors;
  }

  private static bool TryGetCommitOrder(
    List<ApiDeclaration> declarations,
    GeneratorExecutionContext context,
    out ApiDeclaration[] commitOrder)
  {
    var byId = declarations.ToDictionary(item => item.ApiId, StringComparer.Ordinal);
    var indegree = declarations.ToDictionary(
      item => item.ApiId,
      item => item.CommitAfter.Length,
      StringComparer.Ordinal);
    var dependents = declarations.ToDictionary(
      item => item.ApiId,
      _ => new List<string>(),
      StringComparer.Ordinal);

    foreach (ApiDeclaration declaration in declarations)
    {
      foreach (string dependency in declaration.CommitAfter)
      {
        dependents[dependency].Add(declaration.ApiId);
      }
    }

    var ready = new SortedSet<string>(indegree
      .Where(pair => pair.Value == 0)
      .Select(pair => pair.Key), StringComparer.Ordinal);
    var result = new List<ApiDeclaration>(declarations.Count);
    while (ready.Count > 0)
    {
      string apiId = ready.Min!;
      ready.Remove(apiId);
      result.Add(byId[apiId]);
      foreach (string dependent in dependents[apiId].OrderBy(value => value,
                 StringComparer.Ordinal))
      {
        indegree[dependent]--;
        if (indegree[dependent] == 0)
        {
          ready.Add(dependent);
        }
      }
    }

    if (result.Count == declarations.Count)
    {
      commitOrder = result.ToArray();
      return true;
    }

    var orderedIds = new HashSet<string>(result.Select(item => item.ApiId),
      StringComparer.Ordinal);
    foreach (ApiDeclaration declaration in declarations
               .Where(item => !orderedIds.Contains(item.ApiId) &&
                 IsInDependencyCycle(item.ApiId, byId))
               .OrderBy(item => item.ApiId, StringComparer.Ordinal))
    {
      context.ReportDiagnostic(Diagnostic.Create(
        DependencyCycle,
        declaration.Location,
        declaration.ApiId));
    }

    commitOrder = Array.Empty<ApiDeclaration>();
    return false;
  }

  private static bool IsInDependencyCycle(
    string apiId,
    Dictionary<string, ApiDeclaration> declarationsById)
  {
    var visited = new HashSet<string>(StringComparer.Ordinal);
    var pending = new Stack<string>(declarationsById[apiId].CommitAfter);
    while (pending.Count > 0)
    {
      string currentApiId = pending.Pop();
      if (currentApiId == apiId)
      {
        return true;
      }

      if (!visited.Add(currentApiId))
      {
        continue;
      }

      foreach (string dependency in declarationsById[currentApiId].CommitAfter)
      {
        pending.Push(dependency);
      }
    }

    return false;
  }

  private static string GenerateCatalog(
    IReadOnlyList<ApiDeclaration> preparationOrder,
    IReadOnlyList<ApiDeclaration> commitOrder,
    string catalogNamespace)
  {
    var source = new StringBuilder();
    source.AppendLine("#nullable enable");
    source.AppendLine("namespace " + catalogNamespace + ";");
    source.AppendLine();
    source.AppendLine("public static class " + CatalogClassName);
    source.AppendLine("{");
    AppendDescriptors(source, preparationOrder);
    AppendExecuteMethod(source, preparationOrder, commitOrder);
    source.AppendLine("}");
    return source.ToString();
  }

  private static string GetCatalogNamespace(string? assemblyName)
  {
    string name = string.IsNullOrWhiteSpace(assemblyName) ? "UnknownAssembly" : assemblyName;
    var safeName = new StringBuilder(name.Length + 1);
    foreach (char character in name)
    {
      safeName.Append(char.IsLetterOrDigit(character) || character == '_'
        ? character
        : '_');
    }

    if (safeName.Length == 0 || char.IsDigit(safeName[0]))
    {
      safeName.Insert(0, '_');
    }

    return "Terraria.WorldStorage.Generated.@" + safeName;
  }

  private static void AppendDescriptors(
    StringBuilder source,
    IReadOnlyList<ApiDeclaration> declarations)
  {
    source.AppendLine("  private static readonly global::System.Collections.Generic.IReadOnlyList<");
    source.AppendLine("    global::Terraria.WorldStorage.WorldLoadApiDescriptor> _descriptors =");
    source.AppendLine("      global::System.Array.AsReadOnly(new global::Terraria.WorldStorage.");
    source.AppendLine("        WorldLoadApiDescriptor[]");
    source.AppendLine("        {");
    foreach (ApiDeclaration declaration in declarations)
    {
      source.AppendLine("          new global::Terraria.WorldStorage.WorldLoadApiDescriptor(");
      source.AppendLine("            " + Literal(declaration.ApiId) + ",");
      source.AppendLine("            " + Literal(declaration.OwnerId) + ",");
      source.AppendLine("            " + Literal(declaration.SectionId) + ",");
      source.AppendLine("            " + declaration.MinimumFormatVersion.ToString(CultureInfo.InvariantCulture) + ",");
      source.AppendLine("            " + declaration.MaximumFormatVersion.ToString(CultureInfo.InvariantCulture) + ",");
      string requirement = declaration.Requirement == 0 ? "Required" : "Optional";
      source.AppendLine("            global::Terraria.WorldStorage.WorldLoadSectionRequirement." +
        requirement + ",");
      source.Append("            new string[] { ");
      source.Append(string.Join(", ", declaration.CommitAfter.Select(Literal)));
      source.AppendLine(" }),");
    }

    source.AppendLine("        });");
    source.AppendLine();
    source.AppendLine("  public static global::System.Collections.Generic.IReadOnlyList<");
    source.AppendLine("    global::Terraria.WorldStorage.WorldLoadApiDescriptor> Descriptors => _descriptors;");
    source.AppendLine();
  }

  private static void AppendExecuteMethod(
    StringBuilder source,
    IReadOnlyList<ApiDeclaration> preparationOrder,
    IReadOnlyList<ApiDeclaration> commitOrder)
  {
    source.AppendLine("  public static global::Terraria.WorldStorage.WorldLoadApiExecutionResult Execute(");
    source.AppendLine("    global::Terraria.WorldStorage.WorldLoadApiBindings bindings)");
    source.AppendLine("  {");
    source.AppendLine("    if (bindings is null)");
    source.AppendLine("    {");
    source.AppendLine("      return global::Terraria.WorldStorage.WorldLoadApiExecutionResult.Failed(");
    source.AppendLine("        global::Terraria.WorldStorage.WorldLoadApiStage.Binding,");
    source.AppendLine("        null,");
    source.AppendLine("        null,");
    source.AppendLine("        global::Terraria.WorldStorage.WorldLoadApiFailure.Create(");
    source.AppendLine("          \"MissingBindings\", \"World load API bindings were not supplied.\"));");
    source.AppendLine("    }");
    if (preparationOrder.Count == 0)
    {
      source.AppendLine("    return global::Terraria.WorldStorage.WorldLoadApiExecutionResult.Failed(");
      source.AppendLine("      global::Terraria.WorldStorage.WorldLoadApiStage.Binding,");
      source.AppendLine("      null,");
      source.AppendLine("      null,");
      source.AppendLine("      global::Terraria.WorldStorage.WorldLoadApiFailure.Create(");
      source.AppendLine(
        "        \"MissingApiDeclarations\", \"No world load API declarations were found.\"));");
      source.AppendLine("  }");
      source.AppendLine();
      return;
    }

    source.AppendLine("    int formatVersion = 0;");

    for (int index = 0; index < preparationOrder.Count; index++)
    {
      ApiDeclaration declaration = preparationOrder[index];
      string apiType = TypeName(declaration.ApiType);
      string contextType = TypeName(declaration.ContextType);
      string sectionType = TypeName(declaration.SectionType);
      string preparedType = TypeName(declaration.PreparedType);
      source.Append("    const string ownerId").Append(index).Append(" = ")
        .Append(Literal(declaration.OwnerId)).AppendLine(";");
      source.Append("    const string sectionId").Append(index).Append(" = ")
        .Append(Literal(declaration.SectionId)).AppendLine(";");
      source.Append("    ").Append(apiType).Append(" api").Append(index).AppendLine(" = default!;");
      source.Append("    global::Terraria.WorldStorage.IWorldLoadApi<")
        .Append(contextType).Append(", ").Append(sectionType).Append(", ")
        .Append(preparedType).Append("> endpoint").Append(index).AppendLine(" = default!;");
      source.Append("    ").Append(contextType).Append(" ownerContext").Append(index)
        .AppendLine(" = default!;");
      source.Append("    global::Terraria.WorldStorage.WorldLoadSection<")
        .Append(sectionType).Append("> section").Append(index).AppendLine(" = default;");
      source.Append("    ").Append(preparedType).Append(" prepared").Append(index)
        .AppendLine(" = default!;");
      source.Append("    bool hasPrepared").Append(index).AppendLine(" = false;");
    }

    source.AppendLine();
    AppendDiscardLocalFunction(source, preparationOrder);
    source.AppendLine("    try");
    source.AppendLine("    {");
    source.AppendLine("      formatVersion = bindings.FormatVersion;");
    source.AppendLine("      bindings.CancellationToken.ThrowIfCancellationRequested();");
    for (int index = 0; index < preparationOrder.Count; index++)
    {
      AppendBindingPreflight(source, preparationOrder[index], index);
    }

    source.AppendLine("    }");
    source.AppendLine("    catch (global::System.Exception exception)");
    source.AppendLine("    {");
    source.AppendLine("      return global::Terraria.WorldStorage.WorldLoadApiExecutionResult.Failed(");
    source.AppendLine("        global::Terraria.WorldStorage.WorldLoadApiStage.Binding,");
    source.AppendLine("        null,");
    source.AppendLine("        null,");
    source.AppendLine("        global::Terraria.WorldStorage.WorldLoadApiFailure.Create(");
    source.AppendLine("          \"BindingException\", \"World load API binding failed.\"),");
    source.AppendLine("        exception);");
    source.AppendLine("    }");

    for (int index = 0; index < preparationOrder.Count; index++)
    {
      AppendPrepareCall(source, preparationOrder[index], index);
    }

    for (int index = 0; index < commitOrder.Count; index++)
    {
      int apiIndex = IndexOf(preparationOrder, commitOrder[index]);
      AppendCommitCall(source, commitOrder[index], apiIndex);
    }

    source.AppendLine("    return global::Terraria.WorldStorage.WorldLoadApiExecutionResult.Completed();");
    source.AppendLine("  }");
    source.AppendLine();
  }

  private static void AppendDiscardLocalFunction(
    StringBuilder source,
    IReadOnlyList<ApiDeclaration> preparationOrder)
  {
    source.AppendLine("    global::System.Exception? DiscardPreparedResults()");
    source.AppendLine("    {");
    source.AppendLine("      global::System.Collections.Generic.List<");
    source.AppendLine("        global::System.Exception>? cleanupExceptions = null;");
    for (int index = preparationOrder.Count - 1; index >= 0; index--)
    {
      source.Append("      if (hasPrepared").Append(index).AppendLine(")");
      source.AppendLine("      {");
      source.AppendLine("        try");
      source.AppendLine("        {");
      source.Append("          endpoint").Append(index).Append(".DiscardPrepared(ownerContext")
        .Append(index).Append(", in prepared").Append(index).AppendLine(");");
      source.AppendLine("        }");
      source.AppendLine("        catch (global::System.Exception exception)");
      source.AppendLine("        {");
      source.AppendLine(
        "          cleanupExceptions ??= new global::System.Collections.Generic.List<");
      source.AppendLine("            global::System.Exception>();");
      source.AppendLine("          cleanupExceptions.Add(exception);");
      source.AppendLine("        }");
      source.AppendLine("        finally");
      source.AppendLine("        {");
      source.Append("          hasPrepared").Append(index).AppendLine(" = false;");
      source.AppendLine("        }");
      source.AppendLine("      }");
    }

    source.AppendLine("      return cleanupExceptions is null");
    source.AppendLine("        ? null");
    source.AppendLine("        : new global::System.AggregateException(cleanupExceptions);");
    source.AppendLine("    }");
    source.AppendLine();
  }

  private static void AppendBindingPreflight(
    StringBuilder source,
    ApiDeclaration declaration,
    int index)
  {
    string apiId = Literal(declaration.ApiId);
    string apiType = TypeName(declaration.ApiType);
    string contextType = TypeName(declaration.ContextType);
    string sectionType = TypeName(declaration.SectionType);
    string unsupportedFormatMessage =
      "The world format version is not supported by API " + declaration.ApiId + ".";
    source.Append("      if (formatVersion < ")
      .Append(declaration.MinimumFormatVersion.ToString(CultureInfo.InvariantCulture))
      .Append(" || formatVersion > ")
      .Append(declaration.MaximumFormatVersion.ToString(CultureInfo.InvariantCulture))
      .AppendLine(")");
    source.AppendLine("      {");
    source.AppendLine("        return global::Terraria.WorldStorage.WorldLoadApiExecutionResult.Failed(");
    source.AppendLine("          global::Terraria.WorldStorage.WorldLoadApiStage.Binding,");
    source.Append("          ").Append(apiId).AppendLine(",");
    source.Append("          ").Append(Literal(declaration.OwnerId)).AppendLine(",");
    source.AppendLine("          global::Terraria.WorldStorage.WorldLoadApiFailure.Create(");
    source.Append("            \"UnsupportedFormatVersion\", ")
      .Append(Literal(unsupportedFormatMessage))
      .AppendLine("));");
    source.AppendLine("      }");
    source.Append("      ").Append(apiType).Append(" resolvedApi").Append(index)
      .AppendLine(";");
    source.Append("      bool hasApi").Append(index).AppendLine(";");
    source.AppendLine("      try");
    source.AppendLine("      {");
    source.Append("        hasApi").Append(index).Append(" = bindings.TryGetApi<")
      .Append(apiType).Append(">(out resolvedApi").Append(index).AppendLine(");");
    source.AppendLine("      }");
    source.AppendLine("      catch (global::System.Exception exception)");
    source.AppendLine("      {");
    AppendBindingException(source, declaration, "ApiBindingException",
      "The world load API binding lookup threw an exception.");
    source.AppendLine("      }");
    source.Append("      if (!hasApi").Append(index).Append(" || resolvedApi")
      .Append(index).AppendLine(" is null)");
    AppendBindingFailure(source, declaration, "MissingApi", "The world load API instance is unavailable.");
    source.Append("      api").Append(index).Append(" = resolvedApi").Append(index).AppendLine(";");
    source.Append("      endpoint").Append(index).Append(" = api").Append(index).AppendLine(";");
    source.Append("      bool hasOwnerContext").Append(index).AppendLine(";");
    source.AppendLine("      try");
    source.AppendLine("      {");
    source.Append("        hasOwnerContext").Append(index).Append(" = bindings.TryGetOwnerContext<")
      .Append(contextType).Append(">(ownerId").Append(index).Append(", out ownerContext")
      .Append(index).AppendLine(");");
    source.AppendLine("      }");
    source.AppendLine("      catch (global::System.Exception exception)");
    source.AppendLine("      {");
    AppendBindingException(source, declaration, "OwnerContextBindingException",
      "The world load owner context lookup threw an exception.");
    source.AppendLine("      }");
    source.Append("      if (!hasOwnerContext").Append(index);
    if (declaration.ContextType.IsReferenceType)
    {
      source.Append(" || ownerContext").Append(index).AppendLine(" is null)");
    }
    else
    {
      source.AppendLine(")");
    }

    AppendBindingFailure(source, declaration, "MissingOwnerContext",
      "The owner context is unavailable.");
    source.Append("      bool hasSection").Append(index).AppendLine(";");
    source.AppendLine("      try");
    source.AppendLine("      {");
    source.Append("        hasSection").Append(index).Append(" = bindings.TryGetSection<")
      .Append(sectionType).Append(">(sectionId").Append(index).Append(", out section")
      .Append(index).AppendLine(");");
    source.AppendLine("      }");
    source.AppendLine("      catch (global::System.Exception exception)");
    source.AppendLine("      {");
    AppendBindingException(source, declaration, "SectionBindingException",
      "The world load section lookup threw an exception.");
    source.AppendLine("      }");
    source.Append("      if (!hasSection").Append(index).AppendLine(")");
    if (declaration.Requirement == 0)
    {
      AppendBindingFailure(source, declaration, "MissingRequiredSection",
        "A required world section is unavailable.");
    }
    else
    {
      source.AppendLine("      {");
      source.Append("        section").Append(index)
        .Append(" = global::Terraria.WorldStorage.WorldLoadSection<")
        .Append(sectionType).AppendLine(">.Absent;");
      source.AppendLine("      }");
    }

    if (declaration.Requirement == 0)
    {
      source.Append("      if (!section").Append(index).AppendLine(".IsPresent)");
      AppendBindingFailure(source, declaration, "MissingRequiredSection",
        "A required world section is absent.");
    }
  }

  private static void AppendBindingException(
    StringBuilder source,
    ApiDeclaration declaration,
    string code,
    string message)
  {
    source.AppendLine("        return global::Terraria.WorldStorage.WorldLoadApiExecutionResult.Failed(");
    source.AppendLine("          global::Terraria.WorldStorage.WorldLoadApiStage.Binding,");
    source.Append("          ").Append(Literal(declaration.ApiId)).AppendLine(",");
    source.Append("          ").Append(Literal(declaration.OwnerId)).AppendLine(",");
    source.AppendLine("          global::Terraria.WorldStorage.WorldLoadApiFailure.Create(");
    source.Append("            ").Append(Literal(code)).Append(", ")
      .Append(Literal(message)).AppendLine("),");
    source.AppendLine("          exception);");
  }

  private static void AppendBindingFailure(
    StringBuilder source,
    ApiDeclaration declaration,
    string failureCode,
    string message)
  {
    source.AppendLine("      {");
    source.AppendLine("        return global::Terraria.WorldStorage.WorldLoadApiExecutionResult.Failed(");
    source.AppendLine("          global::Terraria.WorldStorage.WorldLoadApiStage.Binding,");
    source.Append("          ").Append(Literal(declaration.ApiId)).AppendLine(",");
    source.Append("          ").Append(Literal(declaration.OwnerId)).AppendLine(",");
    source.AppendLine("          global::Terraria.WorldStorage.WorldLoadApiFailure.Create(");
    source.Append("            ").Append(Literal(failureCode)).Append(", ")
      .Append(Literal(message)).AppendLine("));");
    source.AppendLine("      }");
  }

  private static void AppendPrepareCall(
    StringBuilder source,
    ApiDeclaration declaration,
    int index)
  {
    string apiId = Literal(declaration.ApiId);
    string ownerId = Literal(declaration.OwnerId);
    string preparedType = TypeName(declaration.PreparedType);
    source.AppendLine("    global::Terraria.WorldStorage.WorldLoadPrepareResult<" +
      preparedType + "> prepareResult" + index + ";");
    source.AppendLine("    try");
    source.AppendLine("    {");
    source.AppendLine("      bindings.CancellationToken.ThrowIfCancellationRequested();");
    source.Append("      prepareResult").Append(index).Append(" = endpoint").Append(index)
      .Append(".PrepareLoad(ownerContext").Append(index).Append(", section").Append(index)
      .AppendLine(");");
    source.AppendLine("    }");
    source.AppendLine("    catch (global::System.Exception exception)");
    source.AppendLine("    {");
    source.AppendLine("      global::System.Exception? cleanupException = DiscardPreparedResults();");
    AppendExecutionFailure(source, "Preparation", apiId, ownerId, "PrepareException",
      "World load preparation threw an exception.", "exception", "cleanupException");
    source.AppendLine("    }");
    source.Append("    if (!prepareResult").Append(index).AppendLine(".IsPrepared)");
    source.AppendLine("    {");
    source.Append("      global::Terraria.WorldStorage.WorldLoadApiFailure prepareFailure")
      .Append(index).Append(" = prepareResult").Append(index).AppendLine(".Failure;");
    source.Append("      if (!prepareFailure").Append(index).AppendLine(".IsValid)");
    source.AppendLine("      {");
    source.Append("        prepareFailure").Append(index)
      .AppendLine(" = global::Terraria.WorldStorage.WorldLoadApiFailure.Create(");
    source.AppendLine("          \"InvalidPrepareResult\",");
    source.AppendLine("          \"Rejected preparation did not provide a valid failure.\");");
    source.AppendLine("      }");
    source.AppendLine("      global::System.Exception? cleanupException = DiscardPreparedResults();");
    source.Append("      return global::Terraria.WorldStorage.WorldLoadApiExecutionResult.Failed(")
      .AppendLine();
    source.AppendLine("        global::Terraria.WorldStorage.WorldLoadApiStage.Preparation,");
    source.Append("        ").Append(apiId).AppendLine(",");
    source.Append("        ").Append(ownerId).AppendLine(",");
    source.Append("        prepareFailure").Append(index).AppendLine(",");
    source.AppendLine("        cleanupException: cleanupException);");
    source.AppendLine("    }");
    source.Append("    prepared").Append(index).Append(" = prepareResult").Append(index)
      .AppendLine(".PreparedData;");
    source.Append("    hasPrepared").Append(index).AppendLine(" = true;");
    source.AppendLine();
  }

  private static void AppendCommitCall(
    StringBuilder source,
    ApiDeclaration declaration,
    int index)
  {
    string apiId = Literal(declaration.ApiId);
    string ownerId = Literal(declaration.OwnerId);
    source.Append("    global::Terraria.WorldStorage.WorldLoadCommitResult commitResult")
      .Append(index).AppendLine(";");
    source.AppendLine("    try");
    source.AppendLine("    {");
    source.AppendLine("      bindings.CancellationToken.ThrowIfCancellationRequested();");
    source.Append("      commitResult").Append(index).Append(" = endpoint").Append(index)
      .Append(".CommitLoad(ownerContext").Append(index).Append(", in prepared")
      .Append(index).AppendLine(");");
    source.AppendLine("    }");
    source.AppendLine("    catch (global::System.Exception exception)");
    source.AppendLine("    {");
    source.Append("      hasPrepared").Append(index).AppendLine(" = false;");
    source.AppendLine("      global::System.Exception? cleanupException = DiscardPreparedResults();");
    AppendExecutionFailure(source, "Commit", apiId, ownerId, "CommitException",
      "World load commit threw an exception.", "exception", "cleanupException");
    source.AppendLine("    }");
    source.Append("    hasPrepared").Append(index).AppendLine(" = false;");
    source.Append("    if (!commitResult").Append(index).AppendLine(".Succeeded ||");
    source.Append("        commitResult").Append(index).AppendLine(".Failure.IsValid)");
    source.AppendLine("    {");
    source.Append("      global::Terraria.WorldStorage.WorldLoadApiFailure commitFailure")
      .Append(index).Append(" = commitResult").Append(index).AppendLine(".Failure;");
    source.Append("      if (commitResult").Append(index).AppendLine(".Succeeded)");
    source.AppendLine("      {");
    source.Append("        commitFailure").Append(index)
      .AppendLine(" = global::Terraria.WorldStorage.WorldLoadApiFailure.Create(");
    source.AppendLine("          \"InvalidCommitResult\",");
    source.AppendLine("          \"Commit returned success with a failure.\");");
    source.AppendLine("      }");
    source.Append("      else if (!commitFailure").Append(index).AppendLine(".IsValid)");
    source.AppendLine("      {");
    source.Append("        commitFailure").Append(index)
      .AppendLine(" = global::Terraria.WorldStorage.WorldLoadApiFailure.Create(");
    source.AppendLine("          \"InvalidCommitResult\",");
    source.AppendLine("          \"Rejected commit did not provide a valid failure.\");");
    source.AppendLine("      }");
    source.AppendLine("      global::System.Exception? cleanupException = DiscardPreparedResults();");
    source.AppendLine("      return global::Terraria.WorldStorage.WorldLoadApiExecutionResult.Failed(");
    source.AppendLine("        global::Terraria.WorldStorage.WorldLoadApiStage.Commit,");
    source.Append("        ").Append(apiId).AppendLine(",");
    source.Append("        ").Append(ownerId).AppendLine(",");
    source.Append("        commitFailure").Append(index).AppendLine(",");
    source.AppendLine("        cleanupException: cleanupException);");
    source.AppendLine("    }");
    source.AppendLine();
  }

  private static void AppendExecutionFailure(
    StringBuilder source,
    string stage,
    string apiId,
    string ownerId,
    string code,
    string message,
    string exception,
    string cleanupException)
  {
    source.AppendLine("      return global::Terraria.WorldStorage.WorldLoadApiExecutionResult.Failed(");
    source.Append("        global::Terraria.WorldStorage.WorldLoadApiStage.").Append(stage)
      .AppendLine(",");
    source.Append("        ").Append(apiId).AppendLine(",");
    source.Append("        ").Append(ownerId).AppendLine(",");
    source.AppendLine("        global::Terraria.WorldStorage.WorldLoadApiFailure.Create(");
    source.Append("          ").Append(Literal(code)).Append(", ")
      .Append(Literal(message)).AppendLine("),");
    source.Append("        ").Append(exception).AppendLine(",");
    source.Append("        ").Append(cleanupException).AppendLine(");");
  }

  private static string Literal(string value)
  {
    return SymbolDisplay.FormatLiteral(value, quote: true);
  }

  private static string TypeName(ITypeSymbol type)
  {
    return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
  }

  private static int IndexOf(
    IReadOnlyList<ApiDeclaration> declarations,
    ApiDeclaration declaration)
  {
    for (int index = 0; index < declarations.Count; index++)
    {
      if (ReferenceEquals(declarations[index], declaration))
      {
        return index;
      }
    }

    throw new InvalidOperationException("The API was not found in the preparation order.");
  }

  private sealed record ApiDeclaration(
    string ApiId,
    string OwnerId,
    string SectionId,
    int MinimumFormatVersion,
    int MaximumFormatVersion,
    int Requirement,
    string[] CommitAfter,
    INamedTypeSymbol ApiType,
    ITypeSymbol ContextType,
    ITypeSymbol SectionType,
    ITypeSymbol PreparedType,
    Location Location);

  private sealed record SectionSchema(
    string SectionId,
    ITypeSymbol SectionType,
    int Requirement,
    Location Location);
}
