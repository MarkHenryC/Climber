using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic touch controller. Mouse or mobile.
/// </summary>
public class TouchController : MonoBehaviour
{
    public Transform target; // usually a camera
    public float yawAngleIncrement = 15f, pitchAngleIncrement = 15f;
    public float pitchMax = 45f, pitchMin = -45f;

    public System.Action ButtonUp;

    private bool cursorVisible;
    private Vector3 targetEuler = Vector3.zero;

    void Start()
    {
        SetCursorVisible(false);
    }

    void Update()
    {

#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.Escape))
            SetCursorVisible(!cursorVisible);
#endif

        if (Input.GetMouseButtonUp(0)) // simulate finger off screen
        {
            ButtonUp?.Invoke();
        }
        else if (Input.GetMouseButton(0))
        {
            var currentPos = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            var currentRot = target.localRotation.eulerAngles;
            
            if (currentRot.x > 180f) // Adjust to range -min .. +max for clamping. Unity will adjust negatives
                currentRot.x -= 360f;    
            
            targetEuler.x = Mathf.Clamp(currentRot.x - currentPos.y * pitchAngleIncrement * Time.deltaTime, pitchMin, pitchMax);
            targetEuler.y = currentRot.y + currentPos.x * yawAngleIncrement * Time.deltaTime;

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
