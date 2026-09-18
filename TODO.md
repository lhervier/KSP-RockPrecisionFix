# TODO

## Test in game

To run, with [Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag) and
both fixes installed, a holder record (`Alt+Shift+F6`) before and after each step, and the same steps
with Terrain Precision Fix alone to compare:

- **Time warp** on the ground, then back to normal speed.
- **The rocks through floating origin shifts**: rock records (`Alt+F6`) during the flight 5 km over the
  Mun, where the origin shifts every few tens of seconds. The rocks should stay on the ground, since the
  holder now follows the quad through `PQ.FastUpdateSubQuadsPosition` and
  `PQ.PreciseUpdateSubQuadsPosition`.

Check `KSP.log` for errors.

## Kopernicus

Read, not measured. Kopernicus replaces the stock holder with a subclass that inherits `Setup` without
redeclaring it, and releases it through the same `DestroyQuad`, so it should be covered (see
[What has been read](docs/should-you-install-it.md#what-has-been-read)). It also replaces the pool's
container at runtime, which the fix reads again at every call.

With `scatterColliders`, Kopernicus gives each scatter object a `MeshCollider` on a child GameObject of
the holder, with a quad-local position. Those would follow the holder under the quad, so their offset
should go away with it.

To measure with Rock Precision Fix Diag, Kopernicus installed.

## Parallax

Read, not measured: Parallax has its own scatter system, keyed by quad, and does not touch the stock
holders (see [What has been read](docs/should-you-install-it.md#what-has-been-read)). To check in game,
with Parallax installed.
