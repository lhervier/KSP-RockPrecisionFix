using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfix
{
    /// <summary>
    /// Draws terrain scatter (rocks, grass, trees) on the ground it was built on, at the same height at
    /// every load.
    ///
    /// Stock KSP builds the scatter objects of a terrain quad from the quad's own vertices, in the quad's
    /// own coordinates, but draws them from a holder that hangs from the body's terrain sphere, with a local
    /// position hundreds of kilometres long: 600 km on Kerbin, where a float can only represent every
    /// 62.5 mm. The quad keeps the world position Unity computed for that vector one way, the holder is drawn
    /// with a matrix Unity computes another way, and the two roundings differ: the objects are drawn a few
    /// centimetres above or below the ground, differently on each quad and on each load.
    ///
    /// Here each holder hangs from its own quad, at no offset, so that the objects are drawn with the very
    /// matrix the ground is drawn with. Stock scatter only, on the quads of the highest subdivision level,
    /// which are the only ones that carry any.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class RockPrecisionFixMod : MonoBehaviour
    {
        private const string HarmonyId = "com.github.lhervier.ksp.rockprecisionfix";

        // Set once the patches are in place. A holder is only ever moved while it is set.
        private static bool _active;

        // Private field of the stock scatter: the object its holders hang from while they are not in use.
        // Bound once, but read at every call: Kopernicus replaces it at runtime with an object of its own.
        private static AccessTools.FieldRef<PQSLandControl.LandClassScatter, GameObject> _scatterParent;

        // Spheres already reported, so that this message appears once per body and per session.
        private static readonly HashSet<PQS> _announced = new HashSet<PQS>();

        // Whether the missing pool container has been reported already.
        private static bool _warnedNoContainer;

        private void Start()
        {
            Log.LoadLevel();
            try
            {
                _scatterParent = AccessTools.FieldRefAccess<PQSLandControl.LandClassScatter, GameObject>(
                    "scatterParent");
                new Harmony(HarmonyId).PatchAll(typeof(RockPrecisionFixMod).Assembly);
                _active = true;
                Log.Info($"Version {typeof(RockPrecisionFixMod).Assembly.GetName().Version} installed,"
                    + $" log level {Log.Level}");
            }
            catch (Exception e)
            {
                // With _active left false, no holder is ever moved: the scatter is exactly stock.
                Log.Error($"Could not install the patches, the scatter is left as stock places it: {e}");
            }
        }

        /// <summary>Whether the fix acts on the scatter of this quad.</summary>
        private static bool AppliesTo(PQ quad)
        {
            if (!_active || quad == null)
            {
                return false;
            }
            PQS sphere = quad.sphereRoot;

            // On a sphere whose quads are not surface relative, stock places the holder at the centre of the
            // body, not at a 600 km vector: nothing to fix there.
            if (sphere == null || !sphere.surfaceRelativeQuads || sphere.LocalSpacePQStorage == null)
            {
                return false;
            }

            // Only the quads of the highest subdivision level are moved to this storage, which is not attached
            // to the body, and they are the only ones stock gives scatter to. Any other quad hangs from the
            // sphere itself, with the same kind of 600 km local position as the holder, so hanging the holder
            // from it would gain nothing.
            return quad.transform.parent == sphere.LocalSpacePQStorage.transform;
        }

        // ==========================================================================
        // A holder is given a quad
        // ==========================================================================

        /// <summary>
        /// Hangs a scatter holder from the quad it was just set up for, at no offset, when the fix applies to
        /// that quad. Otherwise, or if anything fails, the holder stays where stock placed it.
        /// </summary>
        private static void HangFromQuad(PQSMod_LandClassScatterQuad holder, PQ quad)
        {
            if (holder == null || !AppliesTo(quad))
            {
                return;
            }
            try
            {
                PQS sphere = quad.sphereRoot;
                bool announce = !_announced.Contains(sphere);

                // Everything that is only needed for the log is computed first, while the holder is still
                // where stock put it.
                double offsetMm = announce || Log.IsDebugEnabled ? DrawnOffsetMm(holder.transform, quad) : 0.0;

                // The objects of the holder are built from quad.verts, which are expressed in the quad's own
                // frame. With the holder at the identity under the quad, that frame is exactly the one they
                // are drawn in, and no transform in the chain holds a 600 km vector any more. The holder then
                // follows the quad through every move stock makes to it, floating origin shifts included,
                // without anything else to do.
                Transform holderTransform = holder.transform;
                holderTransform.SetParent(quad.transform, false);
                holderTransform.localPosition = Vector3.zero;
                holderTransform.localRotation = Quaternion.identity;
                holderTransform.localScale = Vector3.one;

                if (announce)
                {
                    _announced.Add(sphere);
                    Log.Info($"{sphere.name}: scatter drawn from its terrain quads"
                        + $" (first holder was {offsetMm:+0.00;-0.00;0.00} mm off)");
                }
                if (Log.IsDebugEnabled)
                {
                    Log.Debug($"scatter '{holder.scatter?.scatterName}' on quad '{quad.name}':"
                        + $" hung from its quad, was drawn {offsetMm:+0.00;-0.00;0.00} mm off it");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Could not hang a scatter holder from quad '{quad.name}',"
                    + $" it is left as stock places it: {e}");
            }
        }

        /// <summary>
        /// How far above the quad Unity draws the holder, in millimetres, along the vertical of the quad.
        /// Negative when the holder is drawn below it.
        /// </summary>
        private static double DrawnOffsetMm(Transform holder, PQ quad)
        {
            // Both are drawn with their local to world matrix. Its translation is a world position, so near
            // the floating origin, and the difference of two of them is short: a double holds it exactly.
            Vector3d holderOrigin = (Vector3)holder.localToWorldMatrix.GetColumn(3);
            Vector3d quadOrigin = (Vector3)quad.transform.localToWorldMatrix.GetColumn(3);

            // The vertical at the quad: the direction of its origin from the centre of the body, a unit
            // vector in the sphere's frame, turned into world axes. No long vector involved, and a float
            // direction is far more precise than needed on a gap of a few centimetres.
            Vector3d up = ((Vector3d)quad.sphereRoot.transform.TransformDirection(
                (Vector3)quad.positionPlanetRelative)).normalized;

            return Vector3d.Dot(holderOrigin - quadOrigin, up) * 1000.0;
        }

        [HarmonyPatch(typeof(PQSMod_LandClassScatterQuad), nameof(PQSMod_LandClassScatterQuad.Setup))]
        private static class SetupPatch
        {
            // After the stock method, which gives the holder its quad and its 600 km local position. Also
            // covers the Kopernicus holder, which inherits this method without redeclaring it.
            private static void Postfix(PQSMod_LandClassScatterQuad __instance, PQ quad)
            {
                HangFromQuad(__instance, quad);
            }
        }

        // ==========================================================================
        // A holder goes back to its pool
        // ==========================================================================

        /// <summary>
        /// Hangs a holder that <see cref="HangFromQuad"/> moved back from the container of its scatter's pool,
        /// at the identity, as stock places an unused holder. Does nothing to any other holder.
        /// </summary>
        private static void ReturnToPool(PQSLandControl.LandClassScatter scatter, PQSMod_LandClassScatterQuad holder)
        {
            // No check on _active: this only undoes what HangFromQuad did, recognised by the hierarchy itself.
            // Unity objects are compared with == on purpose, which also catches destroyed ones.
            if (holder == null || holder.quad == null || holder.transform.parent != holder.quad.transform)
            {
                return;
            }
            PQ quad = holder.quad;
            try
            {
                // Stock calls this from the quad's onDestroy, before the quad itself goes back to the PQS
                // cache, where it is deactivated and later reused elsewhere. The holder must not go with it:
                // stock expects every holder of its pool under that container, and moves them from there.
                GameObject container = _scatterParent != null ? _scatterParent(scatter) : null;
                Transform back;
                if (container != null)
                {
                    back = container.transform;
                }
                else
                {
                    // Not expected. The sphere is where stock creates that container, at the identity, so a
                    // holder hung there is placed the same way.
                    back = quad.sphereRoot.transform;
                    if (!_warnedNoContainer)
                    {
                        _warnedNoContainer = true;
                        Log.Warning($"scatter '{scatter.scatterName}' has no pool container,"
                            + " its holders are handed back to the terrain sphere instead");
                    }
                }

                Transform holderTransform = holder.transform;
                holderTransform.SetParent(back, false);
                holderTransform.localPosition = Vector3.zero;
                holderTransform.localRotation = Quaternion.identity;
                holderTransform.localScale = Vector3.one;

                if (Log.IsTraceEnabled)
                {
                    Log.Trace($"scatter '{scatter.scatterName}' on quad '{quad.name}': holder back in its pool");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Could not hand a scatter holder of quad '{quad.name}' back to its pool: {e}");
            }
        }

        [HarmonyPatch(typeof(PQSLandControl.LandClassScatter), nameof(PQSLandControl.LandClassScatter.DestroyQuad))]
        private static class DestroyQuadPatch
        {
            // Before the stock method, not after: the first thing it does is clear the holder's quad.
            private static void Prefix(PQSLandControl.LandClassScatter __instance, PQSMod_LandClassScatterQuad q)
            {
                ReturnToPool(__instance, q);
            }
        }
    }
}
