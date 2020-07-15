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
        private int halfScreenWidth, halfScreenHeight;
        private Vector2 touchDownPos;
        private Vector2 unitizedFirstTouch = Vector3.zero, unitizedTouchDelta = Vector2.zero;

        void Start()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            SetCursorVisible(false);
#endif
            halfScreenWidth = Screen.width / 2;
            halfScreenHeight = Screen.height / 2;
        }

        void FixedUpdate()
        {

#if xUNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetKey(KeyCode.Escape))
                SetCursorVisible(!cursorVisible);

            if (Input.GetMouseButtonUp(0)) // simulate finger off screen
            {
                ButtonUp?.Invoke();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                touchDownPos.x = Input.GetAxis("Mouse X");
                touchDownPos.y = Input.GetAxis("Mouse Y");                
            }
            else if (Input.GetMouseButton(0))
            {
                currentDownPos.x = Input.GetAxis("Mouse X");
                currentDownPos.y = Input.GetAxis("Mouse Y");

                Vector2 deltaPos = currentDownPos - touchDownPos;
                var currentRot = target.localRotation.eulerAngles;

                if (currentRot.x > 180f) // Adjust to range -min .. +max for clamping. Unity will adjust negatives
                    currentRot.x -= 360f;

                targetEuler.x = Mathf.Clamp(currentRot.x - deltaPos.y * pitchAngleIncrement * Time.deltaTime, pitchMin, pitchMax);
                targetEuler.y = currentRot.y + deltaPos.x * yawAngleIncrement * Time.deltaTime;

                target.localRotation = Quaternion.Euler(targetEuler);
            }
#else
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                var currentRot = target.localRotation.eulerAngles;

                if (currentRot.x > 180f) // Adjust to range -min .. +max for clamping. Unity will adjust negatives
                    currentRot.x -= 360f;

                Vector2 touchDelta = touch.position - touchDownPos;                

                switch (touch.phase)
                {
                    case TouchPhase.Stationary:                        
                        break;
                    case TouchPhase.Began:
                        unitizedFirstTouch.x = (touch.position.x - halfScreenWidth) / halfScreenWidth;
                        unitizedFirstTouch.y = (touch.position.y - halfScreenHeight) / halfScreenHeight;
                        break;
                    case TouchPhase.Ended:
                        unitizedTouchDelta = Vector2.zero;
                        ButtonUp?.Invoke();
                        break;
                    case TouchPhase.Moved:
                        var unitX = (touch.position.x - halfScreenWidth) / halfScreenWidth;
                        var unitY = (touch.position.y - halfScreenHeight) / halfScreenHeight;
                        unitizedTouchDelta.x = unitX - unitizedFirstTouch.x;
                        unitizedTouchDelta.y = unitY - unitizedFirstTouch.y;
                        break;
                }
                
                targetEuler.x = Mathf.Clamp(currentRot.x - unitizedTouchDelta.y * pitchAngleIncrement * Time.deltaTime, pitchMin, pitchMax);
                targetEuler.y = currentRot.y + unitizedTouchDelta.x * yawAngleIncrement * Time.deltaTime;

                gameData.PlayerUiText(string.Format("unitizedTouchDelta.x: {0}, unitizedTouchDelta.y: {1}", unitizedTouchDelta.x, unitizedTouchDelta.y));

                target.localRotation = Quaternion.Euler(targetEuler);
            }
#endif
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