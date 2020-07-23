using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuiteSensible
{
    public class Plasma : MonoBehaviour
    {
        public Transform mask;

        public void SetLength(float length)
        {
            Vector3 local = mask.localScale;
            local.x = length;
            mask.localScale = local;
        }

        void Start()
        {

        }

        void Update()
        {

        }
    }
}