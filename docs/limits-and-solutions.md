# Limits and solutions

Part of [Rock Precision Fix](../README.md).

What this fix can leave out, and what it has not been checked against yet — each with its solution, or
with what is still missing for one.

## Without Terrain Precision Fix

**Limit.** Not measured. On its own, this mod should keep the scatter on the stock ground, which still
comes back at a different height at every load (see
[Rock Precision Fix Diag, on stock](checking-the-culprit.md#rock-precision-fix-diag-on-stock)).

**Solution.** Install it with Terrain Precision Fix. Measuring it alone takes the same protocol, on the
same save.

## Mods that look for the holders

**Limit.** A mod that finds scatter holders, or the children of a terrain quad, through the scene
hierarchy sees them elsewhere than stock puts them. Only stock, Kopernicus, Parallax, KSP Community Fixes
and TUFX have been read (see [Should you install it?](should-you-install-it.md)).

**Solution.** None in this mod: moving the holder is the fix. Do not install it on its own. In KSP
Community Fixes, it would be for the mods concerned to adapt (see
[If it went into KSP Community Fixes](should-you-install-it.md#if-it-went-into-ksp-community-fixes)).

## Surface features with colliders

**Limit.** Breaking Ground's surface features are placed the same way (`PQSMod_ROCScatterQuad.Setup`
does the same `localPosition = quad.positionPlanet`), and they have colliders, so their offset may be
physical, not only visual. This mod does not handle them.

**Solution.** Not there yet. The same patch should apply: hang the holder from its quad after `Setup`,
hand it back before `LandClassROC.DestroyQuad`. First measure where the physics puts those colliders
(see [TODO.md](../TODO.md)).

## Not checked yet

- **Kopernicus.** It replaces the stock holder with a subclass that inherits `Setup` without redeclaring
  it, and releases it through the same `DestroyQuad`, so it should be covered. What its code does with
  the holders is in [What has been read](should-you-install-it.md#what-has-been-read). *Solution:* measure it with Rock Precision
  Fix Diag (see [TODO.md](../TODO.md)).
- **Parallax.** See [What has been read](should-you-install-it.md#what-has-been-read). *Solution:* check it in game.
- **Scene switches, the map view, time warp, a rover driven across floating origin shifts, a trip to
  orbit and back**, where quads are built and destroyed, and holders sent through the pool. Only loading a
  save is measured. *Solution:* one record per step with Rock Precision Fix Diag, and the `Trace` log to
  check that as many holders go back to their pool as are hung from a quad (see [TODO.md](../TODO.md)).

