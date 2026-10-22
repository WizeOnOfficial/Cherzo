using System.Collections;
using UnityEngine;

namespace GameDevelopmentProject.Animation
{
    [DisallowMultipleComponent]
    public class SlidingDoorController : MonoBehaviour
    {
        [Header("Slide Configuration")]
        [Tooltip("Offset in local coordinates when door is opened.")]
        [SerializeField] private Vector3 openOffset = new Vector3(0f, 0f, 1.8f);

        [Tooltip("Duration of the sliding animation in seconds.")]
        [SerializeField] private float slideDuration = 1.2f;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioSource doorAudioSource;
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip closeClip;

        private Vector3 closedLocalPosition;
        private Vector3 openedLocalPosition;
        private Coroutine activeSlideCoroutine;
        private bool isOpen;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            closedLocalPosition = transform.localPosition;
            openedLocalPosition = closedLocalPosition + openOffset;

            if (doorAudioSource == null)
            {
                doorAudioSource = GetComponent<AudioSource>();
            }
        }

        public void OpenDoor()
        {
            if (isOpen)
            {
                return;
            }

            if (activeSlideCoroutine != null)
            {
                StopCoroutine(activeSlideCoroutine);
            }

            activeSlideCoroutine = StartCoroutine(SlideRoutine(openedLocalPosition, true));
        }

        public void CloseDoor()
        {
            if (!isOpen)
            {
                return;
            }

            if (activeSlideCoroutine != null)
            {
                StopCoroutine(activeSlideCoroutine);
            }

            activeSlideCoroutine = StartCoroutine(SlideRoutine(closedLocalPosition, false));
        }

        private IEnumerator SlideRoutine(Vector3 targetLocalPos, bool targetIsOpen)
        {
            Vector3 startPos = transform.localPosition;
            float elapsed = 0f;

            PlaySound(targetIsOpen ? openClip : closeClip);

            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                transform.localPosition = Vector3.Lerp(startPos, targetLocalPos, smoothT);
                yield return null;
            }

            transform.localPosition = targetLocalPos;
            isOpen = targetIsOpen;
            activeSlideCoroutine = null;
        }

        private void PlaySound(AudioClip clip)
        {
            if (doorAudioSource != null && clip != null)
            {
                doorAudioSource.PlayOneShot(clip);
            }
        }

        public void SetOpenOffset(Vector3 offset)
        {
            openOffset = offset;
            openedLocalPosition = closedLocalPosition + openOffset;
        }
    }
}
