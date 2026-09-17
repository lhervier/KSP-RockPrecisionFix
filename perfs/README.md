# Measurement runs

Readings of [Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag) with this
mod installed, kept as they were logged: one file per load, each holding the last record taken after that
load, copied out of `KSP.log`.

The procedure, the saves and the format of a record belong to that mod:
[its protocol](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/README.md#the-protocol),
[the saves](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/perfs/README.md#the-saves) and
[the log](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/README.md#the-log).

## With Terrain Precision Fix

KSP 1.12.5 on Windows. `GameData` holding Harmony, ModuleManager, KSP Community Fixes 1.41.1,
[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix) 0.1.0, Rock Precision Fix
Diag and this mod. Both fixes at `logLevel = Info`. Terrain scatter on, the log flushed at once. KSP was
started once, and in that session `reference-kerbin.sfs` then `reference-mune.sfs` were each loaded twelve
times, with `Alt+F6` pressed after each load once the scene had settled.

Both fixes wrote to `KSP.log` that they were installed, and that they had acted on each body before its
first record:

```
[RockPrecisionFix] Version 0.1.0.0 installed, log level Info
[TerrainPrecisionFix] Version 0.1.0.0 installed, log level Info
[TerrainPrecisionFix] Kerbin: terrain placed in double precision (first quad corrected by 16.50 mm)
[RockPrecisionFix] Kerbin: scatter drawn from its terrain quads (first holder was -27.51 mm off)
[TerrainPrecisionFix] Mun: terrain placed in double precision (first quad corrected by 9.18 mm)
[RockPrecisionFix] Mun: scatter drawn from its terrain quads (first holder was +8.51 mm off)
```

| load | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Kerbin | [`1`](runs/kerbin-both-load1.log) | [`2`](runs/kerbin-both-load2.log) | [`3`](runs/kerbin-both-load3.log) | [`4`](runs/kerbin-both-load4.log) | [`5`](runs/kerbin-both-load5.log) | [`6`](runs/kerbin-both-load6.log) | [`7`](runs/kerbin-both-load7.log) | [`8`](runs/kerbin-both-load8.log) | [`9`](runs/kerbin-both-load9.log) | [`10`](runs/kerbin-both-load10.log) | [`11`](runs/kerbin-both-load11.log) | [`12`](runs/kerbin-both-load12.log) |
| the Mun | [`1`](runs/mun-both-load1.log) | [`2`](runs/mun-both-load2.log) | [`3`](runs/mun-both-load3.log) | [`4`](runs/mun-both-load4.log) | [`5`](runs/mun-both-load5.log) | [`6`](runs/mun-both-load6.log) | [`7`](runs/mun-both-load7.log) | [`8`](runs/mun-both-load8.log) | [`9`](runs/mun-both-load9.log) | [`10`](runs/mun-both-load10.log) | [`11`](runs/mun-both-load11.log) | [`12`](runs/mun-both-load12.log) |

Every record of Kerbin ends on the same line, but for its number:

```
End of record 1: 64 quads with rocks, 118 holders (0 not built yet); nearest quad 'Kerbin Zn3010000130': 218 rocks, 1780 vertices measured, 0 without ground under them
```

and every record of the Mun on this one:

```
End of record 13: 128 quads with rocks, 128 holders (0 not built yet); nearest quad 'Mun Zp211333000': 20 rocks, 200 vertices measured, 0 without ground under them
```

So every record was taken once all the holders were built, and all the records of a body name the same
nearest quad: its objects and their measured vertices compare one by one from one load to the next.

## The other configurations

Stock and Terrain Precision Fix alone, on the same saves, are kept with
[Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/perfs/README.md).
