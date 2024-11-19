using System.Collections;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;

// 미러링 + 화면 공유 공통 매니저
// 여기다가 ReturnToMainMenu같은 공통 UI가 들어감
public class IngameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static IngameManager instance;
    
    [Header("미러링/화면공유 공통")]
    public FadeCanvas fadeCanvas;
    public float      sceneTransitionTime = 2.0f;
    
    public Button     mainMenuButton;           // 메인메뉴로 돌아가기 버튼
    
    [Header("미러링")]
    public GameViewEncoder _gameViewEncoder;    // 미러링 전용

    private void Awake()
    {
        instance = this;
        
        PhotonNetwork.SendRate          = 10; // 초당 서버로 보내는 패킷 횟수 (기본값 20)
        PhotonNetwork.SerializationRate = 10; // 초당 동기화되는 데이터 횟수 (기본값 10)
    }
    
    void Start()
    {
        // 게임 중(룸 안에 들어와 있는 상태), 네트워크 연결이 끊기면, 메인 메인메뉴로 돌아가기(로비로) // 룸 -> 로비
        if(!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
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
        Debug.Log("방입장 성공");
        if(_gameViewEncoder)    // 미러링에서 CreateRoom or JoinRoom으로 성공 시, 라벨 번호 세팅
            _gameViewEncoder.label = PhotonNetwork.LocalPlayer.ActorNumber; // 엑터 넘버를 라벨 번호로 설정.
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // 이미 교육용 게임 방이 존재하는 경우
        if (returnCode == 32766)
        {
            Debug.Log("방 이미 존재 -> JoinRoom");
            PhotonNetwork.JoinRoom(PlayerPrefs.GetString("roomName")); // Join
        }
        // 그 외, 오류 표시
        else
        {
            Debug.Log(returnCode);
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
            mainMenuButton.interactable = false; // 버튼 비활성화(중복 누르기 방지)
            
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.Disconnect();                     // 서버와 연결 끊기
            
            fadeCanvas.StartFadeIn(duration);                     // 페이드인                
            yield return new WaitForSeconds(duration);  
            
            SceneManager.LoadScene("Main Menu"); // Main Menu Test에서 Main Menu으로 돌아가면, 오류.(XR 중복)
        }
    }
}
