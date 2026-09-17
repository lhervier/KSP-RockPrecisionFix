# What this fix costs: the runs

The logs this mod's performance figures are read from. What they say is in
[Performance](../README.md#performance) on the main page.

Measured with [PQS Bench](https://github.com/lhervier/KSP-PQSBench) in its `counters` mode, **whose page
carries the procedure** — the save, the orbit, how long to fly, and what makes a run worth keeping. Its
other mode, `calibrate`, only replays the vertex placement, which this mod does not touch.

## The runs

KSP 1.12.5. `GameData` holding Harmony, ModuleManager, KSP Community Fixes 1.41.1, PQS Bench and
Terrain Precision Fix 0.1.0 in every run, and this mod, 0.1.0, in two of them. A command pod on rails in
a circular orbit 5 km over the Mun, from [the save PQS Bench provides](https://github.com/lhervier/KSP-PQSBench/blob/master/perfs/ref-mune-5km.sfs),
150 seconds of game time. The runs alternate between the two configurations.

### The machine

| | |
|---|---|
| processor | Intel Core i7-4790K, 8 logical cores |
| memory | 32 GB of DDR3 |
| graphics | NVIDIA GeForce GTX 1060 6 GB |
| system | Windows 10, KSP in a 1280×720 window, vertical sync off, terrain scatter on at full density |

Not the machine of the other runs taken with PQS Bench, stock's and Terrain Precision Fix's own: figures
from those pages are not comparable to these.

### The logs

| log | installed |
|---|---|
| [`mun-05km-tpf-counters-1.log`](runs/mun-05km-tpf-counters-1.log) | Terrain Precision Fix alone |
| [`mun-05km-both-counters-1.log`](runs/mun-05km-both-counters-1.log) | Terrain Precision Fix and this mod |
| [`mun-05km-tpf-counters-2.log`](runs/mun-05km-tpf-counters-2.log) | Terrain Precision Fix alone |
| [`mun-05km-both-counters-2.log`](runs/mun-05km-both-counters-2.log) | Terrain Precision Fix and this mod |

**The `BENCH begin` line does not tell the two configurations apart.** It names the mods patching
`PQS.BuildVertexSurfaceRelative` and `PQS.BuildQuad`, and this mod patches neither: it patches methods
those two call. What tells them apart is this mod's own lines in the same log, written at startup and
when it first acts on a body: `[RockPrecisionFix] Version 0.1.0.0 installed` and
`[RockPrecisionFix] Mun: scatter drawn from its terrain quads`, in both `both` logs and in neither `tpf`
log.

The `BENCH run` lines say the four runs are the same flight: over the Mun from UT 54.68 to 54.76 at
5 000 m, for 149.86 to 149.96 seconds of game time against as much real time — no warp. Each run recorded
148 samples and built **1 272 quads of the highest subdivision level**, 3 211 or 3 213 quads in all.

## The figures

Each figure is a total over the 148 samples of a run: frames over real seconds, the build time of the
quads of the highest level over their number, terrain update time over real seconds.

| log | frames per second | a quad of the highest level | terrain share of real time |
|---|---|---|---|
| `tpf-counters-1` | 111.48 | 2.809 ms | 13.96 % |
| `tpf-counters-2` | 112.87 | 2.743 ms | 13.93 % |
| `both-counters-1` | 115.10 | 2.727 ms | 13.84 % |
| `both-counters-2` | 112.60 | 2.752 ms | 13.69 % |
