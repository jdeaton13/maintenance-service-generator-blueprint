# Prompt Contract: Interpret Maintenance Template

## System intent

Convert a completed maintenance-service requirements document into a strict JSON object matching the supplied `MaintenanceServiceSpec` JSON Schema.

## Rules

1. Return JSON only through schema-constrained output.
2. Do not generate C#, YAML, SQL, or pseudocode.
3. Do not invent business behavior that is not stated or logically required by the framework contract.
4. Preserve supplied validation, business-rule, side-effect, and scenario IDs.
5. Use deterministic specification primitives whenever possible.
6. Use `customRule` only when no supported primitive can represent the behavior.
7. Distinguish incoming, existing, result, batch, and context values explicitly.
8. Side effects must include transaction and idempotency behavior.
9. Material ambiguity must produce a `needsClarification` response rather than a guessed specification.
10. A custom rule requires explicit inputs, outputs, and scenario references.

## Interpretation guidance

- “When X changes from A to B” means a property transition requiring an existing entity.
- “Defaults to” means a mutation, not a validation.
- “Create another object” means a side effect, not direct persistence inside a rule.
- “Reject the entire batch” means `failureMode: atomicBatch`.
- “Current business date” means a context value named `BusinessDate`, not system UTC time.
- “Retry must not create another object” requires a deterministic idempotency key.

## Output modes

The API contract should expose exactly one of:

```json
{
  "status": "complete",
  "specification": {}
}
```

or:

```json
{
  "status": "needsClarification",
  "questions": [
    {
      "id": "Q1",
      "text": "...",
      "reason": "..."
    }
  ]
}
```

## Quality checks before returning

- Every referenced property is declared.
- Every rule has a stable ID.
- Every side effect has idempotency semantics.
- Every custom rule references acceptance scenarios.
- No arbitrary expression strings are present.
- No framework implementation detail is invented unless supplied as explicit context.
