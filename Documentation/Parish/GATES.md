# Parish completion increment

This increment does not remove the remaining full-campaign requirements. It implements and tests World 1 as a connected first playable chapter. Existing foundation tags are immutable.

- [ ] P01 The four Parish courses can be completed from their real entrances with production inputs, including possession, key, Turn and return.
- [ ] P02 The four Mercy secrets have real input-only solutions and are not prerequisites of ordinary completion.
- [ ] P03 The Kindly Usher can be defeated through all three acts using production inputs, with normal damage enabled.
- [ ] P04 Progression and saves correctly unlock the boss then World 2; practice never grants progress.
- [ ] P05 The old mechanics suite and Sunday Best route still pass in a real native build.
- [ ] P06 The release is committed, source and tested binary are packaged, and GitHub publication is independently checked.
- [ ] P07 Source/cure/rail/pulse cues and recovery controls are readable in native screenshots; human enjoyment remains UNKNOWN.

Acceptance command: Tools/Verify-Windows.ps1 -Suite Parish, plus Mechanics and OpeningRoute. The Parish suite sends only InputFrame values into production ActorMotor: no direct position, health, pickup, gate, Turn, phase or completion mutation. World observations may guide the scripted input; this is a route proof, not a blind-player test.
