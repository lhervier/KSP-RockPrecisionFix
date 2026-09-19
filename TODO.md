# TODO

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
