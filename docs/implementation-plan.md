# Implementation Plan

## Phase 0: Framework inventory

Document the existing maintenance framework:

- Base interfaces
- Header model
- Bulk insert/upsert entry points
- Validation pipeline
- Business-rule pipeline
- Side-effect pipeline
- Persistence abstractions
- Transaction boundaries
- Idempotency behavior
- Error model
- Dependency-injection conventions
- Existing representative services

Select three golden services covering simple, moderate, and complex behavior.

## Phase 1: Contracts and schema

Deliver:

- Immutable `MaintenanceServiceSpec` contracts
- JSON and YAML serialization
- Versioned JSON Schema
- Golden serialization tests
- Validation result and diagnostic types

Exit criteria:

- The account example round-trips JSON → C# → YAML → C# without semantic loss.

## Phase 2: Semantic validation

Deliver:

- ID uniqueness checks
- Property and type reference checks
- Operation/rule compatibility checks
- Side-effect mapping checks
- Idempotency checks
- Scenario validation
- Human-readable `explain` output

Exit criteria:

- Invalid examples fail with stable `MNTxxx` diagnostics and source paths.

## Phase 3: Deterministic generation

Support:

- Required and length validations
- Comparison and transition validations
- Default and derived mutations
- One produced-entity side effect
- Atomic batch behavior
- DI registration and endpoint glue

Exit criteria:

- One golden service is regenerated and compiles without manual changes.

## Phase 4: Test generation and verification

Deliver:

- Scenario-to-test emitter
- Isolated compile harness
- Analyzer execution
- Test execution
- Generation manifest

Exit criteria:

- All golden services compile and generated tests pass.

## Phase 5: AI interpretation

Deliver:

- Markdown template parser
- Schema-constrained JSON model call
- Ambiguity result model
- Prompt versioning
- Interpretation evaluation suite
- YAML review output

Exit criteria:

- The interpreter produces the expected specification for the golden templates with no silent behavioral invention.

## Phase 6: Semantic diff and developer workflow

Deliver:

- Semantic spec diff
- CLI workflow
- Pull-request integration
- Generated-code ownership checks
- Documentation

Exit criteria:

- A developer can update a template, approve the semantic diff, regenerate, verify, and open a pull request.

## Phase 7: Custom-rule fallback

Deliver only after deterministic paths are stable:

- Explicit `customRule` specification
- Bounded AI code generation
- Restricted file output
- Compile/analyze/test repair loop with a fixed attempt limit
- Promotion process for repeated custom patterns

Exit criteria:

- A custom rule can be generated without granting the agent broad repository modification access.
