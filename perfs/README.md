# Measurement runs

Readings of [Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag) with this
mod installed, kept as they were logged: one file per load of the same save, each holding the last record
taken after that load, copied out of `KSP.log`.

The procedure, the save and the format of a record belong to that mod:
[its protocol](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/README.md#the-protocol),
[the save](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/perfs/README.md#the-save) and
[the log](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/README.md#the-log).

## With Terrain Precision Fix

KSP 1.12.5 on Windows. `GameData` holding Harmony, ModuleManager, KSP Community Fixes 1.41.1,
[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix) built from commit `34aa73c`,
Rock Precision Fix Diag built from commit `eb8276c`, and this mod built from commit `e4368b8`. Both fixes
at `logLevel = Info`. Terrain scatter on, the log flushed at once. KSP was started once, and the save
loaded six times in that session, with `Alt+F6` pressed after each load once the scene had settled.

Before the first load, both fixes wrote to `KSP.log` that they were installed and had acted on Kerbin:

```
[RockPrecisionFix] Version 0.1.0.0 installed, log level Info
[TerrainPrecisionFix] Version 0.1.0.0 installed, log level Info
[TerrainPrecisionFix] Kerbin: terrain placed in double precision (first quad corrected by 11.32 mm)
[RockPrecisionFix] Kerbin: scatter drawn from its terrain quads (first holder was +19.08 mm off)
```

| load | 1 | 2 | 3 | 4 | 5 | 6 |
|---|---|---|---|---|---|---|
| log | [`load1`](runs/kerbin-both-load1.log) | [`load2`](runs/kerbin-both-load2.log) | [`load3`](runs/kerbin-both-load3.log) | [`load4`](runs/kerbin-both-load4.log) | [`load5`](runs/kerbin-both-load5.log) | [`load6`](runs/kerbin-both-load6.log) |

Every record ends on the same line, but for its number:

```
End of record 1: 64 quads with rocks, 118 holders (0 not built yet); nearest quad 'Kerbin Zn3010000130': 218 rocks, 0 without ground under them
```

So every record was taken once all the holders were built, and all of them name the same nearest quad:
its 218 objects, 200 `Grass00` and 18 `Tree00`, compare one by one from one load to the next.

## The other configurations

Stock, on the same save and the same day, is kept with
[Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/perfs/README.md).
