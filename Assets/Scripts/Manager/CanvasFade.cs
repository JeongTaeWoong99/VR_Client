using System.Collections;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeCanvas : MonoBehaviour
{
    public Coroutine CurrentRoutine { private set; get; } = null;
    
    private CanvasGroup canvasGroup = null;
    private float       alpha       = 0.0f;

    private float quickFadeDuration = 0.5f;

    private bool isFirstFadeOut = true; // 맨 처음 들어왔을 때, 세팅 전의 페이드 아웃인가

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        LevelManager.FadeIn  += StartFadeIn;
        LevelManager.FadeOut += StartFadeOut;
    }
    
    private void OnDestroy()
    {
        LevelManager.FadeIn  -= StartFadeIn;
        LevelManager.FadeOut -= StartFadeOut;
    }
    
    public void StartFadeIn(float fadeDuration)
    {
        canvasGroup.alpha = 1f;
        StopAllCoroutines();
        CurrentRoutine = StartCoroutine(FadeIn(fadeDuration));
    }

    public void StartFadeOut(float fadeDuration)
    {
        StopAllCoroutines();
        CurrentRoutine = StartCoroutine(FadeOut(fadeDuration));
    }

    public void QuickFadeIn()
    {
        StopAllCoroutines();
        CurrentRoutine = StartCoroutine(FadeIn(quickFadeDuration));
    }

    public void QuickFadeOut()
    {
        StopAllCoroutines();
        CurrentRoutine = StartCoroutine(FadeOut(quickFadeDuration));
    }

    private IEnumerator FadeIn(float duration)
    {
        float elapsedTime = 0.0f;

        while (alpha <= 1.0f)
        {
            SetAlpha(elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeOut(float duration)
    {
        float elapsedTime = 0.0f;

        while (alpha >= 0.0f)
        {
            SetAlpha(1 - (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 처음 세팅
        if (isFirstFadeOut)
        {
            isFirstFadeOut = false;
            PhotonNetwork.JoinRoom(PlayerPrefs.GetString("roomName")); // 룸 입장
        }
    }

    private void SetAlpha(float value)
    {
        alpha = value;
        canvasGroup.alpha = alpha;
    }
}