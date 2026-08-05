# CLI Project Notes

Initial commands:

```text
maintenance init <name>
maintenance interpret <request.md>
maintenance validate <spec.yaml>
maintenance explain <spec.yaml>
maintenance diff <old.yaml> <new.yaml>
maintenance generate <spec.yaml> --output <directory>
maintenance verify <spec.yaml>
```

`verify` should generate into an isolated directory and must not overwrite repository files unless explicitly requested.
