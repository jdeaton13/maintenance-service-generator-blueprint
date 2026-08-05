# Generator Project Notes

Prefer typed emitters or Roslyn `SyntaxFactory` for small, safety-critical constructs. Trusted internal templates are acceptable for repetitive files when all template inputs come from the validated generation model.

The generator should not parse YAML directly. It receives a validated `MaintenanceServiceSpec` or normalized generation model.
