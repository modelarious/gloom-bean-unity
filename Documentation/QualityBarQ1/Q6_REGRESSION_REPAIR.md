# Q6 integration failure and correction

All175 native views and all12 input-route cases passed, but the mechanics suite stopped after131 successful checks with two NullReferenceExceptions. A pre-existing independent fixture builds scenery on a child of StageSession, not the StageSession object itself. QualityBarWorld incorrectly used GetComponent and dereferenced null during initialization.

The renderer now resolves the owning session through its parents, like the existing material components. Its group decoder handles non-campaign/base fixture IDs without throwing; actual W1-W5 and first-corruption choices remain unchanged. This is a renderer lifecycle correction, not a weaker test, a removed fixture or a change to physics. All original negative results are retained. The next run repeats the entire13-case current suite.
