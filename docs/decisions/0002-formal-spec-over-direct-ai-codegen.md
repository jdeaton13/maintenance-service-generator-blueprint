# ADR 0002: Use a formal intermediate specification instead of direct AI-to-C# generation

- Status: Proposed
- Date: 2026-08-05
- Supersedes: None
- Related: [ADR 0001](0001-custom-yaml-ir.md)

## Context

The simplest possible implementation of this initiative would be:

```text
Completed business template
        +
Framework documentation
        +
Reference implementations
        ↓
Generative AI coding agent
        ↓
Generated C# service and tests
```

This approach is attractive because it has a low startup cost. It does not require a domain schema, semantic validator, or deterministic code generator. A capable coding agent can inspect existing services, follow examples, write files, compile the solution, and repair failures.

The alternative selected by this repository is:

```text
Completed business template
        ↓
AI requirements interpretation
        ↓
Versioned MaintenanceServiceSpec
        ↓
Structural and semantic validation
        ↓
Deterministic C# generation
        ↓
Compilation, analyzers, and generated tests
```

The decision is not based on an assumption that generative AI cannot produce working C#. It often can. The issue is whether direct generation provides enough control, consistency, auditability, and regeneration capability for a family of production services that share a common framework.

## Decision

Use a versioned, formal `MaintenanceServiceSpec` as the approved interpretation of the developer's business template.

- AI may translate natural-language requirements into the specification.
- Standard framework code is generated deterministically from the specification.
- AI-generated C# is allowed only for explicit, bounded `customRule` escape hatches.
- Generated output must pass compilation, framework analyzers, and acceptance tests.
- The approved specification, not an AI conversation or generated C# implementation, is the source of truth.

## Why template + references + AI is not sufficient by itself

Providing the model with templates, framework documentation, and high-quality reference services is still valuable. Those materials improve generation quality and should be used by the interpretation agent and custom-rule agent. However, they remain **guidance supplied through context**, not enforceable execution semantics.

A direct coding agent can still:

- Interpret equivalent requirements differently across runs.
- Choose a reference pattern that is valid but inconsistent with the pattern selected for another service.
- Omit a framework concern that is implicit in the examples, such as idempotency or atomic batch behavior.
- Produce code that compiles while implementing the wrong business condition.
- Make silent assumptions about old versus incoming state, null behavior, transaction boundaries, effect ordering, or retry behavior.
- Spread one business rule across multiple files, making the interpretation difficult to review.
- Modify framework plumbing when only entity-specific behavior should change.
- Generate tests that confirm its own mistaken interpretation rather than the intended business behavior.

Compilation and unit tests reduce these risks, but they do not establish that the agent interpreted the original requirements correctly. A formal intermediate specification creates a separate review and validation boundary between interpretation and implementation.

## Detailed comparison

| Concern | Direct AI-to-C# | Formal specification + generator |
|---|---|---|
| Initial implementation cost | Low | Higher |
| Time to first prototype | Fast | Slower |
| Human review target | Large code diff plus requirements | Concise behavioral specification plus generated-code summary |
| Equivalent input consistency | Prompt/model dependent | Deterministic after spec approval |
| Framework convention enforcement | Prompt instructions and references | Generator implementation and analyzers |
| Structural validation before code | Limited | JSON Schema and typed deserialization |
| Domain semantic validation | Usually mixed into agent reasoning | Explicit validator with stable diagnostics |
| Framework-wide migration | Agent edits every service | Update generator and regenerate |
| Model replacement | Can change code style and behavior | Interpreter may change while generator remains stable |
| Auditability | Prompt, context, code diff, and tests | Template, approved spec, manifest, code, and tests |
| Semantic diff | Difficult; inferred from code | Direct comparison of rules, mutations, and effects |
| Reproducibility | Best effort | Identical spec and generator version produce identical output |
| Handling irregular logic | Strong | Requires custom-rule escape hatch |
| Risk of overbuilding | Low initially | Real risk if the DSL grows too broadly |

## The most important advantages of the formal specification

### 1. Interpretation is reviewable before implementation

A natural-language statement such as:

> When an account becomes active, create an opening ledger entry.

contains unresolved choices:

- Does "becomes" mean an existing entity transition or any input whose status is active?
- Which previous statuses qualify?
- Does it apply on insert?
- What date source is used?
- Is the effect inserted in the same transaction?
- How is duplicate production prevented on retry?

A formal specification must make those decisions explicit:

```yaml
sideEffects:
  - id: create-opening-ledger-entry
    type: produceEntity
    outputType: AccountLedgerEntry
    when:
      type: propertyTransition
      property: Status
      from: Inactive
      to: Active
      existingEntityRequired: true
    transactionMode: sameTransaction
    idempotency:
      strategy: requestEntityAndEffect
```

The reviewer can validate the meaning without reverse-engineering generated code.

### 2. Standard behavior becomes deterministic

Once the specification says:

```yaml
- id: V1
  type: required
  property: AccountNumber
```

there should be one controlled implementation for that primitive. The model does not choose among multiple possible validator styles, error-code patterns, or null-handling strategies.

This reduces code variation and makes generated services easier to support operationally.

### 3. Framework upgrades are centralized

When the maintenance framework changes an API, direct AI generation requires an agent-driven migration across every existing service. Each edit becomes a new interpretation and implementation event.

With a formal specification:

```text
Approved specifications
        +
New framework-aware generator version
        ↓
Regenerated services
```

The migration is implemented once, evaluated against the full specification suite, and applied consistently.

### 4. Domain constraints can fail early

The semantic validator can reject impossible or incomplete definitions before C# generation:

```text
MNT005 Transition rules require an upsert operation.
MNT007 Every side effect must declare an idempotency strategy.
MNT011 Atomic batch services cannot declare partial side-effect persistence.
```

A direct coding agent may catch these issues, but only through prompt reasoning, compilation feedback, tests, or human review. Stable domain diagnostics make the constraints executable and reusable.

### 5. The system is less dependent on model behavior

Models, prompts, retrieval results, and context ordering will change. With direct code generation, those changes can alter implementation details across regenerated services.

With a formal specification, the model is responsible for a narrower task: mapping business requirements into a known vocabulary. Once approved, the same specification can be regenerated without invoking AI.

### 6. Behavioral changes can be diffed semantically

A raw code diff may contain formatting, generated API changes, or refactoring noise. A semantic specification diff can report:

```text
Changed validation V4:
  Previous: Closed → Active is prohibited
  New: Closed → Active and Closed → Inactive are prohibited

Changed side effect SE1:
  Previous: Runs on any Active input
  New: Runs only on Inactive → Active transition
```

This is a better approval surface for business behavior.

### 7. Evaluation is more precise

For an interpretation agent, the expected output is a bounded specification. Evaluation can compare:

- Rule kinds and stable IDs.
- Property references.
- Conditions and value sources.
- Transaction and idempotency choices.
- Declared ambiguities.

Evaluating a large C# implementation is harder because many implementations may be syntactically different while behaviorally equivalent, and some may compile while hiding semantic errors.

## Risks introduced by the formal-spec approach

This decision has meaningful costs and should not be presented as universally superior.

### Higher platform investment

The team must own:

- The `MaintenanceServiceSpec` contracts.
- A versioned serialization schema.
- Structural and semantic validation.
- Deterministic emitters.
- Generator compatibility with framework versions.
- A primitive catalog and governance process.
- Migration tooling for specification versions.

### DSL scope pressure

Teams may request increasingly complex constructs until YAML becomes an inferior programming language. The repository must resist arbitrary expressions, embedded SQL, loops, and general control flow.

### Reduced flexibility for unusual services

Direct AI generation is naturally flexible. A constrained specification cannot represent every irregular business rule without extension. The selected architecture therefore includes a bounded `customRule` escape hatch.

### False confidence

A valid specification and deterministic generator do not prove that business requirements are correct. Explicit examples and acceptance scenarios remain required, and human review is still necessary for material behavior.

## When direct AI-to-C# is the better choice

Use direct AI generation, supported by references and compile/test feedback, when most of the following are true:

- This is an exploratory proof of concept.
- Only a small number of services will be generated.
- Services are highly irregular and share little semantic vocabulary.
- The framework changes infrequently.
- Generated services will be treated as normal hand-owned C# afterward.
- The organization accepts code-level review as the primary governance boundary.
- Fast delivery is more valuable than deterministic regeneration.
- There is not yet enough domain knowledge to define stable primitives.

A direct generation prototype can also be used deliberately to discover repeated patterns before committing them to the formal specification.

## When the formal intermediate specification is the better choice

Use the architecture selected by this repository when most of the following are true:

- Many services share the same maintenance framework.
- Standard validations, transitions, mutations, and side effects repeat.
- Services must be regenerated as the framework evolves.
- Consistency and operational supportability matter.
- Business behavior needs a concise approval and audit surface.
- Idempotency, atomicity, and side-effect semantics must be explicit.
- Multiple agents or model versions may participate over time.
- The organization wants to reduce long-term code-review burden.

## Hybrid strategy

The selected design intentionally keeps generative AI in the workflow:

```text
Human business template
        ↓
AI interpretation using references and framework documentation
        ↓
Validated MaintenanceServiceSpec
        ↓
Deterministic generation for supported primitives
        ↓
Bounded AI generation for explicit customRule nodes only
        ↓
Compile, analyze, and test
```

References are therefore not discarded. They are used where they provide the most value:

- Teaching the interpretation agent the domain vocabulary.
- Supplying examples for ambiguous requirements.
- Guiding custom-rule generation.
- Creating evaluation fixtures.
- Helping propose new deterministic primitives from recurring patterns.

The difference is that references do not replace the formal contract.

## Adoption strategy

Do not begin by modeling the entire framework. Build a proof of concept from three to ten representative services.

1. Give a coding agent the templates and references and have it implement the services directly.
2. Capture where implementations vary or assumptions are made.
3. Extract only repeated, stable semantics into `MaintenanceServiceSpec` primitives.
4. Implement deterministic generation for those primitives.
5. Retain `customRule` for the uncommon remainder.
6. Measure whether deterministic coverage and review savings justify continued investment.

Recommended proof-of-concept metrics:

- Percentage of production code generated deterministically.
- Percentage of rules requiring `customRule`.
- Interpretation defects found during spec review.
- Generated-code defects found after spec approval.
- Regeneration success across framework upgrades.
- Review time compared with direct AI-generated code.
- Behavioral consistency across equivalent requirements.

## Consequences

Positive:

- AI performs a narrower, more testable interpretation task.
- Standard implementation is deterministic and framework-controlled.
- Requirements, interpretation, and implementation are separate artifacts.
- Framework migrations and model changes are less disruptive.
- Semantic validation and diffing become possible.

Negative:

- Delivery requires more up-front platform work.
- The specification and generator become maintained products.
- The team must govern primitive growth.
- Some unusual rules still require generated or hand-written C#.

## Decision checkpoint

After the proof of concept, revisit this ADR. Continue investing in the specification and generator only if the representative services demonstrate meaningful semantic repetition and deterministic generation materially reduces review, maintenance, or migration cost.
