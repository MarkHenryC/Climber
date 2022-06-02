using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace quitesensible
{
    public class GetControlValues : MonoBehaviour
    {
        public Transform primary2DAxis;
        public ActionMapper mapper;

        private const float MaxTilt = 30f;

        private void Start()
        {
            mapper?.SetThumbCallback(Controller2dAxis);
        }

        private void Controller2dAxis(Vector2 axisValue)
        {
            primary2DAxis.localRotation = Quaternion.Euler(axisValue.y * -MaxTilt, 0f, axisValue.x * MaxTilt);
        }

        private void ControllerDirection(Vector3 directionValue)
        {
        }

    }
}