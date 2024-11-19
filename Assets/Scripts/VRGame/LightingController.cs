using System;
using System.Collections;
using UnityEngine;

public class LightingController : MonoBehaviour
{
    public float startIntensity;
    public float lightIntensity;

    
    public IEnumerator AdjustIntensity(float duration)
    {
        Light[] lights = GetComponentsInChildren<Light>();
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentIntensity = Mathf.Lerp(startIntensity, lightIntensity, elapsedTime / duration);

            foreach (Light light in lights)
            {
                if (light.type == LightType.Point)
                {
                    light.intensity = currentIntensity;
                }
            }

            yield return null;
        }
    }

    public void LightOn()
    {
        StartCoroutine(AdjustIntensity(2.0f));
    }
    
    private void Start()
    {
        Light[] lights = GetComponentsInChildren<Light>();

        foreach (Light light in lights)
        {
            light.intensity = startIntensity;
        }
    }
}
