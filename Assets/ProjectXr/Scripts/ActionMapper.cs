using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR.Input;
using System;

namespace quitesensible
{
    public class ActionMapper : MonoBehaviour
    {
        [SerializeField] protected InputActionReference actionReferenceThumb = null;
        [SerializeField] protected InputActionReference actionReferenceAim = null;
        [SerializeField] protected InputActionReference actionReferencePosition = null;
        [SerializeField] protected InputActionReference actionReferenceTrigger = null;
        [SerializeField] protected InputActionReference actionReferenceGrip = null;

        protected Action<Vector2> thumbCallback;
        protected Action<Vector3> aimCallback;
        protected Action<Vector3> positionCallback;
        protected Action<float> triggerCallback;
        protected Action<float> gripCallback;

        void OnEnable()
        {
            Debug.Assert(actionReferenceThumb != null && actionReferenceThumb.action != null);

            if (actionReferenceThumb)
            {
                actionReferenceThumb.action.started += UpdateThumbStarted;
                actionReferenceThumb.action.performed += UpdateThumbPerformed;
                actionReferenceThumb.action.canceled += UpdateThumbCanceled;

                actionReferenceThumb.action.Enable();
            }

            if (actionReferenceAim)
            {
                actionReferenceAim.action.started += UpdateAimStarted;
                actionReferenceAim.action.performed += UpdateAimPerformed;
                actionReferenceAim.action.canceled += UpdateAimCanceled;

                actionReferenceAim.action.Enable();
            }

            if (actionReferencePosition)
            {
                actionReferencePosition.action.started += UpdatePositionStarted;
                actionReferencePosition.action.performed += UpdatePositionPerformed;
                actionReferencePosition.action.canceled += UpdatePositionCanceled;

                actionReferencePosition.action.Enable();
            }

            if (actionReferenceThumb)
            {
                StartCoroutine(UpdateBinding(actionReferenceThumb));
            }
            
        }

        void OnDisable()
        {
            Debug.Assert(actionReferenceThumb != null && actionReferenceThumb.action != null);

            if (actionReferenceThumb)
            {
                actionReferenceThumb.action.started -= UpdateThumbStarted;
                actionReferenceThumb.action.performed -= UpdateThumbPerformed;
                actionReferenceThumb.action.canceled -= UpdateThumbCanceled;

                actionReferenceThumb.action.Disable();
            }

            if (actionReferenceAim)
            {
                actionReferenceAim.action.started -= UpdateAimStarted;
                actionReferenceAim.action.performed -= UpdateAimPerformed;
                actionReferenceAim.action.canceled -= UpdateAimCanceled;

                actionReferenceAim.action.Disable();
            }

            if (actionReferencePosition)
            {
                actionReferencePosition.action.started -= UpdatePositionStarted;
                actionReferencePosition.action.performed -= UpdatePositionPerformed;
                actionReferencePosition.action.canceled -= UpdatePositionCanceled;

                actionReferencePosition.action.Disable();
            }
        }

        public void SetThumbCallback(Action<Vector2> callback)
        {
            thumbCallback = callback;
        }

        public void SetAimCallback(Action<Vector3> callback)
        {
            aimCallback = callback;
        }

        public void SetPositionCallback(Action<Vector3> callback)
        {
            positionCallback = callback;
        }

        protected virtual void UpdateThumbStarted(InputAction.CallbackContext ctx) { }
        protected virtual void UpdateThumbPerformed(InputAction.CallbackContext ctx) { }
        protected virtual void UpdateThumbCanceled(InputAction.CallbackContext ctx) { }
        protected virtual void UpdateAimStarted(InputAction.CallbackContext ctx) { }
        protected virtual void UpdateAimPerformed(InputAction.CallbackContext ctx) { }
        protected virtual void UpdateAimCanceled(InputAction.CallbackContext ctx) { }
        protected virtual void UpdatePositionStarted(InputAction.CallbackContext ctx) { }
        protected virtual void UpdatePositionPerformed(InputAction.CallbackContext ctx) { }
        protected virtual void UpdatePositionCanceled(InputAction.CallbackContext ctx) { }

        private IEnumerator UpdateBinding (InputActionReference iar)
        {
            // Assume if we bind one control, the others should be OK
            while (isActiveAndEnabled)
            {
                if(actionReferenceThumb.action != null &&
                    actionReferenceThumb.action.controls.Count > 0 &&
                    actionReferenceThumb.action.controls[0].device != null &&
                    OpenXRInput.TryGetInputSourceName(actionReferenceThumb.action, 0, out var actionName, OpenXRInput.InputSourceNameFlags.Component, actionReferenceThumb.action.controls[0].device))
                {
                    OnActionBound();
                    break;
                }

                yield return new WaitForSeconds(1.0f);
            }
        }

        protected virtual void OnActionBound() { }
    }
}