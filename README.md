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

Stock places each terrain quad and its scatter *holder* from the same 600 km vector, stored in a float,
where a step is 62.5 mm. The quad is drawn from its position, the holder from its matrix, and the two can
round to different steps. Which ones is a draw that changes at every load, so the scatter comes back
higher or lower against the ground each time.

**→ Full chapter: [The culprit](docs/the-culprit.md)**

## Checking the culprit

[Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag) loads the same save twelve
times on Kerbin and on the Mun. With Terrain Precision Fix alone, the ground stops moving but the scatter
does not: half of the measured vertices come back 130 mm apart on Kerbin. With both fixes, every holder is
drawn exactly on its quad, and no vertex moves by more than 0.125 mm. Over a whole flight low over the Mun,
every holder stays drawn on its quad, and every holder goes back to its pool.

**→ Full chapter: [Checking the culprit](docs/checking-the-culprit.md)**

## The fix this mod proposes

Two Harmony patches hang each holder from its own quad, at no offset, and hand it back to its pool when
the quad goes. The stock scatter is then drawn with the very matrix of the ground it was built on.

**→ Full chapter: [The fix this mod proposes](docs/the-fix-this-mod-proposes.md)**

## Should you install it?

Not as a mod of its own. The fix moves stock objects, the scatter holders, and a mod that looks for them
where stock puts them would miss them, against a defect nobody sees. Inside KSP Community Fixes, it would
become a question for modders: whether such mods should adapt.

**→ Full chapter: [Should you install it?](docs/should-you-install-it.md)**

## Performance

Measured in flight with [PQS Bench](https://github.com/lhervier/KSP-PQSBench), with and without this mod:
no cost shows. The difference is smaller than between two runs of the same configuration.

**→ Full chapter: [Performance](docs/performance.md)**

## Limits and solutions

Not measured without Terrain Precision Fix; mods that look for the holders. What is still to check
(time warp, Kopernicus, Parallax) is in [TODO.md](TODO.md).

**→ Full chapter: [Limits and solutions](docs/limits-and-solutions.md)**

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
