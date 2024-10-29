using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

// 화면 공유 전용
public class SharedRoomManager : MonoBehaviourPunCallbacks
{
    public Button mainMenuButton; // 메인메뉴로 돌아가기 버튼

    public GameObject[] objectsToHide;
    public FadeCanvas   fadeCanvas;
    public Material     videoMaterial;
    public VideoPlayer  videoPlayer;
    public float        fadeDuration = 1.0f;

    private Material _skyMaterial;
    
    private void Start()
    {
        _skyMaterial = RenderSettings.skybox;
        
        // 세팅 쭉~~ 넣어주기(시간 / 재생상태 등등)
        videoPlayer.url = PlayerPrefs.GetString("videoPath"); // 동영상 위치를 정적으로 받아서, 바로 넣어주기
    }

    [PunRPC]
    public void StartVideo()
    {
        FadeAndSwitchVideo(videoMaterial, videoPlayer.Play);
    }

    [PunRPC]
    public void PauseVideo()
    {
        FadeAndSwitchVideo(_skyMaterial, videoPlayer.Pause);
    }
    
    private void FadeAndSwitchVideo(Material targetMaterial, Action onCompleteAction)
    {
        foreach (GameObject obj in objectsToHide)
            obj.SetActive(targetMaterial.Equals(_skyMaterial));
        RenderSettings.skybox = targetMaterial;
        onCompleteAction.Invoke();
    }

    [PunRPC] // 버튼으로 사용 + CMS RPC로도 사용
    public void OnReturnToMainMenu()
    {
        StartCoroutine(ReturnToMainMenu());
    }
    
    private IEnumerator ReturnToMainMenu()
    {
        if (PhotonNetwork.InRoom)
        {
            mainMenuButton.interactable = false; // 버튼 비활성화(중복 누르기 방지)
            
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.Disconnect();                     // 서버와 연결 끊기
            
            fadeCanvas.QuickFadeIn();                       // 페이드인                
            yield return new WaitForSeconds(fadeDuration);  
            
            SceneManager.LoadScene("Main Menu"); // Main Menu Test에서 Main Menu으로 돌아가면, 오류.(XR 중복)
        }
    }
}