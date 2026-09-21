# NPC Contact Damage Input Boundary

The NPC contact-damage route now ignores candidates with invalid identity or geometry before AABB
intersection. NPC and player handles must be valid; positions and collider dimensions must be
finite, and widths/heights must be positive. Existing active/cooldown gates and strict overlap
semantics remain unchanged, so a valid overlap still emits one typed `DamagePlayerCommand`.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`, SHA256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`, NPC/player hitbox
intersection branches around lines `47159` and `64456`. The source uses hitbox intersection for
contact decisions; this card protects the ECS equivalent from forged numeric state.

Accepted: active valid NPC/player identities, finite positions, positive finite colliders, cooldown
zero and strict AABB overlap. Rejected: invalid handles, inactive candidates, cooldown candidates,
non-finite positions/dimensions and non-positive collider dimensions. Deferred: type-specific
contact immunity, knockback, difficulty modifiers, collision/LOS and presentation/network effects.
