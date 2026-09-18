# Measurement runs

Readings of [Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag) with this
mod installed, kept as they were logged, copied out of `KSP.log`: for the rocks, one file per load, each
holding the last record taken after that load; for the holder pools, one file per flight or session,
holding every record taken during it.

The procedure, the saves and the format of a record belong to that mod:
[its protocol](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/docs/measuring-the-rocks.md#the-protocol),
[the saves](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/diag/README.md#the-saves) and
[the log](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/docs/measuring-the-rocks.md#the-log).

## The rocks, over twelve loads

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

## The holder pools, over a flight

This mod takes each holder out of the pool's container while its quad is in use, and hangs it back there
when the quad is handed back. This series checks that every holder does go back, and that none is lost on
the way.

The same install as above, with a later build of Rock Precision Fix Diag: the first one
with the holder record. `ref-mune-5km.sfs` loaded once, then `Alt+Shift+F6` pressed 30 s into the flight,
again about every two minutes, and once more after the pod crashed, following
[its protocol](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/docs/checking-the-holder-pools.md#over-a-flight).

Both fixes wrote to `KSP.log` that they had acted on the Mun before the first record:

```
[TerrainPrecisionFix] Mun: terrain placed in double precision (first quad corrected by 14.82 mm)
[RockPrecisionFix] Mun: scatter drawn from its terrain quads (first holder was -1.34 mm off)
```

| flight | records |
|---|---|
| with both fixes | [`mun-5km-both-holders.log`](runs/mun-5km-both-holders.log) |

The file holds the six records and the line of `KSP.log` reporting the crash, between the fifth and the
sixth. Every record ends on `0 broken rules`:

| record | holders in use | free | broken rules |
|---|---|---|---|
| 1 | 344 | 40 | 0 |
| 2 | 168 | 216 | 0 |
| 3 | 144 | 240 | 0 |
| 4 | 224 | 160 | 0 |
| 5 | 152 | 232 | 0 |
| 6, after the crash | 568 | 40 | 0 |

Those counts are, record for record, those of the flight with Terrain Precision Fix alone: the flight
takes as many holders out of the pool, at the same moments, with this mod as without it.

## The holder pools, across scene switches

This mod hangs a holder from its quad, and the quads of the most detailed level are shared by every body.
Leaving a body switches its terrain off: this series checks that its holders go back to their pools
before stock destroys them, and that none travels on a quad to the next body.

The same install and build as the flight above. One session, following
[its protocol](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/docs/checking-the-holder-pools.md#across-scene-switches):
`reference-mune.sfs` loaded from the Space Center, back to the Space Center, then `reference-kerbin.sfs`,
with `Alt+Shift+F6` pressed in each of the three scenes.

Both fixes wrote to `KSP.log` that they had acted on each body before its first record:

```
[TerrainPrecisionFix] Kerbin: terrain placed in double precision (first quad corrected by 16.50 mm)
[RockPrecisionFix] Kerbin: scatter drawn from its terrain quads (first holder was -27.51 mm off)
[TerrainPrecisionFix] Mun: terrain placed in double precision (first quad corrected by 1.68 mm)
[RockPrecisionFix] Mun: scatter drawn from its terrain quads (first holder was -0.40 mm off)
```

| session | records |
|---|---|
| with both fixes | [`scenes-both-holders.log`](runs/scenes-both-holders.log) |

The file holds the three records, the line of `KSP.log` marking the arrival in each scene, and those of
both fixes. Every record ends on `0 in no pool` and `0 broken rules`:

| record | scene | pools | holders in use | free |
|---|---|---|---|---|
| 1 | the Mun | the Mun's `Rock00` | 128 | 32 |
| 2 | the Space Center | Kerbin's `Tree00`, `Grass00`, `boulder`, `Pine00`, `cactus` | 4 | 316 |
| 3 | Kerbin | the same five | 118 | 202 |

Those are, pool for pool, the counts of the same session with Terrain Precision Fix alone. The Mun's pool
is gone from the second record on, and no holder of the Mun turned up under a quad of Kerbin.

## The other configurations

Stock and Terrain Precision Fix alone, on the same saves, are kept with
[Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/diag/README.md).
