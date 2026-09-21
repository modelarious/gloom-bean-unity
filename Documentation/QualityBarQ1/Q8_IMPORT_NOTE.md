# Q8 import preflight

The175-image native build passed. The subsequent gameplay runner refused execution because Unity expanded four new minimal PNG importer metadata files during import. This is an intentional clean-source guard, not a gameplay PASS or test failure. The expanded metadata is preserved as source and the next current test ID rebuilds from that committed state. No runtime C# or PNG art is altered by this recovery.
