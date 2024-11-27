using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeCanvas : MonoBehaviour
{
    public Coroutine CurrentRoutine { private set; get; } = null;

    [SerializeField]
    private Renderer fadeQuadRenderer;
    private float    alpha = 0.0f;

    public float quickFadeDuration = 1f;

    public  bool isMirroringScene;      // 미러링씬이면
    private bool isFirstFadeOut = true; // 맨 처음 들어왔을 때, 세팅 전의 페이드 아웃인가

    private void Awake()
    {
        StartFadeOut(quickFadeDuration);
    }
    
    public void StartFadeIn(float fadeDuration)
    {
        fadeQuadRenderer.material.color= new Color(0, 0, 0, 1);
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
        // 미러링방 입장(교육 게임방이 없어도 입장이 가능하니 CreateRoom -> 만들기 실패하면, JoinRoom)
        if (isFirstFadeOut && isMirroringScene)
        {
            isFirstFadeOut = false;
            
            RoomOptions options = new RoomOptions();
            options.MaxPlayers  = 20;
            PhotonNetwork.CreateRoom(PlayerPrefs.GetString("roomName"),options);
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
        fadeQuadRenderer.material.color= new Color(0, 0, 0, alpha);
    }
}