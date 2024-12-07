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
using Hashtable = ExitGames.Client.Photon.Hashtable;

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
        
        Application.targetFrameRate = 60; // 게임 프레임 고정
        
        PhotonNetwork.SendRate          = 60;
        PhotonNetwork.SerializationRate = 60;
        
        #if UNITY_EDITOR
                PlayerPrefs.SetString("playerName", "Editor");
        #endif
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

        // 첫 게임접속 후, 로비화면이면서, 닉네임이 설정되어 있지 않은 경우.
        // 한번 설정하면, 삭제 전까지 쭉~ 고정...
        if (!hasSetNick && string.IsNullOrEmpty(PlayerPrefs.GetString("playerName")))
        {
            CloseMenus();
            nameInputScreen.SetActive(true);

            if (PlayerPrefs.HasKey("playerName"))
            {
                nameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        // 닉네임이 설정되어 있는 경우
        else
        {
            CloseMenus();
            menuButtons.SetActive(true);
            feedbackText.gameObject.SetActive(true);
        
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
            // 기존 닉네임으로 해쉬값 설정
            Hashtable playerProperties = new Hashtable { { "Trainee", PlayerPrefs.GetString("playerName") } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
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
        
        PlayerPrefs.SetString("roomName", "VR Game");   // 교육 게임은 모두, "VR Game"이름의 룸으로 들어가도록 함.
        SceneManager.LoadScene(selectedRoomName.text);  // 씬은 각자의 게임 씬으로 이동.
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
        foreach (var roomLists in roomList)
        {
            if(roomLists.Name != "VR Game")
            {
                // 삭제 호출 -> 방 프리팹 삭제
                if (roomLists.RemovedFromList)
                {
                    //Debug.Log(roomLists.Name + " 방 삭제");
                    // 그룹에서 텍스트이름 비교하여, 해당 방 프리팹 삭제
                    foreach (SharedRoomButton rb in allRoomButtons)
                    {
                        if(rb.videoNameText.text + ".mp4$" + rb.makeID_Text.text == roomLists.Name)
                            Destroy(rb.gameObject);
                    }
                }
                // 방 생성 호출 -> 방 프리팹 생성
                else
                {
                    // Debug.Log(roomLists.Name + " | " + (roomLists.PlayerCount != roomLists.MaxPlayers) + " | " + !roomLists.RemovedFromList + " | " + roomLists.IsVisible);
                    // 미러링은 방은 검색 안되도록 하기...
                    if(roomLists.PlayerCount != roomLists.MaxPlayers && !roomLists.RemovedFromList && roomLists.IsVisible)
                    {
                        //Debug.Log(roomLists.Name + " CMS 방 생성");
                        GameObject       newClone  = Instantiate(sharedRoomPrefabs, sharedRoomGroup.transform);
                        SharedRoomButton newButton = newClone.GetComponent<SharedRoomButton>();
                        
                        newButton.SettingRoomPanel(roomLists);
                        newButton.gameObject.SetActive(true);
                        allRoomButtons.Add(newButton);
                    }
                }
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
            
            // 교육생 구분, 접속 플레이어 구분 헤쉬테이블 추가(닉네임 설정하고, 해쉬값 설정되어야 함.)
            Hashtable playerProperties = new Hashtable { { "Trainee", PlayerPrefs.GetString("playerName") } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
