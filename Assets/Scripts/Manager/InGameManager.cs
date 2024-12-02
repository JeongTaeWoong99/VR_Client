using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// 미러링 + 화면 공유 (공통 매니저)
// 여기다가 ReturnToMainMenu같은 공통 UI가 들어감
public class InGameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static InGameManager instance;
    
    [Header("미러링/화면공유 공통")]
    public FadeCanvas fadeCanvas;
    public float      sceneTransitionTime = 2.0f;
    
    private Button returnToMainMenuButton;          // 메인메뉴로 돌아가기 버튼

    [Header("미러링")]
    public GameViewEncoder _gameViewEncoder;    // 미러링 전용

    private void Awake()
    {
        instance = this;
        
        // 교육생 구분, 해쉬분 헤쉬테이블 추가
        Hashtable playerProperties = new Hashtable { { "Trainee", PlayerPrefs.GetString("playerName") } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        
        // 씬네임 구분, 헤쉬테이블 추가
        Hashtable sceneNameProperties = new Hashtable { { "SceneName", SceneManager.GetActiveScene().name } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(sceneNameProperties);
    }
    
    void Start()
    {
        // 게임 중(룸 안에 들어와 있는 상태), 네트워크 연결이 끊기면, 메인 메인메뉴로 돌아가기(로비로) // 룸 -> 로비
        if(!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        
        // ReturnToMainMenu Button 이름을 가진 오브젝트를 찾음
        GameObject buttonObject = GameObject.Find("ReturnToMainMenu Button");
        if (buttonObject != null)
        {
            returnToMainMenuButton = buttonObject.GetComponent<Button>();
            if (returnToMainMenuButton != null)
            {
                // OnReturnToMainMenu를 버튼 클릭 이벤트에 연결
                returnToMainMenuButton.onClick.AddListener(OnReturnToMainMenu);
                Debug.Log("ReturnToMainMenuButton 이벤트가 성공적으로 연결되었습니다.");
            }
            else
                Debug.LogError("Button 컴포넌트를 찾을 수 없습니다.");
        }
        else
            Debug.LogError("'ReturnToMainMenu Button'이라는 이름의 오브젝트를 찾을 수 없습니다.");
        
        
    }
    
    public void OnEvent(EventData photonEvent)
    {
        
    }
    
    // 활성화 될 때마다 호출
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    
    // 활성화 될 때마다 호출
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    
    // 방에 입장 완료시 호출(CreateRoom하고 난 후 or 만들어져 있는 방을 룸버튼 클릭하여, 들어가면)
    public override void OnJoinedRoom()
    {
        // 미러링 방
        // 미러링에서 CreateRoom or JoinRoom으로 성공 시, 라벨 번호 세팅
        if(_gameViewEncoder) 
            _gameViewEncoder.label = PhotonNetwork.LocalPlayer.ActorNumber; // 엑터 넘버를 라벨 번호로 설정.
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // 이미 교육용 게임 방이 존재하는 경우
        if (returnCode == 32766)
            PhotonNetwork.JoinRoom(PlayerPrefs.GetString("roomName")); // JoinRoom // (roomName == "VR Game")
        // 그 외, 오류 표시
        else
            Debug.Log(returnCode);
    }
    
    // 다른 플레이어가 방에서 나갈시 호출
    public override void OnPlayerLeftRoom(Player leftPlayer)
    {
        // 화면 공유방에서 나간 경우... 누군가 나간 경우
        if (PhotonNetwork.CurrentRoom.Name != "VR Game")
        {
            // RPC로 OnReturnToMainMenu가 전달되지 않아서, 리턴 버튼을 누를 수 잇는 경우...!
            if (returnToMainMenuButton.interactable)
            {
                // CMS가 남아있는지 확인!!
                // 나간 사람이 CMS일 경우!!
                List<Player> cmsPlayerList = PhotonNetwork.PlayerListOthers.Where(C => C.CustomProperties.ContainsKey("CMS") && (bool)C.CustomProperties["CMS"]).ToList();
                if (cmsPlayerList.Count == 0)
                {
                    StartCoroutine(ReturnToMainMenu(sceneTransitionTime));
                }
            }
        }
    }
    
    [PunRPC] // 버튼으로 사용 + CMS RPC로도 사용
    public void OnReturnToMainMenu()
    {
        StartCoroutine(ReturnToMainMenu(sceneTransitionTime));
    }
    
    public IEnumerator ReturnToMainMenu(float duration)
    {
        if (PhotonNetwork.InRoom)
        {
            returnToMainMenuButton.interactable = false; // 버튼 비활성화(중복 누르기 방지)
            
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.Disconnect();                     // 서버와 연결 끊기
            
            fadeCanvas.StartFadeIn(duration);                     // 페이드인                
            yield return new WaitForSeconds(duration);  
            
            SceneManager.LoadScene("Main Menu");
        }
    }
    
    public int UpdateDeviceNameAndBattery()
    {
        string deviceName = GetDeviceName();
        
        // '메타퀘스트 오큘러스 퀘스트'인 경우
        if (deviceName.Contains("Oculus"))
        {
            // Oculus 전용 API를 사용하여 배터리 정보 가져오기
            float batteryLevel      = SystemInfo.batteryLevel; // 배터리 수준 (0.0 ~ 1.0)
            int   batteryPercentage = Mathf.RoundToInt(batteryLevel * 100);
            
            Debug.Log($"배터리 : {batteryPercentage}%");
            return batteryPercentage;
        }
        
        // 에디터 빌드 or 지원하지 않는 디바이스
        Debug.Log(deviceName + " or 지원하지 않는 디바이스입니다.");
        return 0;
    }
    
    private string GetDeviceName()
    {
        // OVRPlugin에서 디바이스 이름 가져오기(게임 빌드 상태에서만 리턴함...)
        if (OVRPlugin.productName != null)
            return OVRPlugin.productName;
        
        // 에디터 빌드 상태인 경우
        return "에디터 빌드";
    }
}