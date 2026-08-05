# Maintenance Service Generator Blueprint

This repository is a design and implementation handoff for an AI-assisted generator that creates maintenance services from a business-focused template.

The target workflow is:

```text
Developer fills out a Markdown template
                ↓
AI interprets the template into a strict JSON object
                ↓
Deserialize into MaintenanceServiceSpec
                ↓
Serialize approved interpretation as YAML
                ↓
Structural + semantic validation
                ↓
Deterministic C# generation
                ↓
Roslyn analysis + build + generated tests
                ↓
Reviewable pull request
```

## Core recommendation

Use a custom, domain-specific YAML representation as the approved intermediate specification, but do not build a general-purpose YAML programming language.

The real domain contract is a versioned, strongly typed `MaintenanceServiceSpec` object model. JSON is used for schema-constrained AI output. YAML is used for human review and source control. Generated C# is disposable and should not be edited directly.

## Why not generate C# directly from the template?

Direct AI-to-C# generation is the fastest way to prototype this idea and remains a valid fallback for irregular rules. It is not the primary architecture because framework references and examples guide a model but do not enforce semantics. A formal intermediate specification makes assumptions about transitions, atomicity, idempotency, mutations, and side effects explicit before implementation; allows structural and domain validation; supports semantic diffs; and lets framework changes be applied by updating one deterministic generator.

The full tradeoff analysis, including cases where direct AI generation is preferable, is recorded in [`ADR 0002`](docs/decisions/0002-formal-spec-over-direct-ai-codegen.md).

## Goals

- Developers describe the entity, validations, business rules, side effects, and examples once.
- Standard behavior is generated deterministically.
- AI is used for requirements interpretation and narrowly bounded exceptional logic.
- Every generated service is structurally validated, semantically validated, compiled, analyzed, and tested.
- Framework upgrades can regenerate all services from their approved specifications.

## Non-goals

- Running an AI model during every `dotnet build`.
- Allowing arbitrary C#, SQL, or expression strings inside YAML.
- Replacing the maintenance framework with generated bespoke implementations.
- Letting developers hand-edit generated files.
- Attempting to represent every possible business rule in the first schema version.

## Repository map

- [`AGENTS.md`](AGENTS.md): primary instructions for an implementation agent.
- [`docs/architecture.md`](docs/architecture.md): proposed architecture and processing pipeline.
- [`docs/domain-model.md`](docs/domain-model.md): recommended intermediate representation.
- [`docs/generation-contract.md`](docs/generation-contract.md): deterministic versus generative responsibilities.
- [`docs/implementation-plan.md`](docs/implementation-plan.md): phased delivery plan.
- [`docs/decisions/0001-custom-yaml-ir.md`](docs/decisions/0001-custom-yaml-ir.md): architecture decision record.
- [`docs/decisions/0002-formal-spec-over-direct-ai-codegen.md`](docs/decisions/0002-formal-spec-over-direct-ai-codegen.md): detailed comparison with direct template-and-reference AI code generation.
- [`spec/schema/maintenance-service.schema.json`](spec/schema/maintenance-service.schema.json): initial JSON Schema.
- [`spec/examples/account-maintenance.request.md`](spec/examples/account-maintenance.request.md): example human template.
- [`spec/examples/account-maintenance.yaml`](spec/examples/account-maintenance.yaml): example approved interpretation.
- [`prompts/interpret-template.md`](prompts/interpret-template.md): prompt contract for the interpretation step.
- [`prompts/generate-custom-rule.md`](prompts/generate-custom-rule.md): bounded fallback prompt for custom logic.
- [`src/Contracts/MaintenanceServiceSpec.cs`](src/Contracts/MaintenanceServiceSpec.cs): starter C# contract model.
- [`tests/Evals/account-maintenance.expected.json`](tests/Evals/account-maintenance.expected.json): initial evaluation fixture.

## Suggested first milestone

Implement a CLI that supports:

```bash
dotnet maintenance init AccountMaintenance
dotnet maintenance interpret AccountMaintenance.request.md
dotnet maintenance validate AccountMaintenance.yaml
dotnet maintenance generate AccountMaintenance.yaml
dotnet maintenance verify AccountMaintenance.yaml
```

For the first proof of concept, support only:

- Bulk insert and bulk upsert.
- Required, maximum length, range, uniqueness, field comparison, and prohibited transition validations.
- Default and derived value mutations.
- Conditional production of one or more related entities.
- Atomic batch failure.
- Unit-test generation from explicit examples.

## Success criteria

The proof of concept is successful when three representative services can be regenerated from their approved specifications and all of the following pass:

1. JSON Schema validation.
2. Domain semantic validation.
3. Generated code compilation.
4. Framework-specific Roslyn analyzers.
5. Generated unit and integration tests.
6. Regeneration without manual edits to generated files.
