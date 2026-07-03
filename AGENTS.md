# Agent Context

## .NET Verification Rules

This repository can be open in VS Code with C# Dev Kit while the agent is working.
In the agent sandbox, full solution builds and `dotnet test` can be slow or unreliable because MSBuild/Roslyn processes and vstest socket communication may behave differently than on the developer machine.

## Line Endings

This repository uses CRLF line endings for C# files through `.editorconfig`.
When creating or editing text files, preserve the repository line-ending style instead of introducing LF-only files.

Before finishing changes, check for Git line-ending warnings in changed files. Avoid leaving warnings like:

```text
warning: in the working copy of '<file>', CRLF will be replaced by LF the next time Git touches it
```

If a newly created or edited file has LF endings while the repository expects CRLF, convert only the files changed by the agent to CRLF.

## Code Modeling Conventions

Use model types according to their role:

- EF entities: mutable `class` with `get`/`set`.
- DTO, request, response, and integration contracts: `sealed record` with `init`/`required`.
- Settings/options: `sealed class` with `init` or `set` properties and options validation.

Do not convert EF entities to records. EF entities participate in object tracking, relationship fixup, change detection, and migrations; record value equality is usually the wrong semantic model there.

Prefer `sealed record` for public API DTOs and contract models because they are value-like, serialize cleanly, and work well in tests.

Keep settings/options as classes for predictable `ConfigurationBinder`, `IOptions<T>`, and `ValidateOnStart()` behavior.

Use targeted verification by default:

```bash
dotnet build <path-to-project>.csproj --no-restore -v minimal
```

When changes touch tests, verify test projects by building the specific test project:

```bash
dotnet build <path-to-test-project>.csproj --no-restore -v minimal
```

Avoid full solution builds unless the user explicitly asks for them:

```bash
dotnet build <solution>.sln
```

Avoid relying on `dotnet test` from the agent sandbox unless explicitly requested. In this environment it may fail before executing tests with a `System.Net.Sockets.SocketException (13): Permission denied` from vstest communication. If that happens, report it as an environment limitation and ask the developer or CI to run the actual test execution.

Do not run `dotnet build-server shutdown` as a routine step. It can disturb the developer's active VS Code/C# Dev Kit session. Use it only when clearly needed or when the user asks for it.

For normal code changes, the preferred verification sequence is:

```bash
dotnet build src/<service>/<service>.csproj --no-restore -v minimal
dotnet build src/tests/<changed-test-project>/<changed-test-project>.csproj --no-restore -v minimal
```

If formatting needs to be checked, scope `dotnet format` to the files changed by the agent instead of formatting the whole solution:

```bash
dotnet format <solution>.sln --no-restore --verify-no-changes --include <changed-file-1> <changed-file-2>
```
