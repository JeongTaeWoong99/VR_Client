using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// 로비
public class PunSystem : MonoBehaviourPunCallbacks
{
    public static PunSystem instance;
    
    [Header("로딩")]
    public GameObject loadingScreen;
    
    [Header("피드백")]
    public TextMeshProUGUI feedbackText;        // 버튼 클릭 시, 파이어베이스 피드백 텍스트

    [Header("로그인")]
    public GameObject     nameInputScreen;
    public TMP_InputField nameInput;
    public static bool    hasSetNick; // ☆ 정적 bool (게임을 끝내고 돌아와서도, true상태로 남아있음)
    
    [Header("메뉴")]
    public GameObject menuButtons;
    
    [Header("방선택")]
    public GameObject selectRoomScreen;
    public Text       selectedRoomName;
    
    [Header("공유룸")]
    public  GameObject              roomBrowserScreen;
    public  GameObject              sharedRoomPrefabs; // RoomButton 스크립트 타입의 변수
    public  GameObject              sharedRoomGroup;   // 생성위치
    private List<SharedRoomButton>  allRoomButtons = new List<SharedRoomButton>();
    
    private void Awake()
    {
        instance = this;
        
        PhotonNetwork.SendRate          = 30; // 초당 서버로 보내는 패킷 횟수 (기본값 20)
        PhotonNetwork.SerializationRate = 30; // 초당 동기화되는 데이터 횟수 (기본값 10)
    }
    
    void Start()
    {
        CloseMenus();
        loadingScreen.SetActive(true);
        feedbackText.gameObject.SetActive(false);

        if (!PhotonNetwork.IsConnected) // 게임화면에서, 다시 메인메뉴로 돌아와서, 설정세팅을 하는 경우 방지
        {
            PhotonNetwork.ConnectUsingSettings(); // PhotonServerSettings 파일의 설정들로 네트워킹을 세팅한다.
        }                                         // 네트워크가 정상적으로 접속되면, OnConnectedToMaster() 함수가 호출된다;
    }

    void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        selectRoomScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
        nameInputScreen.SetActive(false);
    }

    // 서버접속 완료시 호출
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(); // 로비입장

        // 방을 처음 만든 사람이 마스터, 이후 마스터가 방을 나가면, 남아있는 렌덤한 사람에게 마스터 권한이 간다.
        // 마스터가 PhotonNetwork.LoadLevel()을 호출하면, 모든 플레이어가 동일한 레벨을 자동으로 로드(true면 로드 , false면 로드 x) -> StartGame버튼에서 로드레벨 사용
        //PhotonNetwork.AutomaticallySyncScene = true;
    }
    //서버접속 실패시 재시도
    public override void OnDisconnected(DisconnectCause cause) 
    {
        // 마스터 서버로의 재접속 시도
        PhotonNetwork.ConnectUsingSettings();
    }

    // 로비입장 완료시 호출
    public override void OnJoinedLobby()
    {
        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        // 첫 게임접속 후 로비화면
        if (!hasSetNick)
        {
            CloseMenus();
            nameInputScreen.SetActive(true);

            if (PlayerPrefs.HasKey("playerName"))
            {
                nameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        else
        {
            CloseMenus();
            menuButtons.SetActive(true);
            feedbackText.gameObject.SetActive(true);
        
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }

    // 버튼 함수
    public void OpenSelectRoom()
    {
        CloseMenus();
        selectRoomScreen.SetActive(true);
        feedbackText.gameObject.SetActive(false);   // 피드백 텍스트 안보이게 하기
    }
    
    public void SelectRoom()
    {
        CloseMenus();
        loadingScreen.SetActive(true);
    
        RoomOptions options = new RoomOptions();
        options.MaxPlayers  = 20;
        
        PhotonNetwork.CreateRoom(selectedRoomName.text,options); // 방생성 및 설정된 옵션 전달
    }
    
    // // 방에 입장 완료시 호출(CreateRoom하고 난 후 or 만들어져 있는 방을 룸버튼 클릭하여, 들어가면)
    public override void OnJoinedRoom()
    {
        // 1인 교육용 게임 입장
        if (PhotonNetwork.CurrentRoom.Name == "VR Game")
        {
            SceneManager.LoadScene("Space"); // 게임 바로 시작(방 만들어지고, 바로 시작 + 룸 입장하고 바로 시작)
        }
        // 화면 공유 방 입장
        // else
        // {
        //     PhotonNetwork.LoadLevel("360VideoScene");
        // }
    }
    
    // 방생성이 실패하면 호출(실패 코드와 메세지설명을 받을 수 있음)
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // 이미 교육용 게임 방이 존재하는 경우
        if (returnCode == 32766)
        {
            PhotonNetwork.JoinRoom(selectedRoomName.text); // 방에 입장.
        }
        // 그 외, 오류 표시
        else
        {
            Debug.Log(returnCode);
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "오류코드 : " + returnCode;
            CloseMenus();
        }
    }
    
    // 버튼함수
    // 만든 방 삭제 및 삭제가 완료되면, Lobby로 다시 접속(Room -> Lobby)
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        loadingScreen.SetActive(true);
    }
    
    // 접속한 방을 떠나면, 호출됨.
    public override void OnLeftRoom()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    // 버튼 함수
    public void OpenRoomBrowser()
    {
        CloseMenus();
        roomBrowserScreen.SetActive(true);
    }

    // 버튼 함수(공용) 
    // 닫기(메뉴로 돌아가기)
    public void ClosePanel()
    {
        CloseMenus();
        menuButtons.SetActive(true);
        feedbackText.gameObject.SetActive(true);   // 피드백 텍스트 안보이게 하기
        feedbackText.text = "접속을 환영합니다.";
    }

    // 룸 리스트 초기화 - 현재 생성된 룸들의 정보가 담긴 리스트가 매개변수로 온다.
    // 로비 내에 룸이 생성되거나 사라질때 자동 호출되는 콜백
    public override void OnRoomListUpdate(List<RoomInfo> roomList) // 자동업데이트
    {
        foreach(SharedRoomButton rb in allRoomButtons)  // 기존 정보 모두 삭제
            Destroy(rb.gameObject);
        allRoomButtons.Clear();
        
        foreach (var roomLists in roomList)
        {   
            // 미러링은 방은 검색 안되도록 하기...
            if(roomLists.Name != "VR Game" && roomLists.PlayerCount != roomLists.MaxPlayers && !roomLists.RemovedFromList && roomLists.IsVisible)
            {
                GameObject        newClone   = Instantiate(sharedRoomPrefabs, sharedRoomGroup.transform);
                SharedRoomButton newButton = newClone.GetComponent<SharedRoomButton>();
                
                newButton.SettingRoomPanel(roomLists);
                newButton.gameObject.SetActive(true);
                allRoomButtons.Add(newButton);
            }
        }
    }
    
    // 버튼 함수
    public void SetNickname()
    {
        if(!string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = nameInput.text;

            PlayerPrefs.SetString("playerName", nameInput.text);

            CloseMenus();
            menuButtons.SetActive(true);
            feedbackText.gameObject.SetActive(true);

            hasSetNick = true;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
