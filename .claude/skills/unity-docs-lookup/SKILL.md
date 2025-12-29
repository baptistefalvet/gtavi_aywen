---
name: unity-docs-lookup
description: Look up Unity API documentation from locally installed Unity docs AND package documentation (Cinemachine, Input System, etc.). Use when user asks to verify documentation or when uncertain about Unity API signatures, parameters, or Unity 6-specific features. Provides efficient access to ScriptReference, Manual, and package markdown docs without inflating context window.
---

# Unity Documentation Lookup

Efficiently search local Unity documentation to verify API signatures and parameters.
**Supports both core Unity APIs and package documentation** (Cinemachine, Input System, etc.).

## Usage

```bash
# Look up a core Unity class
python3 scripts/search_unity_docs.py Transform

# Look up a specific method
python3 scripts/search_unity_docs.py Rigidbody AddForce

# Look up package documentation (Cinemachine, Input System, etc.)
python3 scripts/search_unity_docs.py CinemachineCamera

# Search for APIs by name (searches both core and packages)
python3 scripts/search_unity_docs.py --search Input
```

## When to Use

- Uncertain about API parameters or signatures
- Need Unity 6-specific features
- Complex APIs with multiple overloads
- User asks to verify documentation
- **Working with package APIs** (Cinemachine, Input System, AI Navigation, etc.)

Do NOT use for common APIs you already know.

## Features

### Core Unity API
- Auto-detects latest Unity installation
- Extracts descriptions, signatures, parameters

### Package Documentation (NEW!)
- Searches `Library/PackageCache/*/Documentation~/*.md`
- Parses markdown tables for properties
- Shows: Properties, Targets, Usage, Examples

## Notes

- Automatically tries core Unity API first, then packages
- Package docs only available from Unity project directory
- Use `--search` when unsure of exact name
