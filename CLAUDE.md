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
