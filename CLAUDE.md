## Git discipline
After each logical unit of work:
- create a git commit
- push to the current branch

If push cannot be completed because of credentials, remote access, branch protection, or environment limits:
- say so explicitly
- do not claim the push succeeded

Commit messages must be short, specific, and scoped to the actual change.
Do not leave completed logical units of work uncommitted.
Do not add a "Co-Authored-By" trailer to any commit message.

### Multi-fix prompts
When a single prompt asks for **more than one unrelated fix** (different files,
different bugs, different ADRs, different concerns — not the natural sub-tasks
of one feature), do not bundle them into a single commit. Instead, for each
fix in turn:

1. implement only that one fix
2. add or update only the tests directly related to it
3. run the impacted tests; verify they pass
4. create one commit scoped to that fix (with a commit message describing only it)
5. push, then move to the next fix

Each fix becomes one commit. Each commit is independently reviewable, revertable,
and bisectable. A multi-fix prompt produces N commits, not one.

Related sub-tasks of the same fix (e.g., a code change plus its test plus a
docstring update plus a doc cross-reference) belong in the same commit — they
are not "different fixes". The discriminator is whether the changes share a
single root cause, ADR, or feature; if yes, one commit; if no, separate commits.

Do not bundle "while I'm here" cleanups into a fix commit. If a stale comment
or unrelated drift is discovered mid-fix, either: (a) note it explicitly and
defer it; or (b) handle it as its own follow-up commit after the in-scope fix
is committed.

## Dependency injection

Get every dependency through the constructor. The DI container does this for
you when types are resolved via `ActivatorUtilities.CreateInstance` or via
`services.GetRequiredService<T>`. Inside a type's body, do not reach into the
global service provider (`App.Services.GetService<T>()` / `App.LoggerFor<T>()`
into a static field) to fetch dependencies — that's the service-locator
anti-pattern: hidden coupling, broken testability, surprising order-of-init
bugs when the static field initializer runs before the container is built.

If a dependency is genuinely optional and most callers don't have one (e.g.
a logger in `TrueCraft.Core` classes that are constructed by unit tests
without bootstrapping a container), accept it as a nullable constructor
parameter with `NullLogger<T>.Instance` (or equivalent) as the fallback —
not as a static lookup from `App.Services`.

## Logging strategy

Diagnostic logging is configuration, not code churn. When chasing a bug:

1. Add `LogDebug` / `LogTrace` calls at the suspected hot spots.
2. Reproduce, read the log, find the cause, fix it.
3. **Leave the log calls in.** Suppress them via the Serilog `MinimumLevel`
   override in the relevant settings file (e.g. `launchersettings.json`'s
   `Serilog.MinimumLevel.Override`) — flip the source from `Debug` to
   `Information` so the calls compile out at the filter level instead of
   being deleted from the source.

Deleting log calls is a destructive move; reach for it only when the
message itself turns out to be wrong or misleading, never for "this is
noisy by default".

### Hot-path guards

`ILogger.LogDebug(template, args...)` is **not** free when the level is
disabled — `args` are still evaluated and boxed into an `object[]` even
if the formatter never runs. In a hot path (called hundreds of thousands
of times per frame: `World.GetChunk`, `Region.GetChunk`,
`LoadOrGenerateRegion`, etc.) that allocation alone produces enough
pressure to overrun Serilog's bounded `Async` sink queue and trigger
"unable to enqueue" warnings.

**Where guards belong:** methods that run inside a tight loop or get
called from one (per-frame, per-chunk-access, per-block, etc.). At the
top of such a method, cache `IsEnabled` once and short-circuit each
call site:

```csharp
var debug = _log.IsEnabled(LogLevel.Debug);
if (debug) _log.LogDebug("World.GetChunk({Chunk}) start", coordinates);
// ... work ...
if (debug) _log.LogDebug("World.GetChunk({Chunk}) done", coordinates);
```

**Where guards do NOT belong:** code paths that only run a handful of
times per process — constructors, one-shot startup steps, error
branches, file I/O at world-load time, etc. There the
allocation cost is irrelevant and the guard is just noise. Plain
`_log.LogDebug(...)` is fine.

Rule of thumb: ask "could this method ever run >1000× per second?" If
yes, guard. If no, don't.
