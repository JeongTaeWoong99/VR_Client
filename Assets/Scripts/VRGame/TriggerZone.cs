using UnityEngine;
using System.Collections;

public class TriggerZone : MonoBehaviour
{
    private bool played = false;
    public GameObject vortex;
    public GameObject outsideController;
    public float originSize;
    public float targetSize;
    public float fadeDuration;
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")&&played==false)
        {
            played = true;
            StartCoroutine(VortexOn());
        }
        else if (other.CompareTag("Vortex"))
        {
            SpaceOutsideController spaceOutsideController = outsideController.GetComponent<SpaceOutsideController>();
            spaceOutsideController.sideSpeed = 0;
            spaceOutsideController.forwardSpeed = 0;
            AudioManager.instance.Stop("Engine");
            
            AudioManager.instance.Play("Ending");
            
            StartCoroutine(IngameManager.instance.ReturnToMainMenu(5f));
        }
    }

    private IEnumerator VortexOn()
    {
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float size = Mathf.Lerp(originSize, targetSize, elapsedTime / fadeDuration);
            vortex.transform.localScale = size * Vector3.one;
            yield return null;
        }

        // 정확히 설정
        vortex.transform.localScale = targetSize * Vector3.one;
        AudioManager.instance.Play("Vortex");
    }
}