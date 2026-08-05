# Prompt Contract: Generate a Bounded Custom Rule

## Purpose

Generate one custom maintenance-rule handler and its tests when the approved specification contains a `customRule` node that cannot be emitted deterministically.

## Inputs provided to the model

- The exact custom-rule specification.
- Entity type definitions.
- Allowed framework interfaces and examples.
- Allowed injected dependencies.
- Analyzer requirements.
- Acceptance scenarios.
- Target namespace and file names.

## Restrictions

1. Generate only the requested handler and tests.
2. Do not modify project files, framework infrastructure, unrelated services, or generated deterministic files.
3. Do not call persistence APIs directly.
4. Return failures, mutations, and side-effect intents through the supplied framework context.
5. Preserve the specification rule ID in diagnostics and tests.
6. Use cancellation tokens for asynchronous work.
7. Avoid reflection and dynamic code.
8. Do not introduce new dependencies without explicit approval.
9. Do not weaken tests to make the implementation pass.
10. Stop and report an unsupported requirement when the allowed interfaces cannot implement it.

## Verification

The generated output is accepted only after:

- Compilation succeeds.
- Framework analyzers succeed.
- All referenced acceptance scenarios pass.
- No unauthorized files changed.
