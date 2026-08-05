# ADR 0001: Use a custom domain specification serialized as YAML

- Status: Proposed
- Date: 2026-08-05

## Context

The organization has a generic framework for maintenance services. Each service bulk inserts or upserts a specific entity, applies entity-specific validations and business rules, and may produce additional entity types as side effects.

The goal is to reduce hand coding by allowing developers to complete a business-oriented template and have the service generated.

Alternatives considered:

1. AI directly generates C# from the template.
2. Developers author C# using attributes and fluent builders.
3. Developers author a custom YAML DSL.
4. AI interprets the template into custom YAML, then a deterministic compiler generates C#.
5. Use a general specification platform such as TypeSpec.

## Decision

Use a versioned, strongly typed `MaintenanceServiceSpec` as the formal intermediate representation.

- AI returns schema-constrained JSON.
- The approved interpretation is serialized to YAML for review and source control.
- A deterministic .NET generator produces standard C# implementation and tests.
- AI-generated C# is permitted only for explicit custom-rule escape hatches.

## Rationale

For the detailed comparison against direct generative AI using a template and reference implementations, see [ADR 0002](0002-formal-spec-over-direct-ai-codegen.md).


This approach provides:

- A reviewable interpretation between natural language and code.
- Deterministic regeneration.
- Model independence.
- Central framework migrations.
- Strong structural and semantic validation.
- A domain vocabulary tailored to maintenance services.
- Lower tooling overhead than introducing another compiler ecosystem initially.

## Consequences

Positive:

- Developers do not need to hand-code standard services.
- Generated code can be replaced at any time.
- Framework changes can regenerate all services.
- Common custom patterns can become new deterministic primitives.

Negative:

- The organization must maintain a domain model, schema, validator, and generator.
- The primitive catalog must be governed carefully.
- There is a risk of creating a poor programming language inside YAML.

## Guardrails

- Do not permit arbitrary expressions, SQL, C#, loops, or scripts in YAML.
- Keep YAML as serialized domain data.
- Add a primitive only when its semantics are precise and reusable.
- Require custom rules to declare inputs, outputs, and scenarios.
- Keep model calls outside ordinary builds.

## Reconsider TypeSpec when

- Multiple platforms or languages consume the same specifications.
- Developers begin authoring the formal specification directly.
- API, client, event, and deployment generation converge around a shared model.
- The custom specification requires substantial language-composition features.
