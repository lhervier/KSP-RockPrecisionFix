# The fix this mod proposes

Part of [Rock Precision Fix](../README.md). The short version is on the main page, under [The fix this mod proposes](../README.md#the-fix-this-mod-proposes); here are the two patches in detail.

## Hanging each holder from its quad

Each holder hangs from its own quad, at no offset. The objects are then drawn in the frame they were
built in, with the very matrix the ground is drawn with, and no transform in the chain holds a 600 km
vector any more.

- **After `PQSMod_LandClassScatterQuad.Setup`**, which gives a holder its quad: the holder is re-parented
  under the quad, with zero position, identity rotation and unit scale. From then on it follows the quad
  through everything stock does to it, floating origin shifts (`PQ.FastUpdateSubQuadsPosition`) and
  re-placements (`PQ.PreciseUpdateSubQuadsPosition`) included, with nothing more to do.
- **Before `PQSLandControl.LandClassScatter.DestroyQuad`**, which returns a holder to its pool when its
  quad is destroyed: the holder goes back under the pool's container, as stock placed it, before the
  quad itself goes back to the PQS cache to be reused elsewhere. A prefix, because the stock method
  starts by clearing the holder's quad.

This is not a new way of drawing scatter. It is the stock scatter, drawn from the frame stock built it in.

## Only where scatter grows

Only the holders of quads that hang from `LocalSpacePQStorage` are moved. In stock, scatter only exists
on those quads, and any other quad hangs from the sphere with the same kind of 600 km local position as
the holder, so hanging the holder from it would gain nothing.

Holders on a sphere whose quads are not surface relative are left alone too: there, stock places the
holder at the centre of the body, not at a long vector.

## Why moving the holder is safe for stock

Read in the stock code:

- a destroyed quad calls its `onDestroy` delegates, which release its holder, before it goes to the PQS
  cache, so no holder travels with a recycled quad;
- `PQS.ResetSphere` destroys the quads before the scatter destroys its holders;
- the visibility of a holder is switched with `obj.SetActive` from the quad's `onVisible` and
  `onInvisible` delegates, not through the hierarchy, so hanging it from the quad does not change when it
  is shown.

## Safeguards

- if any patch fails to install, no holder is ever moved;
- a holder that cannot be hung from its quad stays where stock placed it;
- the release only acts on a holder that hangs from its own quad, so it never moves a holder this mod did
  not move.

