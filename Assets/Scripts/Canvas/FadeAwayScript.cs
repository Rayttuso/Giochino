using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class FadeAwayScript : MonoBehaviour
{

    private GameObject go;
    public float fadeTime;
    private TextMeshProUGUI fadeAwayText;
    public float alphaValue;
    public float fadeAwayPerSecond;

    void Start()
    {
        fadeAwayText = GetComponent<TextMeshProUGUI>();
        fadeAwayPerSecond = 1 / fadeTime;
        alphaValue = fadeAwayText.color.a;
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeTime > 0 && fadeAwayText.enabled==true)
        {
            alphaValue -= fadeAwayPerSecond * Time.deltaTime;
            fadeAwayText.color = new Color(fadeAwayText.color.r, fadeAwayText.color.g, fadeAwayText.color.b, alphaValue);
            fadeTime = Time.deltaTime;
        }
    }
}
