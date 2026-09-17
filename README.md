# Rock Precision Fix

A fix for stock KSP 1.12, for the terrain scatter drawn around your craft — the rocks, and around the
KSC the grass and the trees:

> **KSP never draws terrain scatter at the same height against the ground twice.** Load the same save
> several times, and every rock, tuft of grass or tree comes back drawn a little higher or a little
> lower against the ground each time — several centimetres apart on Kerbin.

With this mod, the scatter of each terrain quad is drawn with the same matrix as the ground of that
quad. Two Harmony patches, in one source file.

**How this was made.** Written with Claude, Anthropic's AI assistant, and reviewed line by line by a
human — me. I am saying so up front, because contributions made with an AI deserve a closer look than
others, and because some people would rather stop reading here. That look is what this page is built
for: every figure on it comes from an in-game measurement, the instrument behind them is public and runs
on a stock install, the stock code quoted here is a handful of lines anyone can check, and the fix fits
in one file you can read in a few minutes.

## Why it matters

It barely does. Stock scatter has no collider: nothing rests on it and nothing hits it, so a rock drawn a
few centimetres higher or lower than at the last load changes nothing for your craft. Scatter is also
sunk partly into the ground on purpose, so the shift is hard to see, and most of the time you will not
see it at all.

This fix exists for another reason. [Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix)
changes the height at which KSP builds the ground: it moves the quads, not the scatter drawn on them, and
the scatter ends up even further from the ground than in stock (see [Compatibility](#compatibility)).
This mod keeps the scatter on its ground, with or without Terrain Precision Fix.

## The problem

[Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag) shows it on a stock
install. For every object of the terrain quad nearest to a landed craft, it measures the height of the
object's lowest point above the ground right under it (the terrain collision surface, found by a ray
cast). Scatter objects are partly sunk into the ground by construction, so the value itself says
little; what matters is whether it stays the same from one load to the next.

It does not. Kerbin, next to the KSC, the same save loaded six times, on the same quad each time,
`Kerbin Zn3010000130` (218 objects: around the KSC, the scatter is grass and trees, 200 `Grass00` and
18 `Tree00`). Measured with an earlier version of that mod, whose columns the rows are named after:

| load | 1 | 2 | 3 | 4 | 5 | 6 |
|---|---|---|---|---|---|---|
| **Rocks**: objects above the ground, mean (mm) | −324.378 | −283.626 | −283.712 | −389.836 | −304.302 | −323.870 |
| **Matrix**: holder drawn off its transform position, vertical (mm) | 0.000 | +40.707 | +40.791 | −64.503 | +20.497 | 0.000 |
| **Rocks − Matrix** (mm) | −324.378 | −324.335 | −324.505 | −325.332 | −324.799 | −323.870 |

The objects move by 106 mm against the ground from one load to the next. Take off the **Matrix** term,
explained below, and what is left is constant to 1.5 mm on average. Object by object, the median range
over the loads is 106 mm, and 3.9 mm once **Matrix** is taken off. An earlier series of six loads on the
same quad gave a 119 mm range.

About fifty of the 218 objects, those whose lowest point is 0.5 to 1.8 m away from the ground, keep a
residue of a few centimetres from one load to the next once **Matrix** is taken off. It is not explained.

Over all 118 scatter holders of the 64 quads around the craft, on those six loads, the **Matrix** term
ranges from −85.5 to +64.7 mm, with a standard deviation of 29.7 mm. It changes from one holder to the
next and from one load to the next, and is made of whole single precision steps along the world axes,
projected on the vertical. For the quads, the same measurement is 0.000 mm everywhere.

Measured on stock KSP with Harmony and ModuleManager, without any fix installed.

## Why it happens

The objects of a terrain quad are built from the quad's own vertices, in the quad's own coordinates
(`PQSLandControl.LandClassScatter.CreateScatterMesh`):

```csharp
scatterPos = Vector3.Lerp(q.quad.verts[num3], q.quad.verts[num2], UnityEngine.Random.value);
```

They are written as they are into the mesh of a holder, a `PQSMod_LandClassScatterQuad` taken from a
pool. So they stand on the ground only if the holder is drawn with the same origin as the quad.

Both are placed from the same double precision vector, the origin of the quad relative to the centre of
the body, hundreds of kilometres long, stored in a single precision `localPosition` under the terrain
sphere, whose origin is the centre of the body:

```csharp
// PQ.SetupQuad
quadTransform.localPosition = positionPlanet;
// PQSMod_LandClassScatterQuad.Setup
base.transform.localPosition = quad.positionPlanet;
```

Then they part ways. The quads of the highest subdivision level, the only ones that carry scatter, are
moved to a container of their own, `sphere.LocalSpacePQStorage`, near the world origin, keeping the
world position Unity computed for them. The holder stays under the sphere, and Unity draws it with its
local to world matrix. For a local position this long, the translation of that matrix is not the
transform position: it is off by combinations of whole float steps (62.5 mm at 600 km), and the objects
are drawn that much above or below the ground.

The transform positions themselves agree: over six other loads of the same save, the holder and the
quad had the same `transform.position` to 0.000 mm on all 64 quads, and on Gilly, in one reading over 128
quads carrying scatter, to the bit. The **Matrix** row above is the whole difference.

## What the fix does

Each holder hangs from its own quad, at no offset, so that the objects are drawn in the frame they were
built in, with the very matrix the ground is drawn with. No transform in the chain holds a 600 km vector
any more.

- **After `PQSMod_LandClassScatterQuad.Setup`**, which gives a holder its quad: the holder is re-parented
  under the quad, with zero position, identity rotation and unit scale. From then on it follows the quad
  through everything stock does to it, floating origin shifts (`PQ.FastUpdateSubQuadsPosition`) and
  re-placements (`PQ.PreciseUpdateSubQuadsPosition`) included, with nothing more to do.
- **Before `PQSLandControl.LandClassScatter.DestroyQuad`**, which returns a holder to its pool when its
  quad is destroyed: the holder goes back under the pool's container, as stock placed it, before the
  quad itself goes back to the PQS cache to be reused elsewhere. A prefix, because the stock method
  starts by clearing the holder's quad.

Left exactly as stock places them:

- holders of quads that do not hang from `LocalSpacePQStorage` (in stock, scatter only exists on the
  quads that do);
- holders on a sphere whose quads are not surface relative, where stock places the holder at the centre
  of the body, not at a long vector;
- everything, if the patches fail to install.

Why moving the holder is safe, as read in the decompiled code: a destroyed quad calls its `onDestroy`
delegates, which release its holder, before it goes to the PQS cache; `PQS.ResetSphere` destroys the
quads before the scatter destroys its holders; the visibility of a holder is switched with
`obj.SetActive` from the quad's `onVisible` and `onInvisible` delegates, not through the hierarchy.

## Results

Not measured yet.

To measure it: [Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag), with
this mod installed, the same save loaded several times, one reading recorded per load. Expected, from the
code: every holder hanging under its own quad, and under each holder the height of its objects above the
ground the same on every load, as steady as **Rocks − Matrix** was in stock.

## Compatibility

- **Alone**: the scatter follows the stock ground, whatever that ground's own rounding.
- **With [Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix)**, which places
  the ground in double precision: both the ground and the scatter are placed exactly. Without this mod,
  Terrain Precision Fix widens the scatter offset (measured: −66.3 to +152.0 mm, standard deviation
  44.4 mm, same save and same 118 holders), because it moves the quads and not the holders. Neither mod
  depends on the other.
- **Kopernicus** replaces the stock holder with a subclass that inherits `Setup` without redeclaring
  it, and releases it through the same `DestroyQuad`, so it should be covered. It also replaces the
  pool's container at runtime; the mod reads it at every call. Not tested.

## What has not been checked

- Nothing about this mod has been measured in game yet (see [Results](#results)).
- Kopernicus: see above. Its optional scatter colliders (`scatterColliders`) are children of the holder,
  so they would follow it under the quad. Not tested.
- Parallax: its source does not reference the stock scatter holders. Not tested with it.
- Breaking Ground's surface features are placed the same way (`PQSMod_ROCScatterQuad.Setup`), and they
  have colliders. This mod does not handle them yet (see [TODO.md](TODO.md)).
- Flight at speed, the map view and scene changes, where quads are built and destroyed all the time.
- Performance: a handful of transform assignments each time a holder is given a quad or released,
  negligible in principle, not measured.

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
