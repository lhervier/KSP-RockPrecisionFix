# TODO

## Test in game

Measured, with [Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix) installed:

- **Reloads** (see [Rock Precision Fix Diag, with this mod](docs/checking-the-culprit.md#rock-precision-fix-diag-with-this-mod)).
- **The holder pools over a flight** 5 km over the Mun, down to the crash: every holder goes back to its
  pool, none is lost (see [the holder pools, over a flight](diag/README.md#the-holder-pools-over-a-flight)).

Still to run, with [Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag) and
both fixes installed, a holder record (`Alt+Shift+F6`) before and after each step, and the same steps
with Terrain Precision Fix alone to compare:

- **Scene switches**, from `reference-mune.sfs`: to the Space Center, where the Mun's pools are read
  after its terrain was switched off, and back through the Tracking Station.
- **Map view** and back, where the terrain keeps being built and destroyed.
- **Time warp** on the ground, then back to normal speed.
- **The rocks through floating origin shifts**: rock records (`Alt+F6`) during the flight 5 km over the
  Mun, where the origin shifts every few tens of seconds. The rocks should stay on the ground, since the
  holder now follows the quad through `PQ.FastUpdateSubQuadsPosition` and
  `PQ.PreciseUpdateSubQuadsPosition`.

Check `KSP.log` for errors.

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
