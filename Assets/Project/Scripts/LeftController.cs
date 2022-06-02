using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[System.Serializable]
public class UpdateVector3 : UnityEvent<Vector3> { }

public class LeftController : MonoBehaviour
{
    public float moveSpeed = 100f;
    public CharacterController character;
    public float rotateDegrees = 45f;

    public UpdateVector3 updateControllerPosition, updateControllerAim;

    private Quaternion controlDirection;
    private Vector3 controlPosition;
    private bool triggerDown;
    private float turnReading;
    private Vector2 thumbVal;

    private LineRenderer beam;

    private const float triggerThreshold = 0.25f;

    void Start()
    {
        beam = GetComponent<LineRenderer>();
    }

    public void UpdateScanLength(Vector3 position, Vector3 direction, float length)
    {
        DrawBeam(position, direction, length);
    }

    public void TriggerMove(InputAction.CallbackContext ctx)
    {
        switch (ctx.phase)
        {
            case InputActionPhase.Started:
                triggerDown = true;
                break;
            case InputActionPhase.Performed:
                character.SimpleMove(controlDirection * character.transform.rotation *
                    Vector3.forward * moveSpeed * Time.deltaTime);
                break;
            case InputActionPhase.Canceled:
                triggerDown = false;
                break;
        }
    }

    public void TriggerTeleport(InputAction.CallbackContext ctx)
    {
        switch (ctx.phase)
        {
            case InputActionPhase.Started:
                triggerDown = true;
                break;
            case InputActionPhase.Performed:
                character.SimpleMove(controlDirection * character.transform.rotation *
                    Vector3.forward * moveSpeed * Time.deltaTime);
                break;
            case InputActionPhase.Canceled:
                triggerDown = false;
                break;
        }
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
        updateControllerAim?.Invoke(controlDirection * Vector3.forward);
    }

    public void Position(InputAction.CallbackContext ctx)
    {
        controlPosition = ctx.ReadValue<Vector3>();
        updateControllerPosition?.Invoke(controlPosition);
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

    private void DrawBeam(Vector3 position, Vector3 direction, float length)
    {
        if (beam)
        {
            beam.SetPosition(0, position);
            beam.SetPosition(1, position + direction * length);
        }
    }
}
