# Checking the culprit

Part of [Rock Precision Fix](../README.md): the measurements that check [the culprit](the-culprit.md), before and after the fix.

Before anything is changed, [Rock Precision Fix Diag](https://github.com/lhervier/KSP-RockPrecisionFixDiag)
measures what stock does. Its page carries its method and
[its protocol](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/docs/measuring-the-rocks.md#load-after-load).
For every quad carrying scatter around a landed craft, it reads the height of the quad, of each of its
holders, and of the matrices they are drawn with; for every object of the quad nearest to the craft, the
height above the ground right under them of up to 10 of its vertices, spread over the whole object.
Scatter is sunk into the ground on purpose, so that last height says little by itself: what matters is
whether it comes back the same at every load.

A second reading of the instrument,
[the holder pools](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/docs/checking-the-holder-pools.md),
checks something else, over a flight: that every holder taken out of its pool for a quad goes back to it
when the quad is destroyed. It is at the end of this page, after the rocks read along a flight.

Every series of loads below uses [the two saves it keeps](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/diag/README.md#the-saves), each loaded twelve
times, in KSP 1.12.5 with Harmony, ModuleManager and KSP Community Fixes 1.41.1:

- a Mk1 command pod landed on Kerbin, about 8 km north-west of the KSC, where the scatter is grass and
  trees: every record holds the same 64 quads and 118 holders, all of them built, and names the same
  nearest quad, `Kerbin Zn3010000130`, with the same 218 objects, 200 `Grass00` and 18 `Tree00`, and 1,780
  measured vertices;
- a Mk1 command pod landed on the Mun, where the scatter is rocks: 128 quads and 128 holders, and the same
  nearest quad, `Mun Zp211333000`, with 20 `Rock00` and 200 measured vertices.

The *range* of a reading is its largest value minus its smallest over the twelve loads.

## Rock Precision Fix Diag, on stock

The stock series and the one with Terrain Precision Fix alone are kept, with their logs, on
[the instrument's page](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/docs/what-the-readings-show.md#the-rocks).
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

## Rock Precision Fix Diag, with Terrain Precision Fix alone

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

## Rock Precision Fix Diag, with this mod

The same install and the same two saves, with this mod added next to Terrain Precision Fix, each save
loaded twelve times in a single session of KSP, one record taken after each load. The install and the 24
records are under [diag](../diag/README.md).

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

## Rock Precision Fix Diag, the rocks over a flight

The loads above read the scatter right after it is built, around a craft that does not move. In flight,
the world origin follows the craft, and the terrain keeps building quads ahead of it and destroying those
behind it: [the fix](the-fix-this-mod-proposes.md) leaves each holder to follow its quad through all of
that, without doing anything more.

The instrument reads the rocks along a flight, following
[its protocol](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/docs/measuring-the-rocks.md#over-a-flight):
the save
[`ref-mune-5km.sfs`](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/diag/README.md#the-saves),
a Mk1 command pod in a circular equatorial orbit 5 km over the Mun, loaded once, one record 30 s into the
flight, then every two minutes, and one after the pod crashed into the relief. The same install as above,
once with Terrain Precision Fix alone and once with both fixes. The records are under
[diag](../diag/README.md#the-rocks-over-a-flight), and those with Terrain Precision Fix alone with
[the instrument](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/diag/README.md#the-rocks-over-a-flight).

Flown the same way, both flights passed over the same ground: every record names the same quads as its
counterpart in the other flight, 1,600 in all, and the same nearest quad, whose 20 rocks and 200 vertices
compare one by one. The pod is 5 km up, so every quad is at least that far from the world origin, where
single precision coordinates step by about half a millimetre: the same quads' centres, and the ground under
the same vertices, come out up to 0.7 mm and 1.6 mm apart from one flight to the other.

| record | nearest quad | *up* of its holder, Terrain Precision Fix alone | its vertices above the ground, Terrain Precision Fix alone minus both fixes | *up* of all the holders, both fixes |
|---|---|---|---|---|
| 1 | `Mun Zp200000011` | +0.8 mm | +0.5 to +1.0 mm | 0.000 mm, 344 holders |
| 2 | `Mun Xn231111111` | +17.5 mm | +17.3 to +18.1 mm | 0.000 mm, 168 holders |
| 3 | `Mun Xn211311311` | −3.5 mm | −3.5 to −2.0 mm | 0.000 mm, 144 holders |
| 4 | `Mun Xn122020000` | +1.5 mm | +1.2 to +1.7 mm | 0.000 mm, 224 holders |
| 5 | `Mun Xn013331113` | −1.5 mm | −1.9 to −0.5 mm | 0.000 mm, 152 holders |
| 6, after the crash | `Mun Zn200000011` | −15.8 mm | −16.9 to −16.3 mm | 0.000 mm, 568 holders |

**With Terrain Precision Fix alone**, no holder is drawn on its quad, at any record: over the 1,600, the
*up* of their matrices runs from −26.5 to +21.1 mm, never 0, 9.0 mm on average (root mean square), about
what the twelve loads of the Mun gave. The offset does not grow along the flight, but it does not go away
either.

**With both fixes**, at every record, every holder, 1,600 in all, stands at the height of its quad's
centre, to the micrometre, and its matrix too, neither shifted from it, up or across: 0.000 mm. Those
built minutes into the flight and those still there after the crash alike.

**The objects move with their holder, and only with it.** On the nearest quad, every vertex stands higher
or lower against the ground with Terrain Precision Fix alone than with both fixes, by nearly the same
amount for the 200 of them, and that amount is the *up* of their holder in the first flight: taking it off
leaves 0.2 mm (median over the vertices), 1.5 mm at most, within the precision of the reading at that
distance.

In other words: along a flight, the scatter is drawn on its quads just as after a load.

## Rock Precision Fix Diag, the holder pools over a flight

Hanging a holder from its quad takes it out of the container where stock keeps the holders of a pool, and
[the fix](the-fix-this-mod-proposes.md) hangs it back there when the quad is destroyed. A holder missed on
the way would be left on a quad that stock sends back to its cache of quads, to be reused elsewhere; nothing
would show on screen.

The instrument's holder record reads every pool of holders through stock's own bookkeeping, and counts the
holders that break a rule stock keeps: a holder in use that no longer exists or stands on a quad that is
not active, a free holder that does not hang from its pool's container or still has a quad, a count that
disagrees with its list, a holder in no pool. It is taken during a flight, where the terrain keeps building
quads ahead of the craft and destroying those behind it: the save
[`ref-mune-5km.sfs`](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/diag/README.md#the-saves),
a Mk1 command pod in a circular equatorial orbit 5 km over the Mun, loaded once, one record 30 s into the
flight, then about every two minutes, and one after the pod crashed into the relief. The same install as
above, once with Terrain Precision Fix alone and once with both fixes. The records are under
[diag](../diag/README.md#the-holder-pools-over-a-flight), and those with Terrain Precision Fix alone with
[the instrument](https://github.com/lhervier/KSP-RockPrecisionFixDiag/blob/main/diag/README.md#the-holder-pools-over-a-flight).

| record | holders in use | free | broken rules, Terrain Precision Fix alone | broken rules, both fixes |
|---|---|---|---|---|
| 1 | 344 | 40 | 0 | 0 |
| 2 | 168 | 216 | 0 | 0 |
| 3 | 144 | 240 | 0 | 0 |
| 4 | 224 | 160 | 0 | 0 |
| 5 | 152 | 232 | 0 | 0 |
| 6, after the crash | 568 | 40 | 0 | 0 |

**No holder is lost.** With both fixes, every holder handed back hangs in its pool's container, without a
quad, and every holder in use stands on a live quad of the Mun, at every record, the one after the crash
included.

**The pool works as in stock.** The counts of holders in use and free are the same, record for record, in
both flights: the flight takes as many holders out of the pool, at the same moments, with this mod as
without it. The pool of the Mun's `Rock00` grows from 384 holders to 608 after the crash, as stock makes
new ones when it has none free left, and those go through the fix as well.
