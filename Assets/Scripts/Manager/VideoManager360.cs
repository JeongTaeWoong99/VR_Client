using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Video;

// 화면 공유 전용
public class VideoManager360 : MonoBehaviourPunCallbacks
{
    public GameObject[] objectsToHide;
    public FadeCanvas   fadeCanvas;
    public Material     videoMaterial;
    public VideoPlayer  videoPlayer;
    public float        fadeDuration = 1.0f;

    private Material _skyMaterial;
    
    private void Start()
    {
        _skyMaterial = RenderSettings.skybox;

        videoPlayer.url = PlayerPrefs.GetString("videoPath");   // 위치를 정적으로 받아서, 바로 넣어주기
    }

    [PunRPC]
    public void StartVideo()
    {
        StartCoroutine(FadeAndSwitchVideo(videoMaterial, videoPlayer.Play));
    }

    [PunRPC]
    public void PauseVideo()
    {
        StartCoroutine(FadeAndSwitchVideo(_skyMaterial, videoPlayer.Pause));
    }
    
    private IEnumerator FadeAndSwitchVideo(Material targetMaterial, Action onCompleteAction)
    {
        // fadeCanvas.QuickFadeIn();
        // yield return new WaitForSeconds(fadeDuration);

        SetObjectsActive(targetMaterial.Equals(_skyMaterial));
        //fadeCanvas.QuickFadeOut();

        RenderSettings.skybox = targetMaterial;
        onCompleteAction.Invoke();
        yield return null;
    }

    private void SetObjectsActive(bool isActive)
    {
        foreach (GameObject obj in objectsToHide)
        {
            obj.SetActive(isActive);
        }
    }
}