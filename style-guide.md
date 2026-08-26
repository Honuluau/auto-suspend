# Auto-Suspend Style Guide
This style guide is almost entirely inherited from [Google's C# Style Guide](https://google.github.io/styleguide/csharp-style.html) that was viewed on August 26th, 2026. This is made for any developer who may view or work on Auto-Suspend in the future.

### Remarks

Seriously, this style guide is ~99% derived from Google's open-source guide and is no way an original work. I am aware that style guides must be met in the industry but my department does not have any so I have to form my own. With no prior experience in style guides I have to resort to imitation.

I'm not really a fan of how Google handles indents so that is the main change. This is just a modified clone of Google's style guide so that if the original website were updated, the code would still follow the intended format.

## Formatting Guidelines

### Naming Rules

#### Code
*   Names of classes, methods, enumerations, public fields, public properties,
    namespaces: `PascalCase`.
*   Names of local variables, parameters: `camelCase`.
*   Names of private, protected, internal and protected internal fields and
    properties: `_camelCase`.
*   Naming convention is unaffected by modifiers such as const, static,
    readonly, etc.
*   For casing, a "word" is anything written without internal spaces, including
    acronyms. For example, `MyRpc` instead of ~~`MyRPC`~~.
*   Names of interfaces start with `I`, e.g. `IInterface`.

#### Files

*   Filenames and directory names are `PascalCase`, e.g. `MyFile.cs`.
*   Where possible the file name should be the same as the name of the main
    class in the file, e.g. `MyClass.cs`.
*   In general, prefer one core class per file.

### Organization

*   Modifiers occur in the following order: `public protected internal private
    new abstract virtual override sealed static readonly extern unsafe volatile
    async`.
*   Namespace `using` declarations go at the top, before any namespaces. `using`
    import order is alphabetical, apart from `System` imports which always go
    first.
*   Class member ordering:
    *   Group class members in the following order:
        *   Nested classes, enums, delegates and events.
        *   Static, const and readonly fields.
        *   Fields and properties.
        *   Constructors and finalizers.
        *   Methods.
    *   Within each group, elements should be in the following order:
        *   Public.
        *   Internal.
        *   Protected internal.
        *   Protected.
        *   Private.
    *   Where possible, group interface implementations together.

### Whitespace rules

Developed from Google Java style.

*   A maximum of one statement per line.
*   A maximum of one assignment per statement.
*   Indentation via tabs.
*   Column limit: 110.
*   No line break before opening brace.
*   No line break between closing brace and `else`.
*   Braces used even when optional.
*   Space after `if`/`for`/`while` etc., and after commas. Example: `if (logic) {`
*   No space after an opening parenthesis or before a closing parenthesis.
*   No space between a unary operator and its operand. One space between the
    operator and each operand of all other operators.
*   Line wrapping developed from Google C++ style guidelines, with minor
    modifications for compatibility with Microsoft's C# formatting tools:
    *   In general, line continuations are 1 tab.
    *   Line breaks with braces (e.g. list initializers, lambdas, object
        initializers, etc) do not count as continuations.
    *   For function definitions and calls, if the arguments do not all fit on
        one line they should be broken up onto multiple lines, with each
        subsequent line aligned with the first argument. If there is not enough
        room for this, arguments may instead be placed on subsequent lines with
        a four space indent. The code example below illustrates this.