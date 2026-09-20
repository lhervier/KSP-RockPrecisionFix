# Should you install it?

Part of [Rock Precision Fix](../README.md). The short answer is on the main page, under [Should you install it?](../README.md#should-you-install-it); here is the whole case.

Not on a stock install, where nothing it fixes is ever seen; with a mod that gives the scatter colliders,
it is yours to weigh. The [measurements](checking-the-culprit.md) show that the fix does what it says. Its
main drawback is not in what they measure: it moves stock objects, the scatter holders, away from where
stock puts them, and other mods may look for them there.

## What the fix changes for other mods

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

## What has been read

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

## What cannot be read

Every other mod: visual mods such as Scatterer or EVE, planet pack plugins, anything that walks the
terrain's hierarchy. Such a mod would miss the holders where stock puts them, or find unexpected children
under its quads. Nothing says one does. Nothing says none does, and that can only be checked one mod at a
time.

## Why the holder has to move at all

The fix that would leave every other mod alone is the one that keeps the holder under its `Scatter <name>`
container and merely places it better. Reading the stock code, and the way Unity stores a transform, that
way looks closed. Three reasons, from the plainest to the heaviest:

- **The correction is smaller than the step of the number that would carry it.** Unity keeps a child's
  position in its parent's frame, in single precision, whichever call writes it: `position`,
  `localPosition` and `SetPositionAndRotation` all end in that same float. Under the sphere, that number is
  the 600 km vector, where a float changes in steps of 62.5 mm on Kerbin, while the offsets to be taken out
  are the ones measured in [Checking the culprit](checking-the-culprit.md), up to 110 mm. No value that can be written puts the holder where it belongs.
- **The frame it hangs in is single precision as well.** Even given a perfect local position, the holder is
  drawn through the sphere's matrix, over that same 600 km. That product is what rounds differently at
  every load: it is the defect itself, not a way around it.
- **Out of the sphere, the holder has to be placed by hand before every frame.** Placing it in double
  precision needs a parent near the world origin, and stock then stops carrying it: the world position of a
  quad of the highest level is rewritten whenever the body moves in the game's local space, which in flight
  is every frame, and its rotation whenever stock places the quad again. The holder would have to follow
  both — its objects are built in the quad's frame, so its rotation counts as much as its position — and a
  frame missed would leave them not centimetres but metres behind the ground.

**This is read, not measured.** No build of this mod has tried that way, and nothing else on this page rests
on it: the [measurements](checking-the-culprit.md) stand on their own. It is written down because it is the first question to ask
of a fix that moves stock objects, and a reader who sees a way through should say so.

What is left is to take the holder out of the sphere, as this mod does, or to stop drawing the scatter from
the holder's transform altogether and draw it from somewhere else, which changes far more than where an
object hangs. Of the two, hanging the holder from its quad is the smaller change: the quad is the one object
stock already keeps in step, for nothing, with the ground the scatter is built from.

## The balance

On one side, on a stock install, a defect nobody sees: stock scatter has no collider, and is sunk into the
ground on purpose (see [Why the moving scatter matters](../README.md#why-the-moving-scatter-matters)). On
the other, a change to where stock objects hang, which any mod installed along with it may rely on. Its
cost weighs on neither side: none shows in the measurement (see [Performance](performance.md)).

What tips the first side is another mod. Give the scatter colliders — the
[Stock Scatter Collider Enabler Patch](https://github.com/Poodmund/Stock-Scatter-Collider-Enabler-Patch)
does, on top of Kopernicus, and it is on CKAN — and the defect stops being invisible: the rock a craft
hits stands up to 104 mm from the rock its pilot sees, and a kerbal left on a boulder sinks into it at one
load and stands clear of it at the next. Only this fix closes that gap; Terrain Precision Fix halves it
(see [Rock Precision Fix Diag, the colliders](checking-the-culprit.md#rock-precision-fix-diag-the-colliders)).
So the answer below is not the same for everyone: it holds for a stock install, and a player whose mods
give the scatter colliders is weighing something else.

And it is not a change that can be traded away: nothing above leaves a fix that keeps the holders where
stock puts them.

So, should you install it? On a stock install, no. Nobody installing it there would know that the holders
moved, and a mod tripping over it would fail with nothing to point at this one — a bad trade against a
defect that changes nothing in play. With a mod that gives the scatter colliders, the trade is yours to
make: the gap it closes is then a real one, between the rock your craft hits and the rock you see, and no
other fix closes it.

Either way, this mod stays what it is: the measured answer to what Terrain Precision Fix leaves behind,
and the proof that the culprit is the right one.

## If it went into KSP Community Fixes

Then the re-parenting stops being a technical risk and becomes a matter of agreement between modders. A
KSP Community Fixes patch is documented, and can be turned off by a line of its `Settings.cfg`: the move
would be known, and a player hit by it could undo it without removing anything else.

But it would still be a change that other mods have to follow. A mod that finds holders through the
hierarchy would have to change its code: search under the terrain quads as well, as Rock Precision Fix
Diag does, or read the holders from their scatter's pool (`LandClassScatter.cacheAssigned`, a private
list), which does not depend on where they hang. Whether that is worth asking of other modders is not a
technical question, and this page does not answer it. What can be said is what is being asked for: not
only a defect nobody sees, but also the one players already meet when their mods give the scatter
colliders, where the rock hit is not the rock seen.
