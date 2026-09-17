# TODO

## Test in game

Nothing has been run in game yet. With
[Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag), one reading recorded
per step, on stock, with [Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix),
and with both:

- **Reloads**: the same save loaded five or six times. Every holder under its own quad, and under each
  holder the height of its objects above the ground the same on every load.
- **Scene switches**: to the Space Center and back, to the Tracking Station and back. No holder should
  be left under a quad that went back to the PQS cache.
- **Map view** and back, where the terrain keeps being built and destroyed.
- **A rover driven across several floating origin shifts**: the scatter should stay on the ground the
  whole way, since the holder now follows the quad through `PQ.FastUpdateSubQuadsPosition` and
  `PQ.PreciseUpdateSubQuadsPosition`.
- **Time warp** on the ground, then back to normal speed.
- **To orbit and back down**: the quads that carry scatter should be destroyed on the way up and built
  again on the way down, sending their holders through the pool.

Check `KSP.log` at the `Trace` level for errors, and that as many holders go back to their pool as are
hung from a quad.

## Breaking Ground surface features (ROC)

`PQSMod_ROCScatterQuad.Setup` does the same `base.transform.localPosition = quad.positionPlanet`, under
a `rocParent` that `LandClassROC` creates as a child of the terrain sphere, at the identity. The holder
is released through `roc.DestroyQuad(this)`, called from the quad's `onDestroy`. Same scheme as the
stock scatter, so the same fix should apply: hang the holder from its quad in a postfix of `Setup`,
hand it back to `rocParent` in a prefix of `LandClassROC.DestroyQuad` (internal).

Unlike the rocks, surface features have colliders, so the offset may be physical, not only visual.
Whether the physics takes their pose from the same matrix as the renderer, or from the transform
position, is not measured.

The identifier of a surface feature depends on its position within the quad (`rocPOS`, taken from
`quad.verts`), not on the pose of its holder: this fix would not change it.

## Kopernicus scatter colliders

With `scatterColliders`, Kopernicus gives each scatter object a `MeshCollider` on a child GameObject of
the holder, with a quad-local position. Those would follow the holder under the quad, so their offset
should go away with it. To check with Kopernicus installed, along with the rest of Kopernicus
compatibility (its holder subclass, its replacement of the pool's container at runtime).
