# Recovered checkpoint: do not restart from the opening level

Observed 2026-09-20 UTC / 2026-09-19 Edmonton. This checkpoint preserves already-existing code and reports, not a new successful run.

- Main HEAD before preservation: b47386ce1adf46fab87b65a44cca49c83e2db02f.
- Independent foundation HEAD: cfd65bff6cf5024459cb300beb02863c6fd24c59.
- Eight implementation/evidence commits since delivered 9a1c92a are still in local history; no code loss established by this inspection.
- Only two files were untracked in main: Tools/orchard-request.json and Tools/clean-orchard-config.json. Both are preserved now.
- Old RESUME.md and RELEASE_STATUS.md are stale; the detailed agent handoff being added next supersedes their work-position claims.
- Source repository has no remote. Project Factory public request #38 and older private request #37 encountered bad credentials. Context commits are NOT game-code publication.

## Clean-v04 results (historical execution, directly recovered)
Foundation 61/0/0; combined mechanics 198/0/0 (pass/fail/exception).
W1 critical 138 and W1 secrets 173 checks: exit 0, no failures.
W2 critical 313 and W2 secrets 369 checks: exit 0, no failures, using saves actually earned by W1 runs.
Judge 29 checks: exit 0, no failures.
Unearned-world-denied: expected exit 1 and explicit denial, correctly passed the negative-control oracle.
Practice-isolation: 28 checks, two failures, exit 1. Overall clean batch FAIL. Do not promote the batch to PASS.

## Immediate next work
Read Documentation/Evidence/OrchardRecovery/Practice-isolation/orchard-observations.txt and current OrchardVerification.cs; diagnose the failing production-input practice route. Preserve original reports. Repair the game or the witness only as justified by traced state, rerun into a new directory, then continue City (World 3), Fall (World 4), and Empyrean (World 5). Never swap City and Fall as an older coordination proposal did.

Twelve remaining levels and three bosses are only graybox/component-covered, not whole-route-certified. Final art/audio/readability/controller acceptance remain unfinished or UNKNOWN.
