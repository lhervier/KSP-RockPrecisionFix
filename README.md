# Rock Precision Fix

A fix for stock KSP 1.12, written to go with
[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix) and, like it, kept as
small as possible: two Harmony patches, in one source file. Here is what they fix, for the terrain
scatter drawn around your craft — the rocks, and around the KSC the grass and the trees:

> **KSP never draws terrain scatter at the same height against the ground twice.** Load the same save
> several times, and every rock, tuft of grass or tree comes back drawn a little higher or a little
> lower against the ground each time — several centimetres apart on Kerbin.

The fix works, and this page measures it. It is still **not worth installing as a mod of its own**: to
correct a defect nobody sees, it moves stock objects to another place in the scene, where other mods may
expect to find them. That move is its main drawback. Were the fix part of KSP Community Fixes, it would
no longer be a technical risk but a question for modders to settle: whether mods that look for those
objects should change their code. See [Should you install it?](#should-you-install-it)

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
scatter no longer follows the ground it is drawn on: in stock it sometimes comes back exactly on its quad,
with Terrain Precision Fix never. Nobody sees that either. But a fix that leaves something behind, even
something invisible, has to answer for it, and this mod is that answer: with both installed, the ground and
the scatter on it come back at the same place at every load. Whether that answer is worth installing is
another question, answered in [Should you install it?](#should-you-install-it)

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

One value in that method does use the long vector: the local vertical of each object, along which the
object is then sunk into the ground and around which it is turned. It is the direction from the centre of
the body to the object:

```csharp
scatterUp = (scatterPos + q.quad.positionPlanet).normalized;
scatterPos += scatterUp * verticalOffset;
```

That line needs no patch, and this is measured, not only read. An object sunk or turned along a vertical
that changed from one load to the next would come back leaning another way, its vertices higher or lower
against each other. On every series of [Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/README.md#what-the-readings-show),
on Kerbin and on the Mun, on stock and with Terrain Precision Fix, the heights of the measured vertices of
each object against each other come back the same at every load to within 0.07 mm, trees 20 m tall
included.

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
height above the ground right under them of up to 10 of its vertices, spread over the whole object.
Scatter is sunk into the ground on purpose, so that last height says little by itself: what matters is
whether it comes back the same at every load.

Every series below uses [the two saves it keeps](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/perfs/README.md#the-saves), each loaded twelve
times, in KSP 1.12.5 with Harmony, ModuleManager and KSP Community Fixes 1.41.1:

- a Mk1 command pod landed on Kerbin, about 8 km north-west of the KSC, where the scatter is grass and
  trees: every record holds the same 64 quads and 118 holders, all of them built, and names the same
  nearest quad, `Kerbin Zn3010000130`, with the same 218 objects, 200 `Grass00` and 18 `Tree00`, and 1,780
  measured vertices;
- a Mk1 command pod landed on the Mun, where the scatter is rocks: 128 quads and 128 holders, and the same
  nearest quad, `Mun Zp211333000`, with 20 `Rock00` and 200 measured vertices.

The *range* of a reading is its largest value minus its smallest over the twelve loads.

### Rock Precision Fix Diag, on stock

The stock series and the one with Terrain Precision Fix alone are kept, with their logs, on
[the instrument's page](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/README.md#what-the-readings-show).
On stock:

- the centre of each holder stands at the height of its quad's centre, to the micrometre;
- the matrix each quad is drawn with stands exactly on its centre: 0.000 mm everywhere, up and across;
- the matrix each holder is drawn with does not: it stands above or below its quad's centre by an amount
  that changes from one holder to the next and from one load to the next, from −92 to +92 mm on Kerbin,
  0.000 mm only 28% of the time; on the Mun, within 0.1 mm of 0 three times out of four, otherwise
  ±15.2 to ±15.4 mm, nothing in between;
- the vertices of the objects of the nearest quad come back at a different height above the ground at
  every load: 94 mm apart over the twelve loads for half of them on Kerbin, 31 mm on the Mun;
- take their holder's offset off, and that range drops to 5.3 mm for half of them on Kerbin, 2.2 mm on
  the Mun.

The holders stand where their quads are, but are not drawn there, and most of what moves the objects
against the ground is that gap. That is the culprit.

Not all of it: once their holder's offset is taken off, 80 objects out of 218 keep more than 10 mm on
Kerbin, up to 119 mm, and 3 rocks out of 20 on the Mun, up to 18.5 mm. In stock the ground itself moves
against the centre of its quad from one load to the next, by 104 mm under half of the vertices on Kerbin
and 27 mm on the Mun: that is the defect Terrain Precision Fix corrects, and it is why this fix is
measured with it.

### Rock Precision Fix Diag, with Terrain Precision Fix alone

With Terrain Precision Fix installed, the quads stop moving: the centre of the nearest quad comes back at
the same height to 0.002 mm on Kerbin, against 131 mm in stock, and to 0.005 mm on the Mun, against
33 mm. The holders do not follow them:

- their centres no longer stand on their quads': 148 mm apart over the twelve loads for half of the
  holders on Kerbin, up to 227 mm, and 25 mm on the Mun, up to 42 mm;
- the offset of their matrices spans −107 to +110 mm on Kerbin and −27 to +29 mm on the Mun, and is never
  0;
- the vertices of the objects of the nearest quad come back at a different height above the ground at
  every load: 130 mm apart for half of them on Kerbin, 31 mm on the Mun;
- take their holder's offset off, and no vertex moves by more than 6.6 mm on Kerbin, 1.9 mm on the Mun.

The ground is fixed, the holders are not, and now all of what moves the objects is the holders.

### Rock Precision Fix Diag, with this mod

The same install and the same two saves, with this mod added next to Terrain Precision Fix, each save
loaded twelve times in a single session of KSP, one record taken after each load. The install and the 24
records are under [perfs](perfs/README.md).

**Kerbin**

| | with Terrain Precision Fix alone | with both fixes |
|---|---|---|
| centre of the nearest quad, range | 0.002 mm | 0.002 mm |
| matrix of each quad against its centre | *up* and *across* 0.000 mm everywhere | the same |
| centre of each holder against its quad's | range 148 mm (median over the holders), up to 227 mm | the same height, to the micrometre |
| *up* of the holders' matrices | −107 to +110 mm, never 0 | 0.000 mm everywhere, and *across* as well |
| each vertex above the ground, range | 130 mm (median over the vertices), from 127 to 134 mm | 0.035 mm (median), 0.125 mm at most |

**The Mun**

| | with Terrain Precision Fix alone | with both fixes |
|---|---|---|
| centre of the nearest quad, range | 0.005 mm | 0.004 mm |
| matrix of each quad against its centre | *up* and *across* 0.000 mm everywhere | the same |
| centre of each holder against its quad's | range 25 mm (median over the holders), up to 42 mm | the same height, to the micrometre |
| *up* of the holders' matrices | −27 to +29 mm, never 0 | 0.000 mm everywhere, and *across* as well |
| each vertex above the ground, range | 31 mm (median over the vertices), from 30 to 32 mm | 0.012 mm (median), 0.042 mm at most |

**The holders.** On all 118 holders of Kerbin and 128 of the Mun, at every one of the twelve loads, the
centre of the holder and the matrix it is drawn with stand at the height of its quad's centre, to the
micrometre, and neither is shifted from it, up or across. The holders are drawn exactly where their quads
are.

**The objects.** Every measured vertex of the nearest quad comes back at the same height against the
ground at every load: within 0.035 mm for half of them on Kerbin, 0.125 mm at most, and within 0.042 mm on
the Mun. On average, the lowest vertex of each object's model stands 316.8 mm below the ground on Kerbin
and 1,979.8 mm on the Mun, the same at each of the twelve loads.

**Nothing else moves.** In the records taken with Terrain Precision Fix alone, a vertex's height above the
ground minus its holder's *up* is where that vertex would stand without the holder's offset. Averaged over
the twelve loads of each series, vertex by vertex, that height and the one measured here agree to 0.018 mm
(median), 1.2 mm at most, on Kerbin, and to 0.080 mm (median), 0.30 mm at most, on the Mun: the fix removes
the holders' offset, and nothing else about where the objects are drawn.

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

### Why moving the holder is safe for stock

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

## Should you install it?

Not as a mod of its own. The measurements above show that the fix does what it says. Its main drawback is
not in what they measure: it moves stock objects, the scatter holders, away from where stock puts them,
and other mods may look for them there.

### What the fix changes for other mods

One thing: where a holder hangs. Its pool (`cacheAssigned`, `cacheUnassigned`), its `quad`, its delegates
and the way it is shown and hidden stay stock. But a holder in use no longer hangs from the
`Scatter <name>` container under the sphere: it hangs from its quad, under `LocalSpacePQStorage`, outside
the body's hierarchy. Code that finds holders through the hierarchy sees the difference both ways:

- looking under the sphere, `sphere.GetComponentsInChildren<PQSMod_LandClassScatterQuad>()` no longer
  finds the holders in use;
- looking under a quad, it finds children that stock never puts there.

This is not only a hypothesis: [Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag)
finds the holders through the hierarchy, and has to search both places to see them with this mod
installed.

### What has been read

- **Stock** relies on neither. The pool is kept in the lists above, not in the hierarchy. A holder is shown
  and hidden with `obj.SetActive`, a quad with `meshRenderer.enabled`, so the holder does not inherit
  anything from the quad's object. When a collision or a raycast hits the ground, stock asks the object hit
  whether it is a quad (`GetComponent<PQ>()` in `Part`, `ModuleWheelDamage`, `ModuleDeployableSolarPanel`,
  `ModuleGroundSciencePart`), not its parents.
- **Kopernicus**, the only mod read here that uses the holders, reaches them through the scatter
  (`scatterParent`, which this mod reads at every call, and the pool), and reaches the objects of a holder
  through the holder's own children, which follow it. Its lethal and heat emitting scatter computes world
  positions from the holder's matrix and positions in the quad's frame, which this fix makes exact. Its
  optional scatter colliders end up below the quad; stock's `GetComponent<PQ>()` still finds no quad on
  them, but that is not measured.
- **Parallax** has its own scatter system, keyed by quad, and does not touch the stock holders.
- **KSP Community Fixes** does not touch the scatter; `OptimizedModuleRaycasts` asks the object hit whether
  it is a quad, as stock does.
- **TUFX** (1.1.1) works on the camera's image, not on the scene: it has no Harmony patch and never looks
  for a terrain object. Its only walk through the scene lists the objects of the main menu into its debug
  log.

### What cannot be read

Every other mod: visual mods such as Scatterer or EVE, planet pack plugins, anything that walks the
terrain's hierarchy. Such a mod would miss the holders where stock puts them, or find unexpected children
under its quads. Nothing says one does. Nothing says none does, and that can only be checked one mod at a
time.

### The balance

On one side, a defect nobody sees: stock scatter has no collider, and is sunk into the ground on purpose
(see [Why the moving scatter matters](#why-the-moving-scatter-matters)). On the other, a change to where
stock objects hang, which any mod installed along with it may rely on.

And there is no way around that change. As long as a holder hangs under the sphere, a transform in its
chain holds a vector hundreds of kilometres long (see [Two ways out, one taken](#two-ways-out-one-taken)):
any fix takes the holder out of the sphere, or draws the scatter without the holder's transform, which
would change far more.

As a mod of its own, that is not worth it. Nobody installing it would know that the holders moved, and a
mod tripping over it would fail with nothing to point at this one. So this mod stays what it is: the
measured answer to what Terrain Precision Fix leaves behind, and the proof that the culprit is the right
one.

### If it went into KSP Community Fixes

Then the re-parenting stops being a technical risk and becomes a matter of agreement between modders. A
KSP Community Fixes patch is documented, and can be turned off by a line of its `Settings.cfg`: the move
would be known, and a player hit by it could undo it without removing anything else.

But it would still be a change that other mods have to follow. A mod that finds holders through the
hierarchy would have to change its code: search under the terrain quads as well, as Rock Precision Fix
Diag does, or read the holders from their scatter's pool (`LandClassScatter.cacheAssigned`, a private
list), which does not depend on where they hang. Whether an invisible defect is worth asking that of other
modders is not a technical question, and this page does not answer it.

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

### Mods that look for the holders

**Limit.** A mod that finds scatter holders, or the children of a terrain quad, through the scene
hierarchy sees them elsewhere than stock puts them. Only stock, Kopernicus, Parallax, KSP Community Fixes
and TUFX have been read (see [Should you install it?](#should-you-install-it)).

**Solution.** None in this mod: moving the holder is the fix. Do not install it on its own. In KSP
Community Fixes, it would be for the mods concerned to adapt (see
[If it went into KSP Community Fixes](#if-it-went-into-ksp-community-fixes)).

### Surface features with colliders

**Limit.** Breaking Ground's surface features are placed the same way (`PQSMod_ROCScatterQuad.Setup`
does the same `localPosition = quad.positionPlanet`), and they have colliders, so their offset may be
physical, not only visual. This mod does not handle them.

**Solution.** Not there yet. The same patch should apply: hang the holder from its quad after `Setup`,
hand it back before `LandClassROC.DestroyQuad`. First measure where the physics puts those colliders
(see [TODO.md](TODO.md)).

### Not checked yet

- **Kopernicus.** It replaces the stock holder with a subclass that inherits `Setup` without redeclaring
  it, and releases it through the same `DestroyQuad`, so it should be covered. What its code does with
  the holders is in [What has been read](#what-has-been-read). *Solution:* measure it with Rock Precision
  Fix Diag (see [TODO.md](TODO.md)).
- **Parallax.** See [What has been read](#what-has-been-read). *Solution:* check it in game.
- **Scene switches, the map view, time warp, a rover driven across floating origin shifts, a trip to
  orbit and back**, where quads are built and destroyed, and holders sent through the pool. Only loading a
  save is measured. *Solution:* one record per step with Rock Precision Fix Diag, and the `Trace` log to
  check that as many holders go back to their pool as are hung from a quad (see [TODO.md](TODO.md)).
- **Performance.** See [Performance](#performance). *Solution:* measure it.

## Install

Read [Should you install it?](#should-you-install-it) first.

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
