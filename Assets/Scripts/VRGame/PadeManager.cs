using System;
using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class PadeManager : MonoBehaviour
{ 
    public static PadeManager instance;

    private void Awake()
    {
        instance = this;
    }

    public Renderer fadeQuadRenderer; // Quad의 Renderer
    public float fadeDuration = 1.0f;
    public GameObject quad;
    public void FadeOut()
    {
        StartCoroutine(Fade(1)); // Alpha 값을 1로
    }

    public void FadeIn()
    {
        StartCoroutine(Fade(0)); // Alpha 값을 0으로
    }

    private void Start()
    {
        quad.SetActive(true);
        FadeIn();
    }

    private IEnumerator Fade(float targetAlpha)
    {
        Color color = fadeQuadRenderer.material.color;
        float currentAlpha = color.a;
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(currentAlpha, targetAlpha, elapsedTime / fadeDuration);
            fadeQuadRenderer.material.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // 정확히 설정
        fadeQuadRenderer.material.color = new Color(color.r, color.g, color.b, targetAlpha);
    }
}
