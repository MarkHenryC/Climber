using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuiteSensible
{
    public class Player : MonoBehaviour
    {
        public Image lifeGauge;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetLife(float unitised)
        {
            lifeGauge.fillAmount = unitised;
        }
    }
}