# Copilot Instructions

Follow `AGENTS.md` as the primary implementation contract.

Key rules:

- The typed `MaintenanceServiceSpec` is authoritative.
- YAML and JSON are serialization formats, not the compiler domain model.
- Standard behavior must be generated deterministically.
- Do not introduce arbitrary expression evaluation into YAML.
- Do not call AI models during ordinary builds.
- Generated files are disposable and may not contain manual edits.
- Add tests for every new schema primitive and semantic diagnostic.
