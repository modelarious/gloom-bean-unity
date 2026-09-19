# Gates: Orchard continuation

OWNS: Assets/GloomBean/Runtime/Campaign/**, Assets/GloomBean/Runtime/Foundation/GameRoot.cs, Tools/**, Documentation/**

Scope: Continue the authorized full atlas implementation by completing and validating the next material-system chapter, preserving all remaining full-game requirements. No new game-design proposal is silently selected.

- [x] O1: Recover the newer Parish checkpoint without losing source or evidence.
  EVIDENCE: 282 Unity files matched the clean-run identity manifest, exact or CRLF-normalized; source and prior execution evidence committed before new work.

- [ ] O2: All four Orchard levels have complete production-input critical routes and returns.
  CHECK: powershell -NoProfile -File Tools/Verify-Windows.ps1 -Suite Campaign -Route W2
  EXPECT: PASS Campaign
  EVIDENCE: pending

- [ ] O3: Four Orchard Mercies have complete optional routes and successful returns.
  CHECK: powershell -NoProfile -File Tools/Verify-Windows.ps1 -Suite Campaign -Route W2 -WithSecrets
  EXPECT: PASS Campaign
  EVIDENCE: pending

- [ ] O4: The Orchard Judge has a normal-damage, input-only three-act victory.
  CHECK: powershell -NoProfile -File Tools/Verify-Windows.ps1 -Suite Campaign -Route GB-B2
  EXPECT: PASS Campaign
  EVIDENCE: pending

- [ ] O5: Native mechanics and first-world regression remain green after the material-system changes.
  CHECK: powershell -NoProfile -File Tools/Verify-Windows.ps1 -Suite Mechanics
  EXPECT: PASS Mechanics
  EVIDENCE: pending; also requires full Parish critical/secret reruns in release evidence.

- [ ] O6: Clean source, native player and recoverable history are packaged with measured hashes.
  EVIDENCE: pending; GitHub publication is a separate checked boundary, not implied by local commit.

- [ ] O7: Native screenshots expose readable sources, state changes and route decisions.
  EVIDENCE: pending; uncoached human enjoyment and physical controller feel remain UNKNOWN.
