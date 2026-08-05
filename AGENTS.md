# Agent Implementation Instructions

## Mission

Build an AI-assisted, deterministic code-generation system for the organization's maintenance-service framework.

A developer supplies a completed Markdown requirements template. An AI interpretation step converts it into a strict `MaintenanceServiceSpec`. The approved specification is stored as YAML. A deterministic .NET generator produces the service implementation, registrations, framework wiring, documentation metadata, and tests.

Do not treat this as a generic natural-language-to-C# system. The intermediate specification is the source of truth for generation.

Before proposing a simpler direct AI-to-C# implementation, read `docs/decisions/0002-formal-spec-over-direct-ai-codegen.md`. Direct generation is useful for prototyping and bounded custom rules, but it is not a substitute for the approved specification and deterministic generator unless a new ADR changes this decision.

## Required architecture

```text
*.request.md
    ↓ AI interpretation using schema-constrained JSON
MaintenanceServiceSpec JSON
    ↓ deserialize and semantic validation
*.maintenance.yaml
    ↓ deterministic compiler/generator
Generated C# + tests
    ↓
dotnet build + analyzers + dotnet test
```

## Design constraints

1. Define the domain model in C# first. YAML and JSON are only serializations of that model.
2. Use JSON Schema for structural validation of AI output and YAML documents.
3. Implement domain semantic validation in .NET.
4. Generate standard framework behavior deterministically.
5. Use AI-generated C# only for explicitly unsupported custom rules.
6. Never perform model calls from a Roslyn source generator or ordinary build.
7. Generated code must be replaceable and carry an auto-generated header.
8. Hand-written extension code must live outside generated files.
9. Every rule, side effect, and scenario must have a stable identifier.
10. Side effects must have deterministic idempotency semantics.
11. Do not allow arbitrary expressions, SQL, C#, or scripting in YAML.
12. Add new DSL primitives only when their execution semantics are precise and testable.
13. Use templates, framework documentation, and reference services as AI context, but never treat prompt context as an enforceable domain contract.
14. Keep business interpretation reviewable separately from generated implementation.

## Initial deliverables

Create these projects:

```text
Maintenance.Generator.Contracts
Maintenance.Generator.TemplateInterpreter
Maintenance.Generator.Compiler
Maintenance.Generator.CodeGeneration
Maintenance.Generator.Analysis
Maintenance.Generator.Testing
Maintenance.Generator.Cli
```

### Contracts

Implement immutable records for:

- `MaintenanceServiceSpec`
- `ServiceSpec`
- `EntitySpec`
- `PropertySpec`
- `ValidationSpec` discriminated variants
- `BusinessRuleSpec` discriminated variants
- `MutationSpec`
- `SideEffectSpec`
- `ConditionSpec` discriminated variants
- `ValueSourceSpec` discriminated variants
- `ScenarioSpec`
- `ErrorSpec`
- `IdempotencySpec`

### Validation

Implement three gates:

1. **Structural validation** using the versioned JSON Schema.
2. **Semantic validation** against the specification itself and, when available, the consuming C# compilation.
3. **Generated-output validation** using compilation, analyzers, and tests.

Initial semantic diagnostics should include:

```text
MNT001 Entity must declare exactly one key for schema version 1.
MNT002 All rule and side-effect identifiers must be unique.
MNT003 Referenced properties must exist.
MNT004 Enum constants must be valid for the referenced property.
MNT005 Transition rules require an upsert operation.
MNT006 Produced entity mappings must populate all required target properties.
MNT007 Every side effect must declare an idempotency strategy.
MNT008 Scenario references must use declared properties and rules.
MNT009 Arbitrary expression fields are prohibited.
MNT010 Custom rules must declare inputs, outputs, and acceptance scenarios.
```

### Deterministic generation

Generate framework composition and small typed handlers rather than duplicating orchestration logic.

The runtime framework, not generated code, should own:

- Request orchestration
- Transactions
- Bulk persistence
- Idempotency
- Error response formatting
- Logging and tracing
- Side-effect execution
- Outbox behavior for external effects

Generated output should include, as needed:

- Entity or input contract
- Maintenance definition/descriptor
- Validators
- Business-rule handlers
- Side-effect projectors
- Endpoint/controller glue
- Dependency-injection registration
- Serialization metadata
- OpenAPI metadata
- Unit tests
- Integration-test fixtures

### AI interpretation

The interpretation model must return strict JSON matching the schema. It must not return YAML or C# directly.

The interpreter must:

- Preserve stable rule IDs from the template.
- Mark unresolved ambiguity instead of guessing.
- Distinguish incoming entity, existing entity, batch, and execution context.
- Distinguish validation failures, mutations, and produced side effects.
- Require examples for custom rules.
- Reject unsupported behavior with an explicit `unsupported` result.

### AI custom-rule fallback

Only invoke custom-rule generation when a specification node uses `type: customRule` and deterministic primitives are insufficient.

The model receives:

- The bounded rule specification.
- Relevant framework interfaces.
- Relevant entity and context types.
- Allowed dependencies.
- Analyzer rules.
- Acceptance scenarios.

The model may generate only the declared handler and tests. It must not modify framework infrastructure, project files, or unrelated source.

### CLI

Implement these commands:

```text
init        Scaffold a human requirements template.
interpret   Convert Markdown requirements to strict JSON, validate, then emit YAML.
validate    Run structural and semantic validation.
explain     Print a human-readable summary of behavior.
diff        Produce a semantic diff between two specifications.
generate    Generate C# and tests deterministically.
verify      Generate into a temporary directory, compile, analyze, and test.
```

## Implementation sequence

Follow this order:

1. Contracts and serialization.
2. JSON Schema generation or synchronization.
3. Semantic validator.
4. Deterministic generator for one simple service.
5. CLI `validate`, `explain`, and `generate`.
6. Test generation from scenarios.
7. Build/analyzer/test verification harness.
8. AI template interpreter.
9. Semantic diff.
10. Bounded custom-rule fallback.

Do not begin with autonomous repair loops. First make deterministic generation and validation reliable.

## Proof-of-concept scenarios

The first implementation must support at least:

1. Simple bulk insert with required and length validations.
2. Bulk upsert with an existing-versus-incoming prohibited transition.
3. Conditional side effect that produces another entity in the same transaction.
4. Batch uniqueness validation.
5. Atomic batch rejection.

Use the account example in `spec/examples` as the first golden test.

## Definition of done

A service is considered generated successfully only when:

- Its specification passes structural and semantic validation.
- Generation is deterministic for identical input and generator version.
- Generated code has no manual edits.
- The generated project compiles.
- Framework analyzers report no errors.
- Generated acceptance tests pass.
- The generation manifest records input hash, schema version, framework version, generator version, and prompt version.
