using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR.Input;
using System;

namespace quitesensible
{
    public class ActionMapperRight : ActionMapper
    {
        public float rotateDegrees = 45f;
        public CharacterController character;
        public float beamLength = 1f;

        private Quaternion controlDirection;
        private Vector2 thumbVal;
        private float turnReading;
        private Vector3 controlPosition;

        private LineRenderer beam;

        private const float triggerThreshold = 0.25f;

        private void Start()
        {
            beam = GetComponent<LineRenderer>();
        }

        protected override void UpdateThumbStarted(InputAction.CallbackContext ctx)
        {
            thumbVal = ctx.ReadValue<Vector2>();
            ReadAction(thumbVal.x);
            thumbCallback?.Invoke(thumbVal);
        }

        protected override void UpdateThumbPerformed(InputAction.CallbackContext ctx)
        {
            thumbVal = ctx.ReadValue<Vector2>();
            ReadAction(thumbVal.x);
            thumbCallback?.Invoke(thumbVal);
        }

        protected override void UpdateThumbCanceled(InputAction.CallbackContext ctx)
        {
            thumbVal = ctx.ReadValue<Vector2>();
            ReadAction(thumbVal.x);

            if (turnReading != 0f)
            {
                character.transform.Rotate(0f, turnReading * rotateDegrees, 0f);
                turnReading = 0f;
            }

            thumbCallback?.Invoke(thumbVal);
        }

        protected override void UpdateAimStarted(InputAction.CallbackContext ctx)
        {
            controlDirection = ctx.ReadValue<Quaternion>();
        }

        protected override void UpdateAimPerformed(InputAction.CallbackContext ctx)
        {
            controlDirection = ctx.ReadValue<Quaternion>();
        }

        protected override void UpdateAimCanceled(InputAction.CallbackContext ctx)
        {
            controlDirection = ctx.ReadValue<Quaternion>();
        }


        private void ReadAction(float xVal)
        {
            float absVal = Mathf.Abs(xVal);
            if (absVal >= triggerThreshold)
            {
                if (absVal > Mathf.Abs(turnReading))
                    turnReading = xVal;
            }
        }

        protected override void UpdatePositionPerformed(InputAction.CallbackContext ctx)
        {
            controlPosition = ctx.ReadValue<Vector3>();

            DrawBeam();
        }

        private void DrawBeam()
        {
            if (beam)
            {
                beam.SetPosition(0, controlPosition);
                beam.SetPosition(1, controlPosition + controlDirection * Vector3.forward * beamLength);
            }
        }
    }
}