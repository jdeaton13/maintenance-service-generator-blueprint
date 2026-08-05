using System.Text.Json;
using System.Text.Json.Serialization;

namespace Maintenance.Generator.Contracts;

public sealed record MaintenanceServiceSpec(
    int SchemaVersion,
    ServiceSpec Service,
    EntitySpec Entity,
    IReadOnlyList<ValidationSpec> Validations,
    IReadOnlyList<BusinessRuleSpec> BusinessRules,
    IReadOnlyList<SideEffectSpec> SideEffects,
    IReadOnlyList<ScenarioSpec> Scenarios);

public sealed record ServiceSpec(
    string Name,
    string Route,
    MaintenanceOperation Operation,
    FailureMode FailureMode);

public enum MaintenanceOperation
{
    BulkInsert,
    BulkUpsert
}

public enum FailureMode
{
    AtomicBatch,
    PartialBatch
}

public sealed record EntitySpec(
    string Name,
    IReadOnlyList<string> Key,
    IReadOnlyList<PropertySpec> Properties);

public sealed record PropertySpec(
    string Name,
    string Type,
    bool Required,
    int? MinimumLength = null,
    int? MaximumLength = null,
    IReadOnlyList<JsonElement>? AllowedValues = null);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(RequiredValidationSpec), "required")]
[JsonDerivedType(typeof(MaximumLengthValidationSpec), "maximumLength")]
[JsonDerivedType(typeof(ComparisonValidationSpec), "comparison")]
[JsonDerivedType(typeof(ProhibitedTransitionValidationSpec), "prohibitedTransition")]
[JsonDerivedType(typeof(ConditionalRequiredValidationSpec), "conditionalRequired")]
[JsonDerivedType(typeof(BatchUniqueValidationSpec), "batchUnique")]
[JsonDerivedType(typeof(CustomValidationSpec), "customRule")]
public abstract record ValidationSpec(string Id, ErrorSpec Error);

public sealed record RequiredValidationSpec(
    string Id,
    ErrorSpec Error,
    string Property) : ValidationSpec(Id, Error);

public sealed record MaximumLengthValidationSpec(
    string Id,
    ErrorSpec Error,
    string Property,
    int Value) : ValidationSpec(Id, Error);

public sealed record ComparisonValidationSpec(
    string Id,
    ErrorSpec Error,
    ValueSourceSpec Left,
    ComparisonOperator Operator,
    ValueSourceSpec Right) : ValidationSpec(Id, Error);

public enum ComparisonOperator
{
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual
}

public sealed record ProhibitedTransitionValidationSpec(
    string Id,
    ErrorSpec Error,
    string Property,
    JsonElement From,
    JsonElement To) : ValidationSpec(Id, Error);

public sealed record ConditionalRequiredValidationSpec(
    string Id,
    ErrorSpec Error,
    string Property,
    ConditionSpec When) : ValidationSpec(Id, Error);

public sealed record BatchUniqueValidationSpec(
    string Id,
    ErrorSpec Error,
    IReadOnlyList<string> Properties) : ValidationSpec(Id, Error);

public sealed record CustomValidationSpec(
    string Id,
    ErrorSpec Error,
    string Requirements,
    IReadOnlyList<string> Inputs,
    IReadOnlyList<CustomRuleOutput> Outputs,
    IReadOnlyList<string> ScenarioIds) : ValidationSpec(Id, Error);

public enum CustomRuleOutput
{
    ValidationFailure,
    Mutation,
    SideEffect
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SetDefaultRuleSpec), "setDefault")]
[JsonDerivedType(typeof(SetDerivedValueRuleSpec), "setDerivedValue")]
public abstract record BusinessRuleSpec(string Id);

public sealed record SetDefaultRuleSpec(
    string Id,
    string TargetProperty,
    ConditionSpec When,
    ValueSourceSpec Value) : BusinessRuleSpec(Id);

public sealed record SetDerivedValueRuleSpec(
    string Id,
    string TargetProperty,
    ConditionSpec When,
    ValueSourceSpec Value) : BusinessRuleSpec(Id);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ProduceEntitySideEffectSpec), "produceEntity")]
public abstract record SideEffectSpec(string Id);

public sealed record ProduceEntitySideEffectSpec(
    string Id,
    string OutputType,
    ConditionSpec When,
    TransactionMode TransactionMode,
    IdempotencySpec Idempotency,
    IReadOnlyList<PropertyMappingSpec> Mappings) : SideEffectSpec(Id);

public enum TransactionMode
{
    SameTransaction,
    Outbox
}

public sealed record IdempotencySpec(
    IdempotencyType Type,
    IReadOnlyList<ValueSourceSpec> Values);

public enum IdempotencyType
{
    Composite
}

public sealed record PropertyMappingSpec(
    string TargetProperty,
    ValueSourceSpec Value);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AllConditionSpec), "all")]
[JsonDerivedType(typeof(PropertyEqualsConditionSpec), "propertyEquals")]
[JsonDerivedType(typeof(PropertyTransitionConditionSpec), "propertyTransition")]
[JsonDerivedType(typeof(IsNewEntityConditionSpec), "isNewEntity")]
public abstract record ConditionSpec;

public sealed record AllConditionSpec(
    IReadOnlyList<ConditionSpec> Conditions) : ConditionSpec;

public sealed record PropertyEqualsConditionSpec(
    EntityScope Scope,
    string Property,
    JsonElement Value) : ConditionSpec;

public sealed record PropertyTransitionConditionSpec(
    string Property,
    JsonElement From,
    JsonElement To) : ConditionSpec;

public sealed record IsNewEntityConditionSpec : ConditionSpec { }

public enum EntityScope
{
    Incoming,
    Existing,
    Result
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "source")]
[JsonDerivedType(typeof(ConstantValueSourceSpec), "constant")]
[JsonDerivedType(typeof(IncomingPropertyValueSourceSpec), "incomingProperty")]
[JsonDerivedType(typeof(ExistingPropertyValueSourceSpec), "existingProperty")]
[JsonDerivedType(typeof(ResultPropertyValueSourceSpec), "resultProperty")]
[JsonDerivedType(typeof(ContextValueSourceSpec), "contextValue")]
public abstract record ValueSourceSpec;

public sealed record ConstantValueSourceSpec(JsonElement Value) : ValueSourceSpec;

public sealed record IncomingPropertyValueSourceSpec(string Property) : ValueSourceSpec;

public sealed record ExistingPropertyValueSourceSpec(string Property) : ValueSourceSpec;

public sealed record ResultPropertyValueSourceSpec(string Property) : ValueSourceSpec;

public sealed record ContextValueSourceSpec(string Name) : ValueSourceSpec;

public sealed record ErrorSpec(
    string Code,
    string Message,
    IReadOnlyList<string> Properties);

public sealed record ScenarioSpec(
    string Id,
    string Title,
    JsonElement? Given,
    JsonElement? Input,
    JsonElement? InputBatch,
    JsonElement Expect);
