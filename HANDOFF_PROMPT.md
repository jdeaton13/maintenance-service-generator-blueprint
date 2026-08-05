# Handoff Prompt for an Implementation Agent

You are implementing the system described in this repository.

Start by reading, in order:

1. `AGENTS.md`
2. `README.md`
3. `docs/architecture.md`
4. `docs/domain-model.md`
5. `docs/generation-contract.md`
6. `docs/implementation-plan.md`
7. `docs/decisions/0001-custom-yaml-ir.md`
8. `docs/decisions/0002-formal-spec-over-direct-ai-codegen.md`

Then inspect:

- `spec/schema/maintenance-service.schema.json`
- `spec/examples/account-maintenance.request.md`
- `spec/examples/account-maintenance.yaml`
- `src/Contracts/MaintenanceServiceSpec.cs`
- `tests/Evals/account-maintenance.expected.json`

## Assignment

Implement Phase 1 and Phase 2 from `docs/implementation-plan.md`:

- Complete the immutable contracts.
- Ensure JSON serialization matches the version 1 schema.
- Add YAML serialization and deserialization.
- Add schema validation.
- Add semantic validation with stable `MNTxxx` diagnostics.
- Add round-trip and invalid-spec tests.
- Add CLI commands for `validate` and `explain`.

## Required behavior

- Treat the C# `MaintenanceServiceSpec` model as authoritative.
- Do not add arbitrary expression execution.
- Do not make model calls from build or compilation.
- Do not implement custom-rule AI generation yet.
- Keep all public behavior versioned and covered by tests.
- Use the account example as the first golden fixture.
- Do not replace the formal-spec pipeline with direct template-to-C# generation. References may guide AI interpretation, but the approved specification must remain the generation contract.

## Before changing architecture

If the supplied schema, contracts, and example disagree, normalize them around the domain intent documented in `AGENTS.md`. Record meaningful architecture changes as a new ADR rather than silently changing the design.

## Completion report

At completion, report:

1. Files added or changed.
2. Commands used to build and test.
3. Supported version 1 primitives.
4. Remaining gaps for Phase 3.
5. Any design decisions that require human review.
