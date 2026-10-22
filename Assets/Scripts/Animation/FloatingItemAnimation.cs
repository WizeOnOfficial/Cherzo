using UnityEngine;

namespace GameDevelopmentProject.Animation
{
    /// <summary>
    /// Gentle hover + spin for world pickups (UTS rubric: animated object).
    /// Pauses automatically when the object is re-parented (held) or is falling under physics.
    /// </summary>
    [DisallowMultipleComponent]
    public class FloatingItemAnimation : MonoBehaviour
    {
        [Header("Hover Configuration")]
        [SerializeField] private float hoverAmplitude = 0.08f;
        [SerializeField] private float hoverFrequency = 2.2f;

        [Header("Rotation Configuration")]
        [SerializeField] private float rotateSpeed = 60f;
        [SerializeField] private Vector3 rotateAxis = Vector3.up;
        [Tooltip("ON = spin around the world axis (looks like a classic pickup even if the model is tilted).")]
        [SerializeField] private bool rotateInWorldSpace = true;

        private Vector3 basePosition;
        private Transform originalParent;
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            CaptureBase();
        }

        private void Update()
        {
            // Held / re-parented by another system: do not fight it.
            if (transform.parent != originalParent)
            {
                return;
            }

            // Physics is moving it (dropped / falling): follow physics, re-anchor.
            if (rb != null && !rb.isKinematic && rb.linearVelocity.sqrMagnitude > 0.05f)
            {
                basePosition = transform.localPosition;
                return;
            }

            float heightOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            transform.localPosition = basePosition + Vector3.up * heightOffset;

            transform.Rotate(rotateAxis, rotateSpeed * Time.deltaTime,
                rotateInWorldSpace ? Space.World : Space.Self);
        }

        /// <summary>Re-anchor the hover around the current position (e.g. after being dropped).</summary>
        public void CaptureBase()
        {
            originalParent = transform.parent;
            basePosition = transform.localPosition;
        }
    }
}
