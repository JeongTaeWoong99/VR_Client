using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Video;

// 화면 공유 전용
public class SharedRoomManager : MonoBehaviourPunCallbacks
{
    public static SharedRoomManager instance;

    public GameObject[] objectsToHide;
    public Material     videoMaterial;
    public VideoPlayer  videoPlayer;
    public AudioSource  audioSource;

    private Material _skyMaterial;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _skyMaterial = RenderSettings.skybox;
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
    
    [PunRPC]  // 입장시, 상태 동기화
    public void VideoSetting(bool isPlaying, double currentTime, float currentAudioVolume)
    {
        // 세팅 넣어주기(경로(자체) / 시간 / 재생상태 / 볼륨)
        videoPlayer.url  = PlayerPrefs.GetString("videoPath"); // 동영상 위치를 정적으로 받아서, 바로 넣어주기
        videoPlayer.time = currentTime;                           // 재생 시간 동기화
        if (isPlaying) StartVideo();                              // 재생 상태 동기화
        audioSource.volume = currentAudioVolume;                  // 오디오소스 볼륨 동기화
    }
    
    [PunRPC]    // 시간 슬라이더 이동 동기화
    public void VideoTimeChange(double newTime)
    {
        videoPlayer.time = newTime;
    }
 
    [PunRPC]    // 볼륨 슬라이더 이동 동기화
    public void VolumeChange(float newVolume)
    {
        audioSource.volume = newVolume;
    }
    
}