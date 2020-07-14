using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuiteSensible
{
    public class Fader : MonoBehaviour
    {
        public Image fadeImage;
        public Color imageColor;
        public float fadeTime = 1f;
        public bool test;

        private Color clearColor, opaqueColor;

        private void OnEnable()
        {
            fadeImage.color = imageColor;
            clearColor = opaqueColor = imageColor;
            clearColor.a = 0f;
            opaqueColor.a = 1f;
        }

        void Start()
        {
            if (test)
                FadeIn(OnComplete);
        }

        public void FadeIn(System.Action a = null)
        {
            LeanTween.value(gameObject, TweenColor, opaqueColor, clearColor, fadeTime).setOnComplete(a);
        }

        public void FadeOut(System.Action a = null)
        {
            LeanTween.value(gameObject, TweenColor, clearColor, opaqueColor, fadeTime).setOnComplete(a);
        }

        private void TweenColor(Color c)
        {
            fadeImage.color = c;
        }

        private void OnComplete()
        {
            Debug.Log("Fade complete");
        }
    }
}