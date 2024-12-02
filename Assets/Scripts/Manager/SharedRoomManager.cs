using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using Firebase.Storage;

// 화면 공유 전용
public class SharedRoomManager : MonoBehaviourPunCallbacks
{
    public static SharedRoomManager instance;

    private FirebaseStorage  storage;
    private StorageReference stRef;

    public GameObject      settingScreen;
    public TextMeshProUGUI settingText;

    [HideInInspector] 
    public bool isVideoSetting;

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
        // storage 세팅
        storage = FirebaseStorage.DefaultInstance;
        stRef   = storage.GetReferenceFromUrl("gs://cms-login-d93aa.appspot.com/");

        _skyMaterial = RenderSettings.skybox;
        
        settingScreen.gameObject.SetActive(true);   // 다른 방에서는 꺼져 있도록!
        settingText.gameObject.SetActive(true);     // 다른 방에서는 꺼져 있도록!
    }

    [PunRPC]
    public void StartVideo()
    {
        // 비디오 세팅이 완료된 상태에만 RPC 작동하도록 함.
        if (!isVideoSetting)
            return;
            
        Debug.Log("StartVideo");
        FadeAndSwitchVideo(videoMaterial, videoPlayer.Play);
    }

    [PunRPC]
    public void PauseVideo()
    {
        // 비디오 세팅이 완료된 상태에만 RPC 작동하도록 함.
        if (!isVideoSetting)
            return;
    
        Debug.Log("PauseVideo");
        FadeAndSwitchVideo(_skyMaterial, videoPlayer.Pause);
    }
    
    private void FadeAndSwitchVideo(Material targetMaterial, Action onCompleteAction)
    {
        foreach (GameObject obj in objectsToHide)
            obj.SetActive(targetMaterial.Equals(_skyMaterial));
        RenderSettings.skybox = targetMaterial;
        onCompleteAction.Invoke();
    }
    
    [PunRPC]    // 시간 슬라이더 이동 동기화
    public void VideoTimeChange(double newTime)
    {
        // 비디오 세팅이 완료된 상태에만 RPC 작동하도록 함.
        if (!isVideoSetting)
            return;
    
        videoPlayer.time = newTime;
    }
    
    [PunRPC]    // 볼륨 슬라이더 이동 동기화
    public void VolumeChange(float newVolume)
    {
        // 비디오 세팅이 완료된 상태에만 RPC 작동하도록 함.
        if (!isVideoSetting)
            return;
    
        audioSource.volume = newVolume;
    }

    private void PlayerStateRenewal(bool isSettingComplete)
    {
        // CMS에서 닉네임+세팅 상태+배터리 변경 보내기!
        List<Player> cmsPlayers = PhotonNetwork.PlayerListOthers.Where(C => C.CustomProperties.ContainsKey("CMS") && (bool)C.CustomProperties["CMS"]).ToList();
        if (cmsPlayers.Count > 0)
        {
            foreach (var cmsPlayer in cmsPlayers)
            {
                if (isSettingComplete)
                {
                    photonView.RPC("PlayerSettingStateRenewal", cmsPlayer, PhotonNetwork.LocalPlayer.NickName,"○",InGameManager.instance.UpdateDeviceNameAndBattery());
                    Debug.Log("CMS에게 세팅 완료 전파!");
                }
                else
                {
                    photonView.RPC("PlayerSettingStateRenewal", cmsPlayer, PhotonNetwork.LocalPlayer.NickName,"X",InGameManager.instance.UpdateDeviceNameAndBattery());
                    Debug.Log("CMS에게 세팅 중 전파!");
                }
            }
        }
    }
    
    [PunRPC]  // 입장시 + 동영상 변경 시, 비디오 존재 체크
    public void VideoExistCheck(string videoName)
    {
        PlayerStateRenewal(false); // 세팅 상태 X 전파!(동영상 변경)
        PauseVideo();                            // 동영사 멈추기!(맨처음 입장 or 동영상 변경)
        
        isVideoSetting = false;
        settingScreen.SetActive(true);
        settingText.text = "동영상을 존재를 체크하고 있습니다.";
    
        // 빌드 상태에 따라서, 각각 다른 위치에서 동영상이 있는지 체크함...
        string localFilePath;
        Debug.Log("VideoExistCheck의 " + videoName);
        // 에디터 or 윈도우 빌드
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            localFilePath      = Path.Combine(desktopPath, videoName);
        }
        // VR빌드(모바일)
        else
        {
            string downloadPath = "/storage/emulated/0/Download";
            localFilePath       = Path.Combine(downloadPath, videoName);
        }
        
        // 파일 존재 O -> 비디오 세팅
        if (File.Exists(localFilePath))
        {
            Debug.Log("존재 O");
            VideoSetting(localFilePath,PhotonNetwork.LocalPlayer);
        }
        // 파일 존재 X  -> 비디오 다운로드 -> 비디오 세팅
        else
        {
            Debug.Log("존재 X");
            StartCoroutine(VideoDownload(localFilePath));
        }
    }
    
    private void VideoSetting(string videoPath,Player localPlayer)
    {
        // 동영상 위치 바로 넣어주기
        videoPlayer.url  = videoPath;
        
        // VideoSettingRequest를 cms에게 보내기!
        // 룸에 접속해 있는 CMS 시스템의 리스트를 받기.
        List<Player> cmsPlayers = PhotonNetwork.PlayerListOthers.Where(C => C.CustomProperties.ContainsKey("CMS") && (bool)C.CustomProperties["CMS"]).ToList();
        if (cmsPlayers.Count > 0)
        {
            foreach (var cmsPlayer in cmsPlayers)
            {
                // VideoSettingRequest을 요청하는 RPC를 cms에게 보내기(Player 정보를 매개 변수로! 리턴을 받아야 하니)
                photonView.RPC("VideoSettingRequest", cmsPlayer, localPlayer);
                Debug.Log("요청");
            }
        }
        
        /////////////////////////////////////////////
        // VideoSettingRequest(클라에서 CMS) -> VideoSettingAccept(CMS에서 클라)
        /////////////////////////////////////////////
    }
    
    [PunRPC]  // 상태 동기화 (클라 <- CMS)
    public void VideoSettingAccept(bool isPlaying, double currentTime, float currentAudioVolume)
    {
        isVideoSetting   = true;
        Debug.Log("수락");
        
        // 세팅 넣어주기(시간 / 재생상태 / 볼륨)
        videoPlayer.time = currentTime;                           // 재생 시간 동기화
        if (isPlaying) StartVideo();                              // 재생 상태 동기화
        audioSource.volume = currentAudioVolume;                  // 오디오소스 볼륨 동기화
        
        settingScreen.SetActive(false);
        settingText.text = "동영상 세팅이 완료되었습니다.";

        PlayerStateRenewal(true);   // CMS에게 완료된 상태를 알려주기!
    }
    
    // 비디오 다운로드(경로에 없는 경우)
    private IEnumerator VideoDownload(string videoPath)
    {
        settingText.text = "동영상을 다운받고 있습니다.";
        
        // // 저장위치
        // string downloadPath;
        // // 빌드 상태에 따라서, 각각 다른 위치에서 동영상이 있는지 체크함...
        // // string localFilePath;
        //
        // // 에디터 or 윈도우 빌드
        // if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        // {
        //     downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), videoName); // 바탕화면에 저장
        //     // string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //     // localFilePath      = Path.Combine(desktopPath, videoName);
        // }
        // // VR빌드(모바일)
        // else
        // {
        //     downloadPath = Path.Combine("/storage/emulated/0/Download", videoName);                               // Download 폴더에 저장
        //     // string downloadPath = "/storage/emulated/0/Download";
        //     // localFilePath       = Path.Combine(downloadPath, videoName);
        // }
        
        // 빌드 상태에 따라서, 각각 다른 위치에서 동영상이 있는지 체크함...
        
        string localFilePath;
        string videoName = Path.GetFileName(videoPath);
        Debug.Log("VideoDownload의 " + videoName);
        // 에디터 or 윈도우 빌드
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            localFilePath      = Path.Combine(desktopPath, videoName);
        }
        // VR빌드(모바일)
        else
        {
            string downloadPath = "/storage/emulated/0/Download";
            localFilePath       = Path.Combine(downloadPath, videoName);
        }
        
        StorageReference videoRef = stRef.Child(videoName); // 저장소 참조 위치(Path.GetFileName를 이용하여, videoName에서 경로 부분을 제거함.)
        Task downloadTask         = videoRef.GetFileAsync(localFilePath);     // 비동기 Task 실행
        
        yield return new WaitUntil(() => downloadTask.IsCompleted);
        
        // 실패 -> 강제로 메뉴로 복귀
        if (downloadTask.IsFaulted || downloadTask.IsCanceled)
        {
            settingText.text = downloadTask.Exception + " : 에러 발생";
            settingScreen.SetActive(false);
            InGameManager.instance.OnReturnToMainMenu();    // 강제로 바탕화면으로 복귀
        }
        // 다운 성공 -> VideoSetting -> VideoSettingAccept 순서대로 똑같이 실행하면 됨!
        else
        {
            VideoSetting(videoPath,PhotonNetwork.LocalPlayer);
        }
    }
}