# Gloom Bean - reusable Unity platformer foundation

This branch contains the reusable base only. It has no atlas campaign or possession dependencies.

Open with Unity 6000.5.9f1. Open Assets/GloomBean/Scenes/Boot.unity and press Play, or use Gloom Bean > Campaign workbench. Select FOUNDATION and any of the four mechanical courses or the foreman boss. The movement tuning asset is under Assets/GloomBean/Resources/MovementTuning.asset.

Controls: WASD/arrows move; Space/Z jump (release for short jump); Shift run; J/X tackle; L or airborne Down+J ground pound; K/C pick up stunned enemies or throw; E interact. Aim throws with Up/Down. Down crouches/crawls and begins a roll on slopes. Tackle while swimming dashes. Esc pauses. F1 displays controls. Standard Windows gamepad mappings are provided but physical-controller acceptance is unmeasured.

Run Tools/Build-Windows.ps1 as your normal licensed Windows user; then Tools/Verify-Windows.ps1 -Suite Mechanics. The opening-route suite belongs only to the atlas branch. All tools have bounded timeouts and isolated test saves.

This is original tuning, not an extracted or frame-perfect Wario Land 4 controller. No Nintendo assets, ROMs, paid assets or external AI dependencies are included.

The original foundation-v0.1.0 commit preceded all campaign work. This branch retains that ancestor and adds tested controller fixes, complete Unity project settings, the tuning asset and editor workbench. Read the acceptance reports rather than treating import success as a human-feel or full-course playthrough certificate.

Authoring: FoundationCampaign.cs composes StageBuilder.cs primitives; ActorMotor.cs owns movement; CarryableEnemy.cs owns patrol/stun/carry/throw; StageSession.cs owns the return loop; Progress.cs owns save data. Runtime-built objects are inspectable during Play. Permanent layout changes are made in the authoring code, not by saving callback-bearing runtime objects as a production scene.
