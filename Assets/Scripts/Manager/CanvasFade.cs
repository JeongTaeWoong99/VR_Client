    using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeCanvas : MonoBehaviour
{
    public Coroutine CurrentRoutine { private set; get; } = null;
    
    private CanvasGroup canvasGroup = null;
    private float       alpha       = 0.0f;

    private float quickFadeDuration = 0.5f;

    public  bool isMirroringScene;      // 미러링씬이면
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
        // 미러링방 입장(방이 없어도 입장이 가능하니 -> CreateRoom)
        if (isFirstFadeOut && isMirroringScene)
        {
            isFirstFadeOut = false;
            
            RoomOptions options = new RoomOptions();
            options.MaxPlayers  = 20;
            PhotonNetwork.CreateRoom(PlayerPrefs.GetString("roomName"),options);
            Debug.Log("실행");
        }
        // 쉐어룸 입장(방이 있어야 입장 가능하니 -> JoinRoom)
        else if (isFirstFadeOut && !isMirroringScene)
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