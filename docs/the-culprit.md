# The culprit

Part of [Rock Precision Fix](../README.md). The short version is on the main page, under [The culprit](../README.md#the-culprit); here are the stock code and the reasoning.

[Checking the culprit](checking-the-culprit.md) checks it before anything is changed.

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

## Why it is different at every load

A rounding depends on the frame the vector is converted into: the world matrix of the terrain sphere,
whose rotation and translation change from one load to the next. It is the same frame, and the same
reason, as for the ground itself, set out on Terrain Precision Fix's page under
[Why it is different at every load](https://github.com/lhervier/KSP-TerrainPrecisionFix#why-it-is-different-at-every-load).
As there, it is a hypothesis read from the stock code, and the fix does not need it to hold: it removes
the long vector the rounding is drawn from.

## With Terrain Precision Fix

Terrain Precision Fix places the quads of the highest level in double precision. It moves the quad, not
its holder. The holder is still placed and drawn from a 600 km float, so it no longer shares even its
position with the quad: the rounding of the quad's origin, which Terrain Precision Fix removed from the
ground, is still in the holder, on top of the rounding of its matrix.

Both come from a transform holding a vector hundreds of kilometres long. The fix follows from that: hang
the holder from its quad, so that no transform in its chain holds one. How exactly is in
[The fix this mod proposes](the-fix-this-mod-proposes.md). First, the culprit has to hold: see [Checking the culprit](checking-the-culprit.md).
