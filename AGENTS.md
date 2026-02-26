# General

- Prefer simple solutions
- Ask if unsure
- Keep answers concise
- Break solutions into small incremental steps
- Do one step at a time

# Tech stack

- .NET (netstandard2.0 library, net5.0 tests)
- F# (primary library implementation)
- C# (interop bindings in `src/Fennel/CSharp.fs`)
- FParsec (parsing library)
- xUnit for testing
- Unquote for F# test assertions

# Build and test

- Build: `dotnet build`
- Test: `dotnet test`
- Clean: `dotnet clean`

# Structure

Fennel is a .NET library that parses and formats Prometheus metrics (exposition format) strings to/from typed F# models.

- `src/Fennel/` - Main library (F# with C#-friendly bindings)
  - `Model.fs` - Discriminated unions representing Prometheus line types
  - `Parser.fs` - FParsec-based parser for Prometheus exposition format
  - `Prometheus.fs` - Public F# API
  - `CSharp.fs` - C#-friendly wrapper API (`Fennel.CSharp` namespace)
  - `Result.fs` - Result type helpers
- `test/Fennel.Tests.Unit/` - F# unit tests
- `test/Fennel.CSharp.Tests/` - C# integration tests
