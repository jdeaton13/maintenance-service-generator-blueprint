# Compiler Project Notes

The compiler should expose a pure pipeline:

```csharp
CompilationResult Compile(
    MaintenanceServiceSpec specification,
    FrameworkModel frameworkModel,
    CompilerOptions options);
```

Recommended stages:

1. Normalize specification values.
2. Run semantic validators.
3. Build an immutable generation model.
4. Emit deterministic source artifacts.
5. Emit generated test artifacts.
6. Return diagnostics, artifacts, and manifest data.

Do not place AI calls in this project.
