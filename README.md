# Rock Precision Fix

A fix for stock KSP 1.12, written to go with
[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix) and, like it, kept as
small as possible: two Harmony patches, in one source file. Here is what they fix, for the terrain
scatter drawn around your craft — the rocks, and around the KSC the grass and the trees:

> **KSP never draws terrain scatter at the same height against the ground twice.** Load the same save
> several times, and every rock, tuft of grass or tree comes back drawn a little higher or a little
> lower against the ground each time — several centimetres apart on Kerbin.

**How this was made.** Written with Claude, Anthropic's AI assistant, and reviewed line by line by a
human — me. I am saying so up front, because contributions made with an AI deserve a closer look than
others, and because some people would rather stop reading here. That look is what this page is built
for: every figure on it comes from an in-game measurement, the instrument behind them is public and runs
on a stock install, the logs they are read from are kept, the stock code quoted here is a handful of
lines anyone can check, and the fix fits in one file you can read in a few minutes.

## Why the moving scatter matters

It barely does. Stock scatter has no collider: no craft rests on it and nothing hits it, so a rock drawn a
few centimetres higher or lower than at the last load changes nothing for the game. Scatter is also sunk
into the ground on purpose, so a shift of a few centimetres mostly moves it within the ground, where
nobody sees it.

This fix exists for another reason. [Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix)
changes where KSP builds the ground: it moves the terrain quads, not the scatter drawn on them, and the
scatter ends up drawn further from the ground than in stock. Nobody sees that either. But a fix that makes
anything worse, even something invisible, has to answer for it, and this mod is that answer: with both
installed, the ground and the scatter on it come back at the same place at every load.

### Disclaimer: it is meant to go with Terrain Precision Fix

This mod does not depend on Terrain Precision Fix, and runs without it. It does not fix the same thing,
though. On its own, it keeps the scatter on the stock ground — and the stock ground itself comes back at a
different height at every load, which is what Terrain Precision Fix is about. Only with both are the
ground and the scatter placed exactly, and this mod is only measured with it, below.

## The culprit

Here it is straight away. The next chapter checks it before anything is changed.

The objects of a terrain quad are built from the quad's own vertices, in the quad's own coordinates, in
`PQSLandControl.LandClassScatter.CreateScatterMesh`, decompiled from KSP 1.12.5:

```csharp
scatterPos = Vector3.Lerp(q.quad.verts[num3], q.quad.verts[num2], UnityEngine.Random.value);
```

That position is written as it is into the mesh of a *holder*, a `PQSMod_LandClassScatterQuad` taken
from a pool, one per kind of scatter on the quad. So the objects stand on the ground only if the holder
is drawn with the same origin as the quad.

One value in that method does use the long vector, and gets it right: the local vertical of each object,
along which the object is then sunk into the ground and around which it is turned. It is the direction
from the centre of the body to the object:

```csharp
scatterUp = (scatterPos + q.quad.positionPlanet).normalized;
scatterPos += scatterUp * verticalOffset;
```

`positionPlanet` is a `Vector3d`, so the sum is computed in double precision and normalised there. Only
the result goes into a float, and it is a unit vector, which a float holds to about seven significant
digits. Nothing is rounded to 62.5 mm there, and that line needs no patch.

Both are placed from the same double precision vector, the origin of the quad relative to the centre of
the body, hundreds of kilometres long, stored in a float `localPosition` under the terrain sphere, whose
origin is the centre of the body:

```csharp
// PQ.SetupQuad — positionPlanet is a Vector3d
quadTransform.localPosition = positionPlanet;
// PQSMod_LandClassScatterQuad.Setup
base.transform.localPosition = quad.positionPlanet;
```

Then they part ways. The quads of the highest subdivision level, the only ones that carry scatter, are
moved to a container of their own, `sphere.LocalSpacePQStorage`, outside the body's hierarchy, keeping
the world position Unity computed for them. The holder stays under the sphere, with its 600 km local
position, and Unity draws it with its local to world matrix.

**That is the error.** Unity keeps both the position of an object and its matrix, and computes them
separately. A float holding 600 km can only change in steps of 62.5 mm, and for a local position that
long, the two computations need not round the same way: the translation of the holder's matrix can stand
whole steps away from its position, along the world axes. The quad, now hanging close to the world
origin, is drawn where its position says; the holder is drawn where its matrix says, and its objects that
much above or below the ground.

### Why it is different at every load

A rounding depends on the frame the vector is converted into: the world matrix of the terrain sphere,
whose rotation and translation change from one load to the next. It is the same frame, and the same
reason, as for the ground itself, set out on Terrain Precision Fix's page under
[Why it is different at every load](https://github.com/lhervier/KSP-TerrainPrecisionFix#why-it-is-different-at-every-load).
As there, it is a hypothesis read from the stock code, and the fix does not need it to hold: it removes
the long vector the rounding is drawn from.

### With Terrain Precision Fix

Terrain Precision Fix places the quads of the highest level in double precision. It moves the quad, not
its holder. The holder is still placed and drawn from a 600 km float, so it no longer shares even its
position with the quad: the rounding of the quad's origin, which Terrain Precision Fix removed from the
ground, is still in the holder, on top of the rounding of its matrix.

Both come from a transform holding a vector hundreds of kilometres long. The fix follows from that: hang
the holder from its quad, so that no transform in its chain holds one. How exactly is in
[The fix this mod proposes](#the-fix-this-mod-proposes). First, the culprit has to hold.

## Checking the culprit

Before anything is changed, [Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag)
measures what stock does. Its page carries its method and
[its protocol](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/README.md#the-protocol).
For every quad carrying scatter around a landed craft, it reads the height of the quad, of each of its
holders, and of the matrices they are drawn with; for every object of the quad nearest to the craft, the
height of its lowest point above the ground right under it. Scatter is sunk into the ground on purpose,
so that last height says little by itself: what matters is whether it comes back the same at every load.

Every series below uses
[the save it keeps](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/perfs/README.md#the-save):
a Mk1 command pod landed on Kerbin, about 8 km north-west of the KSC, where the scatter is grass and
trees, loaded six times, in KSP 1.12.5 with Harmony, ModuleManager and KSP Community Fixes 1.41.1. Every
record holds the same 64 quads and 118 holders, all of them built, and names the same nearest quad,
`Kerbin Zn3010000130`, with the same 218 objects: 200 `Grass00` and 18 `Tree00`. The *range* of a
reading is its largest value minus its smallest over the six loads.

### Rock Precision Fix Diag, on stock

The stock series and the one with Terrain Precision Fix alone are kept, with their logs, on
[the instrument's page](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/README.md#what-the-readings-show).
On stock:

- the centre of each holder stands at the height of its quad's centre, to the micrometre;
- the matrix each quad is drawn with stands exactly on its centre: 0.000 mm everywhere, up and across;
- the matrix each holder is drawn with does not: it stands from −84 to +85 mm above its quad's centre,
  by an amount that changes from one holder to the next and from one load to the next, and is 0.000 mm
  only a third of the time;
- every object of the nearest quad comes back at a different height above the ground at every load:
  44 mm apart over the six loads for half of them, from 27 to 116 mm;
- take its holder's offset off, and that range drops to 2 mm for half of them.

The holders stand where their quads are, but are not drawn there, and most of what moves the objects
against the ground is that gap. That is the culprit.

Not all of it: 60 objects keep more than 10 mm once their holder's offset is taken off, up to 82 mm. In
stock the ground itself moves against the centre of its quad from one load to the next, by 54 mm under
half of the objects: that is the defect Terrain Precision Fix corrects, and it is why this fix is measured
with it.

### Rock Precision Fix Diag, with Terrain Precision Fix alone

With Terrain Precision Fix installed, the quads stop moving: the centre of the nearest quad comes back at
the same height to 0.001 mm, against 126 mm in stock. The holders do not follow them:

- their centres no longer stand on their quads': 137 mm apart over the six loads for half of the holders,
  up to 201 mm;
- the offset of their matrices spans −60 to +176 mm, and is never 0;
- every object of the nearest quad comes back at a different height above the ground at every load,
  129 mm apart for half of them, from 125 to 134 mm: about three times as far as in stock;
- take its holder's offset off, and no object moves by more than 6.9 mm.

The ground is fixed, the holders are not, and now all of what moves the objects is the holders.

### Rock Precision Fix Diag, with this mod

The same install and the same save, with this mod added next to Terrain Precision Fix, the save loaded
six times in a single session of KSP, one record taken after each load. The install and the six records
are under [perfs](perfs/README.md).

| | with Terrain Precision Fix alone | with both fixes |
|---|---|---|
| centre of the nearest quad, range | 0.001 mm | 0.001 mm |
| matrix of each quad against its centre | *up* and *across* 0.000 mm everywhere | the same |
| centre of each holder against its quad's | range 137 mm (median over the holders), up to 201 mm | the same height, to the micrometre |
| *up* of the holders' matrices | −60 to +176 mm, never 0 | 0.000 mm everywhere, and *across* as well |
| each object above the ground, range | 129 mm (median over the objects), from 125 to 134 mm | 0.027 mm (median), 0.114 mm at most |

**The holders.** On all 118 holders and all six loads, the centre of the holder and the matrix it is
drawn with stand at the height of its quad's centre, to the micrometre, and neither is shifted from it,
up or across. The holders are drawn exactly where their quads are.

**The objects.** Every object of the nearest quad comes back at the same height against the ground at
every load: within 0.027 mm for half of them, and only one moves by more than 0.1 mm. On average, the
218 objects stand 323.9 mm below the ground at each of the six loads.

**Nothing else moves.** In the records taken with Terrain Precision Fix alone, an object's height above
the ground minus its holder's *up* is where that object would stand without the holder's offset. Averaged
over the six loads of each series, object by object, that height and the one measured here agree to
0.16 mm (median), 0.9 mm at most: the fix removes the holders' offset, and nothing else about where the
objects are drawn.

This is also the last proof that the culprit is the right one. The fix changes the transform a holder
hangs from, and nothing about the objects in it; were the cause elsewhere, the objects would still move.

In other words: reload the same save as many times as you like, and the scatter comes back at the same
place, on ground that is in the same place.

## The fix this mod proposes

### Two ways out, one taken

**Placing the holder more precisely** where it hangs does not help: as long as a transform in its chain
holds a vector hundreds of kilometres long, Unity rounds it in steps of 62.5 mm on Kerbin. Taking the
holder out of the sphere and placing it in double precision could work, but it would then have to be
moved at every floating origin shift, and placed again whenever stock places its quad again: the very work
stock already does for the quad.

**Hanging the holder from its quad** gets all of that for free, and that is what this mod does.

### Hanging each holder from its quad

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

### Only where scatter grows

Only the holders of quads that hang from `LocalSpacePQStorage` are moved. In stock, scatter only exists
on those quads, and any other quad hangs from the sphere with the same kind of 600 km local position as
the holder, so hanging the holder from it would gain nothing.

Holders on a sphere whose quads are not surface relative are left alone too: there, stock places the
holder at the centre of the body, not at a long vector.

### Why moving the holder is safe

Read in the stock code:

- a destroyed quad calls its `onDestroy` delegates, which release its holder, before it goes to the PQS
  cache, so no holder travels with a recycled quad;
- `PQS.ResetSphere` destroys the quads before the scatter destroys its holders;
- the visibility of a holder is switched with `obj.SetActive` from the quad's `onVisible` and
  `onInvisible` delegates, not through the hierarchy, so hanging it from the quad does not change when it
  is shown.

### Safeguards

- if any patch fails to install, no holder is ever moved;
- a holder that cannot be hung from its quad stays where stock placed it;
- the release only acts on a holder that hangs from its own quad, so it never moves a holder this mod did
  not move.

## Performance

Not measured. What this mod adds runs once each time a holder is given a quad or released, not once per
vertex: a re-parenting and three transform assignments. That should be negligible next to building the
quad and its scatter, but it has not been checked (see [Not checked yet](#not-checked-yet)).

## Limits and solutions

What this fix can leave out, and what it has not been checked against yet — each with its solution, or
with what is still missing for one.

### Without Terrain Precision Fix

**Limit.** Not measured. On its own, this mod should keep the scatter on the stock ground, which still
comes back at a different height at every load (see
[Rock Precision Fix Diag, on stock](#rock-precision-fix-diag-on-stock)).

**Solution.** Install it with Terrain Precision Fix. Measuring it alone takes the same protocol, on the
same save.

### Surface features with colliders

**Limit.** Breaking Ground's surface features are placed the same way (`PQSMod_ROCScatterQuad.Setup`
does the same `localPosition = quad.positionPlanet`), and they have colliders, so their offset may be
physical, not only visual. This mod does not handle them.

**Solution.** Not there yet. The same patch should apply: hang the holder from its quad after `Setup`,
hand it back before `LandClassROC.DestroyQuad`. First measure where the physics puts those colliders
(see [TODO.md](TODO.md)).

### Not checked yet

- **Kopernicus.** It replaces the stock holder with a subclass that inherits `Setup` without redeclaring
  it, and releases it through the same `DestroyQuad`, so it should be covered. It also replaces the
  pool's container at runtime; the mod reads it at every call. Its optional scatter colliders
  (`scatterColliders`) are children of the holder, so they would follow it under the quad. *Solution:*
  measure it with Rock Precision Fix Diag (see [TODO.md](TODO.md)).
- **Parallax.** Its source does not reference the stock scatter holders. *Solution:* check it in game.
- **Scene switches, the map view, time warp, a rover driven across floating origin shifts, a trip to
  orbit and back**, where quads are built and destroyed, and holders sent through the pool. Only loading a
  save is measured. *Solution:* one record per step with Rock Precision Fix Diag, and the `Trace` log to
  check that as many holders go back to their pool as are hung from a quad (see [TODO.md](TODO.md)).
- **Performance.** See [Performance](#performance). *Solution:* measure it.

## Install

Requires KSP 1.12 and [HarmonyKSP](https://github.com/KSPModdingLibs/HarmonyKSP) (the usual
`GameData/000_Harmony`, also installed by KSP Community Fixes).

Copy `GameData/RockPrecisionFixMod` into the `GameData` of KSP. Nothing is written to your saves:
removing the folder gives you the stock scatter back.

## Settings

`GameData/RockPrecisionFixMod/PluginData/settings.cfg` holds a single value, read when KSP starts:

| `logLevel` | what goes to `KSP.log` |
|---|---|
| `Info` (default) | one line at startup, then one line per body the first time its scatter is hung from its quads |
| `Debug` | adds one line per holder hung from its quad, with how far from it the holder was drawn before |
| `Trace` | adds one line per holder handed back to its pool — very verbose, meant for checking |

`Error` and `Warning` are accepted too. To change it: quit KSP, edit the file, start KSP again.

## Build

Set `KSPDIR` to your KSP install folder, which must contain `GameData/000_Harmony`, and run `build.bat`.
It needs the .NET SDK, and produces `GameData/RockPrecisionFixMod/RockPrecisionFixMod.dll`.

## License

MIT
