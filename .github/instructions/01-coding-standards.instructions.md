---
applyTo: "*.cs"
---
# Coding Standards

## C# Code Style

This project follows a specific C# coding style defined in [.editorconfig](src/.editorconfig):

### Key Conventions

- **File-scoped namespaces**: Use file-scoped namespace declarations (`namespace X;` instead of `namespace X { ... }`)
- **Var usage**: Always use `var` when the type is apparent
- **Braces style**: No new line before opening braces
- **Static local functions**: Prefer static local functions when possible
- **Readonly fields**: Always use readonly modifier for fields that don't change after initialization
- **Expression-bodied members**: Properties and accessors should use expression bodies where appropriate
- **Primary constructors**: Always use when constructor is used only to save parameters to instance fields

### Naming Conventions

- **PascalCase**: Used for types, namespaces, methods, properties
- **camelCase**: Used for parameters and local variables
- **m_PascalCase**: Used for private instance fields
- **s_PascalCase**: Used for private static fields
- **IPascalCase**: Interface names start with 'I'
- **TPascalCase**: Type parameter names start with 'T'

### Code Organization

- System directives first, then all other directives
- File-scoped namespaces
- One type per file, with filename matching type name

## Project Structure

- Test projects are named with `.Tests` suffix
- Abstract interfaces are in `.Abstractions` projects

