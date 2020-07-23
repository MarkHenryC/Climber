using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuiteSensible
{
    /// <summary>
    /// Basic touch controller. Mouse or mobile.
    /// </summary>
    public class TouchController : MonoBehaviour
    {
        public GameData gameData;
        public Transform target; // usually a camera
        public float yawAngleIncrement = 15f, pitchAngleIncrement = 15f;
        public float pitchMax = 45f, pitchMin = -45f;

        public System.Action ButtonUp;
        public Quaternion CurrentRotation => transform.localRotation;

        private bool cursorVisible;
        private Vector3 targetEuler = Vector3.zero;

        void Start()
        {
        }

        void FixedUpdate()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                var currentRot = target.localRotation.eulerAngles;

                if (currentRot.x > 180f) // Adjust to range -min .. +max for clamping. Unity will adjust negatives
                    currentRot.x -= 360f;

                switch (touch.phase)
                {
                    case TouchPhase.Stationary:                        
                        break;
                    case TouchPhase.Began:
                        break;
                    case TouchPhase.Ended:
                        ButtonUp?.Invoke();
                        break;
                    case TouchPhase.Moved:
                        break;
                }
                
                targetEuler.x = Mathf.Clamp(currentRot.x - touch.deltaPosition.y * pitchAngleIncrement * Time.deltaTime, pitchMin, pitchMax);
                targetEuler.y = currentRot.y + touch.deltaPosition.x * yawAngleIncrement * Time.deltaTime;

                target.localRotation = Quaternion.Euler(targetEuler);
            }
        }

        /// <summary>
        /// Only appropriate for testing in editor
        /// or if UI added for desktop or WebGL
        /// </summary>
        /// <param name="visible"></param>
        private void SetCursorVisible(bool visible)
        {
            cursorVisible = visible;
            Cursor.visible = cursorVisible;
            if (cursorVisible)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
    }
}