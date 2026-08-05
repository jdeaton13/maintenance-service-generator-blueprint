# Architecture

## Overview

The system separates business intent, formal interpretation, and framework implementation.

```text
Business intent         Formal interpretation          Implementation
---------------         ---------------------          --------------
Markdown template  →    MaintenanceServiceSpec    →    Generated C#
                        JSON/YAML                        and tests
```

This separation makes the system reviewable, regenerable, and less dependent on a specific AI model.

## Components

### 1. Human requirements template

A structured Markdown document captures:

- Service metadata
- Entity fields and key
- Operation type
- Validation rules
- Business rules
- Mutations
- Side effects
- Failure behavior
- Positive and negative examples

The document is optimized for business and application developers, not for compiler implementation details.

### 2. Template interpreter

The interpreter uses an AI model with schema-constrained output to produce strict JSON matching `MaintenanceServiceSpec`.

The interpreter does not generate C#.

Ambiguity should be represented explicitly, for example:

```json
{
  "status": "needsClarification",
  "questions": [
    {
      "id": "Q1",
      "text": "Should a repeated activation request create another ledger entry?"
    }
  ]
}
```

No code should be generated while unresolved material ambiguity remains.

### 3. Approved YAML specification

After JSON deserialization and validation, the formal model is serialized to YAML for review and source control.

YAML is not the implementation language. It is a readable representation of the typed domain model.

### 4. Structural validator

Structural validation enforces:

- Required properties
- Allowed discriminators
- Primitive types
- Collection cardinality
- Version compatibility
- Basic shape of conditions and mappings

### 5. Semantic validator

Semantic validation enforces domain behavior that JSON Schema cannot reliably express:

- Property references resolve.
- Value types match property types.
- Transition rules are legal for the operation.
- Side-effect mappings are complete.
- IDs are unique.
- Atomicity and persistence modes are compatible.
- Custom rules include acceptance scenarios.
- Circular side-effect dependencies are prohibited or explicitly bounded.

When integrated into a consuming .NET solution, Roslyn symbols can additionally verify existing CLR types and generic interface conformance.

### 6. Deterministic generator

The generator consumes only the validated `MaintenanceServiceSpec` and framework metadata.

For a given:

- Specification hash
- Schema version
- Generator version
- Framework version

it should produce equivalent code.

Standard constructs are generated from trusted emitters. Examples include:

- Required validation
- String length validation
- Numeric or date range validation
- Field comparisons
- Existing-versus-incoming transitions
- Default and derived values
- Side-effect entity mappings
- Registration and endpoint glue

### 7. Custom-rule generator

A custom-rule generator is an escape hatch, not the default path.

It is invoked only for an explicit `customRule` node. Its output is bounded to a known interface and validated by compilation, analyzers, and generated acceptance tests.

Repeated custom patterns should be promoted into deterministic DSL primitives.

### 8. Verification harness

The verification harness:

1. Generates files into a temporary directory.
2. Creates or updates an isolated test project.
3. Runs formatting if required.
4. Compiles the output.
5. Runs framework analyzers.
6. Runs generated tests.
7. Produces a manifest and diagnostics report.

## Source-of-truth hierarchy

```text
*.request.md            Human intent and rationale
*.maintenance.yaml      Approved executable specification
Generated/*.g.cs        Disposable implementation
```

The YAML specification is the generation source of truth. The Markdown template remains valuable for explaining intent and regenerating a proposed interpretation.

## Versioning

Every specification includes `schemaVersion`.

Prefer version-specific parsers that normalize into one canonical model:

```text
v1 document ─┐
v2 document ─┼─→ canonical MaintenanceServiceSpec ─→ generator
v3 document ─┘
```

Breaking changes must either:

- Introduce a new schema version, or
- Include an automated migration.
