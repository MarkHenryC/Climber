using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace QuiteSensible
{
    public class Player : MonoBehaviour
    {
        public Image lifeGauge;
        public TextMeshProUGUI displayText;
        public GameData gameData;

        void Start()
        {
            //if (gameData)
            //    gameData.player = this;
        }

        void Update()
        {

        }

        public void SetText(string text)
        {
            displayText.text = text;
        }

        public void SetLife(float unitised)
        {
            lifeGauge.fillAmount = unitised;
        }
    }
}