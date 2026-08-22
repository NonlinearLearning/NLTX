# M-001 supported projectile definition boundary

## Narrow proposition

The current supported projectile slice uses immutable `ProjectileDefinition` records and a typed
`ProjectileDefinitionRegistry`. Supported spawn, lifetime, collision and damage systems resolve
definitions before allocating or mutating projectile state; unknown projectile types remain outside
the supported registry.

## Source boundary

Version4 `Initialize_AlmostEverything` calls `Projectile.InitializeStaticThings` and then defaults
every `ProjectileID` while populating `projHostile` and `projHook` arrays. This card does not claim
that complete legacy projectile defaults, AI styles, frame tables, hostile/hook arrays or all
projectile IDs have been migrated.

## Status

Accepted narrowly as a child of M-001. Full projectile static initialization remains deferred under
the projectile-specific migration plan.
