using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RightController : MonoBehaviour
{
    public float rotateDegrees = 45f;
    public CharacterController character;
    public float beamLength = 1f;

    private Quaternion controlDirection;
    private Vector3 controlPosition;
    private Vector2 thumbVal;
    private float turnReading;

    private LineRenderer beam;

    private const float triggerThreshold = 0.25f;

    void Start()
    {
        beam = GetComponent<LineRenderer>();
    }

    public void Trigger(InputAction.CallbackContext ctx)
    {

    }

    public void Thumbstick(InputAction.CallbackContext ctx)
    {
        switch (ctx.phase)
        {
            case InputActionPhase.Started:
                thumbVal = ctx.ReadValue<Vector2>();
                ReadAction(thumbVal.x);
                break;
            case InputActionPhase.Performed:
                thumbVal = ctx.ReadValue<Vector2>();
                ReadAction(thumbVal.x);
                break;
            case InputActionPhase.Canceled:
                thumbVal = ctx.ReadValue<Vector2>();
                ReadAction(thumbVal.x);

                if (turnReading != 0f)
                {
                    character.transform.Rotate(0f, turnReading * rotateDegrees, 0f);
                    turnReading = 0f;
                }

                break;
        }
    }

    public void Aim(InputAction.CallbackContext ctx)
    {
        controlDirection = ctx.ReadValue<Quaternion>();
        DrawBeam();
    }

    public void Position(InputAction.CallbackContext ctx)
    {
        controlPosition = ctx.ReadValue<Vector3>();
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

    private void DrawBeam()
    {
        if (beam)
        {
            beam.SetPosition(0, controlPosition);
            beam.SetPosition(1, controlPosition + controlDirection * Vector3.forward * beamLength);
        }
    }
}
