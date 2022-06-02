using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR.Input;
using System;

namespace quitesensible
{
    public class ActionMapperLeft : ActionMapper
    { 
        public float moveSpeed = 100f;
        public CharacterController character;
        public float beamLength = .5f;

        private Quaternion controlDirection;
        private Vector3 controlPosition;
        private Vector2 thumbVal;

        private LineRenderer beam;

        private void Start()
        {
            beam = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            // Adjust for possible reorientation from snap turn
            character.SimpleMove(controlDirection * character.transform.rotation * 
                Vector3.forward * thumbVal.y * moveSpeed * Time.deltaTime);
        }

        protected override void UpdateThumbStarted(InputAction.CallbackContext ctx)
        {
            thumbVal = ctx.ReadValue<Vector2>();
            thumbCallback?.Invoke(thumbVal);
        }

        protected override void UpdateThumbPerformed(InputAction.CallbackContext ctx)
        {
            thumbVal = ctx.ReadValue<Vector2>();
            thumbCallback?.Invoke(thumbVal);
        }

        protected override void UpdateThumbCanceled(InputAction.CallbackContext ctx)
        {
            thumbVal = ctx.ReadValue<Vector2>();
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