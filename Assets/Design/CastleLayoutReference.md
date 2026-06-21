# Castle Layout Reference

This is a development reference for spatial continuity, not final UI.

Direction convention: north is up on the map. Door markers should sit on the same side of a room that the corresponding in-room exit uses.

The current prototype has two playable areas, but they are not adjacent rooms:

- Crypt / Coffin Chamber: Dracula's current room.
- Central Demon Door: the open passage out of the crypt.
- Lower Processional: future room.
- Reliquary Gallery: future room.
- Broken Stair: future room.
- Service Corridor: future room.
- Servant Wing / Renfield: Renfield's daytime support area.

Rule: the servant wing is several rooms away from the crypt. Prototype portals may jump across the unbuilt rooms, but room art must not overlap or sit on top of another room.

The current world-space layout follows the same idea: the crypt is near `x = 0`, the castle map is centered near `x = 21.5`, and the servant wing is near `x = 42`, leaving a large coordinate gap for the intervening castle route.

Current travel rule:

- Crypt central demon door -> Castle Map crypt gate.
- Servant wing left door -> Castle Map servant-wing gate.
- Castle Map crypt gate -> Crypt.
- Castle Map servant-wing gate -> Servant Wing.

There should be no direct servant-wing-to-crypt portal. The castle map is the connective tissue until the intervening rooms become real playable rooms.

The servant wing currently has two door positions. The left door is the active service-corridor route to the castle map. The second door must still be represented on the map as a separate exit marker/stub, even if it does not connect to a built destination yet.

Visual reference: `Assets/Art/Castle/castle_layout_reference.png`.
