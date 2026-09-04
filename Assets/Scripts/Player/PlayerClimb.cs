using System.Collections;
using MouseGame.Environment;
using MouseGame.Input;
using MouseGame.UI;
using UnityEngine;

namespace MouseGame.Player
{
    /// <summary>
    /// Simple mantle/climb: detect a Climbable surface ahead within reach, show a prompt,
    /// and on input slide the player up onto it. This is deliberately the "simple" version
    /// called out in the README (detect -> verify destination -> indicate -> move) — no
    /// animation, no Assassin's Creed-style traversal.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerClimb : MonoBehaviour
    {
        private enum LedgeCheckResult
        {
            NoWallHit,
            WallNotClimbable,
            NoLedgeSurfaceFound,
            HeightOutOfRange,
            Blocked,
            Found
        }

        [Header("Detection")]
        [SerializeField] private float wallCheckDistance = 0.6f;
        [SerializeField] private float wallCheckHeight = 0.3f;

        [Header("Ledge height range (above the player's feet)")]
        [SerializeField] private float minLedgeHeight = 0.35f;
        [SerializeField] private float maxLedgeHeight = 1.5f;

        [Header("Movement")]
        [SerializeField] private float climbDuration = 0.3f;

        [Header("References")]
        [SerializeField] private ClimbPromptUI promptUI;

        [Header("Debug")]
        [Tooltip("Logs why a climb was/wasn't detected (once per state change) and draws detection rays as Gizmos while playing, when this Player is selected.")]
        [SerializeField] private bool debugLogging = true;

        private CharacterController controller;
        private PlayerInputReader input;
        private PlayerMotor motor;
        private bool isClimbing;

        // -- Gizmo/log state, updated every TryFindLedge() call --
        private LedgeCheckResult lastResult;
        private LedgeCheckResult lastLoggedResult = (LedgeCheckResult)(-1);
        private Vector3 debugWallOrigin;
        private Vector3 debugWallDir;
        private bool debugHasWallHit;
        private Vector3 debugWallHitPoint;
        private string debugWallHitName;
        private Vector3 debugProbeOrigin;
        private float debugProbeDistance;
        private bool debugHasLedgeHit;
        private Vector3 debugLedgeHitPoint;
        private float debugLedgeHeightAboveFeet;

        /// <summary>World position of the player's feet (bottom of the CharacterController capsule).</summary>
        private Vector3 FeetPosition =>
            transform.position - Vector3.up * (controller.height * 0.5f - controller.center.y);

        private Vector3 FeetToPivot(Vector3 feetPosition) =>
            feetPosition + Vector3.up * (controller.height * 0.5f - controller.center.y);

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<PlayerInputReader>();
            motor = GetComponent<PlayerMotor>();
        }

        private void Update()
        {
            if (isClimbing)
            {
                return;
            }

            if (TryFindLedge(out Vector3 landingPivot))
            {
                promptUI?.ShowPrompt();

                if (input.ClimbPressed)
                {
                    StartCoroutine(ClimbRoutine(landingPivot));
                }
            }
            else
            {
                promptUI?.HidePrompt();
            }
        }

        private bool TryFindLedge(out Vector3 landingPivot)
        {
            landingPivot = default;

            Vector3 feet = FeetPosition;
            Vector3 wallOrigin = feet + Vector3.up * wallCheckHeight;
            debugWallOrigin = wallOrigin;
            debugWallDir = transform.forward;
            debugHasWallHit = false;

            if (!Physics.Raycast(wallOrigin, transform.forward, out RaycastHit wallHit, wallCheckDistance))
            {
                Report(LedgeCheckResult.NoWallHit);
                return false;
            }

            debugHasWallHit = true;
            debugWallHitPoint = wallHit.point;
            debugWallHitName = wallHit.collider.name;

            if (wallHit.collider.GetComponent<Climbable>() == null)
            {
                Report(LedgeCheckResult.WallNotClimbable);
                return false;
            }

            // Probe down from above the max reachable height, just past the wall, to find the
            // ledge's top surface.
            Vector3 probeOrigin = feet
                + transform.forward * (wallHit.distance + 0.1f)
                + Vector3.up * maxLedgeHeight;
            float probeDistance = maxLedgeHeight + 0.2f;
            debugProbeOrigin = probeOrigin;
            debugProbeDistance = probeDistance;
            debugHasLedgeHit = false;

            if (!Physics.Raycast(probeOrigin, Vector3.down, out RaycastHit ledgeHit, probeDistance))
            {
                Report(LedgeCheckResult.NoLedgeSurfaceFound);
                return false;
            }

            debugHasLedgeHit = true;
            debugLedgeHitPoint = ledgeHit.point;

            float ledgeHeightAboveFeet = ledgeHit.point.y - feet.y;
            debugLedgeHeightAboveFeet = ledgeHeightAboveFeet;
            if (ledgeHeightAboveFeet < minLedgeHeight || ledgeHeightAboveFeet > maxLedgeHeight)
            {
                Report(LedgeCheckResult.HeightOutOfRange);
                return false;
            }

            Vector3 landingFeet = ledgeHit.point + Vector3.up * 0.05f;

            if (!HasClearanceAt(landingFeet))
            {
                Report(LedgeCheckResult.Blocked);
                return false;
            }

            landingPivot = FeetToPivot(landingFeet);
            Report(LedgeCheckResult.Found);
            return true;
        }

        private bool HasClearanceAt(Vector3 feetPosition)
        {
            float halfHeight = controller.height * 0.5f;
            Vector3 pivot = FeetToPivot(feetPosition);
            Vector3 top = pivot + Vector3.up * (halfHeight - controller.radius);
            Vector3 bottom = pivot - Vector3.up * (halfHeight - controller.radius);

            return !Physics.CheckCapsule(bottom, top, controller.radius * 0.9f, ~0, QueryTriggerInteraction.Ignore);
        }

        private void Report(LedgeCheckResult result)
        {
            lastResult = result;

            if (!debugLogging || result == lastLoggedResult)
            {
                return;
            }

            lastLoggedResult = result;

            switch (result)
            {
                case LedgeCheckResult.NoWallHit:
                    Debug.Log("[PlayerClimb] nothing ahead within reach.", this);
                    break;
                case LedgeCheckResult.WallNotClimbable:
                    Debug.Log($"[PlayerClimb] hit '{debugWallHitName}' but it has no Climbable component.", this);
                    break;
                case LedgeCheckResult.NoLedgeSurfaceFound:
                    Debug.Log("[PlayerClimb] climbable wall ahead, but no surface found on top of it within reach.", this);
                    break;
                case LedgeCheckResult.HeightOutOfRange:
                    Debug.Log($"[PlayerClimb] ledge is {debugLedgeHeightAboveFeet:F2}m above feet, outside [{minLedgeHeight}, {maxLedgeHeight}].", this);
                    break;
                case LedgeCheckResult.Blocked:
                    Debug.Log("[PlayerClimb] ledge in range but landing spot is blocked (not enough clearance above it).", this);
                    break;
                case LedgeCheckResult.Found:
                    Debug.Log("[PlayerClimb] ledge found — press E to climb.", this);
                    break;
            }
        }

        private IEnumerator ClimbRoutine(Vector3 landingPivot)
        {
            isClimbing = true;
            promptUI?.HidePrompt();

            if (motor != null)
            {
                motor.enabled = false;
            }
            controller.enabled = false;

            Vector3 start = transform.position;
            float elapsed = 0f;
            while (elapsed < climbDuration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, landingPivot, elapsed / climbDuration);
                yield return null;
            }
            transform.position = landingPivot;

            controller.enabled = true;
            if (motor != null)
            {
                motor.enabled = true;
            }
            isClimbing = false;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || controller == null)
            {
                return;
            }

            Gizmos.color = debugHasWallHit ? Color.yellow : Color.red;
            Gizmos.DrawLine(debugWallOrigin, debugWallOrigin + debugWallDir * wallCheckDistance);
            if (debugHasWallHit)
            {
                Gizmos.DrawSphere(debugWallHitPoint, 0.05f);
            }

            if (lastResult == LedgeCheckResult.NoWallHit || lastResult == LedgeCheckResult.WallNotClimbable)
            {
                return;
            }

            Gizmos.color = debugHasLedgeHit ? Color.cyan : Color.magenta;
            Gizmos.DrawLine(debugProbeOrigin, debugProbeOrigin + Vector3.down * debugProbeDistance);
            if (debugHasLedgeHit)
            {
                Gizmos.color = lastResult == LedgeCheckResult.Found ? Color.green : Color.red;
                Gizmos.DrawSphere(debugLedgeHitPoint, 0.06f);
            }
        }
    }
}
