# Performance

Part of [Rock Precision Fix](../README.md). The short version is on the main page, under [Performance](../README.md#performance); here are the protocol and the figures.

What this mod adds runs once each time a holder is given a quad or released, not once per vertex: a
re-parenting and three transform assignments. Both happen inside what
[PQS Bench](https://github.com/lhervier/KSP-PQSBench) times in flight: a holder is given its quad within
`PQS.BuildQuad`, when the `PQSMod`s are told the quad is built, and handed back within `PQS.UpdateQuads`,
when a quad collapses. **Its page carries the procedure.**

Four runs of its `counters` mode, from the save it provides: a command pod on rails 5 km over the Mun,
two and a half minutes each, which build the same 1 272 quads of the highest subdivision level every
time. Two with Terrain Precision Fix alone, two with this mod added, alternated. Taken on my desktop PC,
described with the logs in [`perfs/`](../perfs/README.md); figures from another machine are not comparable
to these.

| | Terrain Precision Fix alone | with this mod |
|---|---|---|
| a quad of the highest level | 2.809 and 2.743 ms | 2.727 and 2.752 ms |
| terrain share of real time | 13.96 and 13.93 % | 13.84 and 13.69 % |
| frames per second | 111.5 and 112.9 | 115.1 and 112.6 |

**No cost shows.** Two runs of the same configuration differ by up to 66 µs per quad of the highest level,
2.4 %. The runs with this mod fall within that spread, and on the cheaper side of it. Whatever the fix
costs is smaller than this measurement can resolve, which is all `counters` claims to tell (see
[What it cannot tell](https://github.com/lhervier/KSP-PQSBench#what-it-cannot-tell)).
