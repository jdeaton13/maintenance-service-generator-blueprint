# Domain Model

## Design principles

1. The C# model is authoritative.
2. JSON and YAML are serializations.
3. Use discriminated unions for rule, condition, and value-source variants.
4. Avoid untyped dictionaries except for forward-compatible metadata.
5. Every behavioral node has a stable ID.
6. Execution semantics must be defined independently of generated code shape.

## Root model

```csharp
public sealed record MaintenanceServiceSpec(
    int SchemaVersion,
    ServiceSpec Service,
    EntitySpec Entity,
    IReadOnlyList<ValidationSpec> Validations,
    IReadOnlyList<BusinessRuleSpec> BusinessRules,
    IReadOnlyList<SideEffectSpec> SideEffects,
    IReadOnlyList<ScenarioSpec> Scenarios);
```

## Initial primitive catalog

### Validations

- `required`
- `maximumLength`
- `minimumLength`
- `range`
- `comparison`
- `unique`
- `batchUnique`
- `referenceExists`
- `prohibitedTransition`
- `conditionalRequired`
- `customRule`

### Business rules and mutations

- `setDefault`
- `setDerivedValue`
- `normalizeValue`
- `prohibitOperation`
- `customRule`

### Conditions

- `all`
- `any`
- `not`
- `propertyEquals`
- `propertyChanged`
- `propertyTransition`
- `entityExists`
- `isNewEntity`
- `batchContainsDuplicate`

### Value sources

- `constant`
- `incomingProperty`
- `existingProperty`
- `contextValue`
- `ruleOutput`

### Side effects

- `produceEntity`
- `produceEntities`
- `enqueueOutboxMessage`

## Execution scopes

Every property or value reference must declare its scope:

- `incoming`: the submitted entity.
- `existing`: the persisted entity before the operation.
- `result`: the entity after generated mutations.
- `batch`: the submitted collection.
- `context`: trusted runtime values such as business date, tenant, user, or request ID.

Avoid ambiguous references such as `entity.Status` when it is unclear whether this means incoming or post-mutation state.

## Custom rules

A custom rule must include:

- Stable ID
- Human-readable requirements
- Allowed inputs
- Expected outputs
- Allowed dependencies
- Failure contract
- At least one positive scenario
- At least one negative or boundary scenario

Custom rules must not directly persist data. They return validation failures, mutations, or side-effect intents through framework-owned contexts.
