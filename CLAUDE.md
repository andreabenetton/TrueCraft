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
