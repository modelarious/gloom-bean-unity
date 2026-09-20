# Gloom Bean / Host Cycle — native Unity game

Twenty authored levels. Five worlds and bosses. Fifteen temporary possessions. One irreversible first corruption.

## Play

The standalone Windows package requires no Unity installation: extract it completely and run **GloomBeanWindows/GloomBean.exe**. Keep its Data, DLL and MonoBleedingEdge folders together. Choose **BEGIN / CONTINUE HOST CYCLE** for the campaign, **PLATFORMER FOUNDATION** for the reusable movement playground, or **PRACTICE** to inspect any stage without earning progress. F1 opens the complete controls; Escape pauses. F4 toggles sound/master mute, F5 toggles the original music.

Open this source directory in **Unity 6000.5.9f1**, open `Assets/GloomBean/Scenes/Boot.unity`, and press Play. **Gloom Bean > Campaign workbench** provides practice selection, movement tuning and static layout editing. No Asset Store purchase, Nintendo asset, ROM, model download or runtime AI service is required.

**Release evidence:** [current release status](Documentation/RELEASE_STATUS.md), [full campaign acceptance](Documentation/Completion/M1_GATES.md), and the structured audit shipped with the release. A runnable complete campaign is different from commercial-quality polish or blind human acceptance. See the explicit scope in those records.

## The two editions

The original `foundation-v0.1.0` checkpoint predates the campaign. The `foundation` branch remains an independent reusable project: four mechanics courses and a foreman boss, with movement, jump/run/tackle tiers, both pounds, stun/carry/throw, patrols, swimming, crouch/crawl/slope roll, moving platforms/carousels, camera, pickups, selection, escape and saves. The latest complete-history bundle preserves this branch as well as the full campaign; never move the original tag.

The campaign adds **Parish / Orchard / City / Fall / False Empyrean**, four levels and a boss per world. Ordinary progression and all-secret progression are distinct. One Mercy belongs to each level. All twenty exact secrets restore the original Bean for the ending only; the saved corruption and future normal gameplay are not reset.

## Controls

| Action | Keyboard | Common Windows gamepad |
|---|---|---|
| Move, aim, swim | WASD / arrows | Left stick |
| Variable-height jump | Space / Z | A |
| Run | Shift | LB |
| Tackle / swim dash | J / X | X |
| Ground pound | L, or Down + tackle in air | Down + X in air |
| Crouch / crawl / start slope roll | Down | Stick down |
| Pick up stunned enemy / throw | K / C | Y |
| Interact / pull Nail | E | B |
| Possession primary | U | RB |
| Possession secondary | I | Back |
| Focus other active possession | Down + I | Down + Back |
| Pause | Escape | Start |
| Controls / designer notes | F1 / F2 | Keyboard fallback |
| Sound / music | F4 / F5 | Keyboard fallback |

Physical controller mapping and subjective feel require an actual controller check; not every device follows XInput numbering. [The play guide](PLAY_GUIDE.md) explains the possession controls without revealing secret solutions.

## Iterate

See [ITERATION_GUIDE.md](ITERATION_GUIDE.md) for source ownership, safe layout editing and release regression.

`Assets/GloomBean/Resources/MovementTuning.asset` exposes the controller numbers. `Runtime/Foundation/ActorMotor.cs` owns movement; `CarryableEnemy.cs` owns patrol/stun/carry/projectile behavior. `StageBuilder.cs` owns reusable objects, while `StageSession.cs` and `Progress.cs` own the level loop and save model. Campaign forms live in `HostController.cs`, `EmbodimentForms.cs`, `MaterialForms.cs`, `SpatialForms.cs`, `ForceAndTimeForms.cs` and `ShadowForm.cs`. `AtlasCampaign.*.cs` authors the levels; the boss builders are separate from the witnesses.

Geometry and components are ordinary inspectable Unity objects during Play. Static box layout edits can be saved by **Campaign workbench > Edit layout > Save static layout overrides**. These ordinary JSON files survive later runs/builds and reject stale source fingerprints. Moving machinery, source/cure placement and callback rules remain builder-authored. A runtime scene snapshot is for inspection, not a self-contained replacement for callbacks and generated textures. The project does not falsely present twenty hand-authored tilemap scenes.

## Build and verify

Run under your normal licensed Windows account:

```powershell
.\Tools\Build-Windows.ps1
.\Tools\Verify-Windows.ps1 -Suite Mechanics
.\Tools\Verify-Windows.ps1 -Suite Campaign -Route GB-L19 -WithSecrets -Practice
.\Tools\Verify-Windows.ps1 -Suite Empyrean -Route GB-B5 -FinalPair mirror-parallax
```

All twenty levels and five bosses route to their real witnesses. `-Describe` explains the dispatch without launching anything. Every test uses a unique isolated report/save directory. `-SaveSeed <earned-test-save.json> -RequireEarned` copies an actual prior earned save unchanged and records its hash; it never writes the source save. `-ExpectedMercies 0`, `19` or `20` checks the final ending boundary rather than granting secrets. World5 is deliberately tested per stage; use the configured full acceptance graph for the complete earned sequence. See `Tools/campaign-acceptance-m1.json`; replace machine-local clone/report paths when running elsewhere.

`Tools/Test-CampaignGraph.ps1`, `Tools/Test-VerifyRoutes.ps1` and `Tools/test_release_audit.py` test the testing/delivery infrastructure. `Tools/audit_release.py` independently verifies result/exit agreement, every parent-save hash, twenty-level/five-boss saves, exact ending states and source/assembly identity. Component fixtures and scripted-input playthroughs are not blind playtests or a proof of every possible player action.

## Save safety and source continuity

Normal saves use Unity's `persistentDataPath/host-cycle-save.json`, with backup recovery. `-gb-save <absolute-file>` selects another normal-play save. Verification always uses its report-local test save. Practice grants no earned clear, coins, Mercy or permanent corruption.

For continuation read [START_HERE.md](START_HERE.md) and the current checkpoint, not older ZIP descriptions. `checkpoint_work.py` stages only owned paths and refuses unexpected HEAD or pre-existing staged changes; `package_checkpoint.py` bundles committed source and complete history. A clone restored from a bundle may have a file-path origin: that is not GitHub publication.

The unmodified original atlas is included in the release Reference folder; its identity is in `Documentation/Source/ATLAS_REFERENCE.json`. The canonical context is GitHub `modelarious/obsidian-notes`. Standalone public game-repository publication is separately recorded; a local commit, downloadable bundle or context update is not a source-remote push.
