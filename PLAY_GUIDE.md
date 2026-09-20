# Gloom Bean — play and experimentation guide

## Starting a journey

Choose **BEGIN / CONTINUE HOST CYCLE**, then the first available world and level. The opening is intentionally pleasant. Something in the first level permanently changes the Bean. After that, temporary creatures can occupy the Open Host; being cured returns the damaged base, not the original cute figure.

Move with arrows/WASD, jump with Space, run with Shift, tackle with J, carry/throw with K, interact with E. A short button press makes a lower jump. Stun an enemy before picking it up; directional input aims the throw. Down crouches or crawls and begins a roll on slopes. In water, tackle becomes a swim dash. In air, L or Down+J performs a pound; sufficient vertical clearance produces the stronger version. These base verbs remain separate from the fifteen possessions.

The world and stage menu track earned progress. The boss opens after its four levels. Complete it to open the next world. **Continue the journey** on a clear screen advances to the appropriate next stage. **Practice** exposes every level/boss but does not bank progress or secrets; it is the quickest way to examine a mechanic.

## Reading a level

The entrance is your exit. Find the Keyling and explore toward the World Nail, then press E to pull it. The resulting **Turn** changes the level itself: opened backstage bracing, reversed pumps, moving geometry, altered shadows or falling architecture. Some stages are timed and others are not; the opening level has no mandatory escape clock. Watch the actual route and the HUD rather than expecting one uniform return rule.

Every level contains one optional Mercy. A normal clear does not require it. Secrets are banked only after a successful normal return. Exactly twenty unlock the restored ending. Nineteen is not enough. Clearing the final boss in Practice does not alter this rule.

## Possessions: controls, not solutions

Only a creature/source in the environment grants a possession. Sources may resemble hazards; the resulting physical rule makes their purpose understandable. On contact, the HUD gives the current rule and cure. In compatible pairs, **Down+I** changes the focused form; do not hold Down while trying to perform an ordinary secondary action.

| Form | Player control | What to watch |
|---|---|---|
| Echo | Normal movement; U resynchronizes | The second body replays inputs two seconds later and is independently blocked by furniture. |
| Marionette | Left/right moves the overhead hook; up/down reels; U transfers at a rail junction | Steer the anchor, not the swinging body. Stage shears cut the tether. |
| Molt | U sheds a husk; I or nearby E reclaims it | Two husks maximum. Your abandoned weight remains physical while the small core cannot tackle. |
| Wax | U melts/reforms; I deposits; Up+I absorbs nearby wax | Liquid cannot jump. Plugs use actual body volume and can redirect flow. |
| Gullet | Aim+U swallows or replaces one terrain chunk; I returns stored terrain home | The removed tile leaves a real hole and its support relationships matter. |
| Root | Hold U and steer through wet substrate; release to retract; I cancels growth | The exposed body stays behind until it follows the exact root curve. |
| Mirror | One movement stream drives two opposite bodies | Furniture changes their alignment. Either body can lose the route. |
| Inside-Out | Ordinary movement in the enclosed interior topology | Read closed outlines as passages; open space may no longer support you. |
| Parallax | Up/down at overlapping silhouettes changes depth | Screen position stays fixed while scale, reach and collision domain change. |
| Censer | Stand still to grow the field; move to dissipate it | Nearby machinery slows while you stay responsive. You cannot freeze the whole world. |
| Stitch | Aim+U chooses two compatible seams; U tugs; I cuts | Architecture really folds. While aiming a stitch, an unfocused Coffin will not also flip. |
| Coffin | Left/right makes a corner-pivot quarter-turn | Orientation and available swept space replace jumping. A horizontal lid can bear a load. |
| Lodestone | U reverses polarity | Equal poles repel; opposites attract. Both free bodies react. Learn which mass is anchored. |
| Shadow | I switches body/shadow control; direction follows connected silhouettes | The physical body stays vulnerable; the fourteen-metre tether is real. |
| Ink | Movement writes; U toggles recording | The footpath hardens after one second and expires after eight, within an eighteen-metre length budget. Wet ink is not a platform. |

## When a route appears blocked

Look first at body size, remaining wax, the location of your husks, the focused possession and whether the platform is moving. A floor that was reachable before the Turn may now be overhead or absent. A useful source may need to be accepted again after a cure. The game does not grant extra jumps to hide these constraints.

**Escape > Restart level** resets that attempt. A failed attempt does not erase your previously earned campaign progress. F2 exposes developer notes; this is an iteration aid and contains hints. F1 lists the controls; F4/F5 control audio. F1 and pause render in front of boss instructions.

## Building a new level from this base

Use the independent foundation branch for a clean mechanics template. Use the campaign workbench for a specific stage, adjust MovementTuning in the Inspector, and save static geometry overrides for quick spatial iteration. Edit authored stage builders for moving structures and callbacks. Keep one branch/worktree per simultaneous worker so two Unity Editors do not share the same Library or build output. Commit each meaningful change before a long build.

The exact reference atlas remains available beside the source. The current release is a native implementation of its mechanics and campaign, not a pixel-for-pixel reproduction of its schematic diagrams or a frame-perfect Nintendo controller. Subjective polish and uncoached controller feel are separate acceptance questions.
