using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

// IOnEventCallback  포톤에서 발생하는 모든 이벤트의 정보를 받을 수 있음.
public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager instance;
    
    public  List<PlayerInfo> allPlayers = new List<PlayerInfo>(); // 플레이어 정보 원본 리스트
    private int index;                                            // 자신의 actor 넘버

    private List<LeaderboardPlayer> lboardPlayers = new List<LeaderboardPlayer>(); // LeaderboardPlayer타입 리스트(내림차순으로 정렬을 위해 필요->gameobject,text등 참조가능)

    public int       killsToWin      = 3;                 // 승리 킬수
    public Transform mapCamPoint;
    public GameState state           = GameState.Waiting; // 처음 state의 상태는 Waiting
    public float     waitAfterEnding = 5f;                // 게임 나가지는 시간

    public bool perpetual; // 다음게임 참가여부(true 참 / false 불참)

    public  float matchLength = 180f;  // 매치 제한시간(초)
    private float currentMatchTime;    // 현재시간
    private float sendTimer;           // 늦게 합류한 플레이어에게 시간 전달
    
    public enum EventCodes : byte
    {
        // EventCodes 바이트 제한 199
        NewPlayer   = 0, 
        ListPlayers = 1, 
        UpdateStat  = 2, 
        NextMatch   = 3, 
        TimerSync   = 4
    }
    
    public enum GameState
    {
        Waiting, Playing, Ending
    }
    private void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        // 게임 중(룸 안에 들어와 있느 상태), 네트워크 연결이 끊기면, 메인 메인메뉴로 돌아가기(로비로) // 룸 -> 로비
        if(!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName); // 닉네임으로 플레이어 
            state = GameState.Playing;             // 상태 변경
            SetupTimer();                          // 타이머 작동
            
            // 마스터 클라이언트에게 시간정보를 받고, 타이머가 켜지도록
            if(!PhotonNetwork.IsMasterClient)
            {
                UIController.instance.timerText.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        // Tab
        if(Input.GetKeyDown(KeyCode.Tab) && state != GameState.Ending)
        {
            if(UIController.instance.leaderboard.activeInHierarchy)
            {
                UIController.instance.leaderboard.SetActive(false);
            } else
            {
                ShowLeaderboard();
            }
        }

        // 시간갱신 (마스터 클라이언트가 제어)
        if (PhotonNetwork.IsMasterClient)
        {
            if (currentMatchTime > 0f && state == GameState.Playing)
            {
                currentMatchTime -= Time.deltaTime;
        
                if (currentMatchTime <= 0f)
                {
                    currentMatchTime = 0f;
        
                    state = GameState.Ending;
                    
                    ListPlayersSend();
        
                    StateCheck();
                }
        
                UpdateTimerDisplay();        // 맨아래
        
                sendTimer -= Time.deltaTime; // 시간을 빼주고
                if(sendTimer <= 0)           // 0아래로 내려가면
                {
                    sendTimer += 1f;         // 다시 1을 더해주고
                    TimerSend();             // 그다음 마스터 클라이언트가 현재시간을 갱신해줌.(1초 단위로)
                }
            }
        }
    }

    // using ExitGames.Client.Photon;의 변수타입 EventData
    // -> Photon 이벤트는 코드 값과 이벤트 내용(있는 경우)이 포함된 매개 변수 사전으로 구성됩니다.
    // -> .sender 와 .Code로 수신자와 이벤트 타입을 알 수 있고, .CustomData로 오브젝트를 얻을 수 있다.(다른 함수도 있음)
    // IOnEventCallback 내장함수 OnEvent
    // -> 들어오는 모든 이벤트에 대해 호출됩니다.
    // -> 즉, room에서 발생하는 모든 이벤트에 대해서 호출된다.(RaiseEvent 함수 : Sends fully customizable events in a room.)
    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code < 200)                               // eventCode가 0 1 2 4 8 ~~ 128 byte까지이다. (256 안됨)
        {                                                        // RaiseEvent의 eventCode자리의 byte값 제한이 199까지이기 때문이다. -> Sends fully customizable events in a room. Events consist of at least an EventCode (0..199) and can have content.
            EventCodes theEvent = (EventCodes)photonEvent.Code;  // 타입 맞추기(이벤트 판단)
            object[] data = (object[])photonEvent.CustomData;    // 타입 맞추기(정보 저장)
            
            // Receive 판단
            switch(theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayersReceive(data);
                    break;
                case EventCodes.UpdateStat:
                    UpdateStatsReceive(data);
                    break;
                case EventCodes.NextMatch:
                    NextMatchReceive(); // 매개변수 X
                    break;
                case EventCodes.TimerSync:
                    TimerReceive(data);
                    break;
            }
        }
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

    // 본인 클라이언트에서 실행(본인 입장 하면서 실행)
    public void NewPlayerSend(string username)
    {
        object[] package = new object[4];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber; // 룸 입장시 할당되는 고유번호
        package[2] = 0;
        package[3] = 0;
        // RaiseEvent -> OnEvent를 발생시키는 함수 -> room에 들어와 있는 모든 클라이언트들의 OnEvent 발동(Sends fully customizable events in a room.)
        // eventCode 자리          : byte 타입으로, 199 바이트까지 가능하며, 어떤 종류의 이벤트인지 판별
        // eventContent 자리       : object타입으로, CustomData를 호출하여, 꺼내어 사용
        // raiseEventOptions 자리  : 정보를 보낼 그룹(즉, OnEvent를 호출할 클라이언트 -> 새로운 플레이어가 들어온 함수는, 모든 그룹에 보내릴요 없이 마스터 클라이언트에게만 보내고, 다음단계로 넘어감)
        // sendOptions 자리        : 정보를 신뢰할 수 있는지
        
        PhotonNetwork.RaiseEvent((byte)EventCodes.NewPlayer, package, new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, new SendOptions { Reliability = true });
    }
    
    // NewPlayerSend의 RaiseEvent에 의해 OnEvent에서 실행(본인 입장 + 다른 사람 입장시 실행을 마스터 클라이언트만!!)
    public void NewPlayerReceive(object[] dataReceived)
    {
        PlayerInfo player = new PlayerInfo((string)dataReceived[0], (int)dataReceived[1], (int)dataReceived[2], (int)dataReceived[3]);

        allPlayers.Add(player);

        ListPlayersSend(); // 마스터 클라이언트가 모든 리스트를 모든 클라이언트에게 보내준다.
    }

    // OnEvent에서 NewPlayerReceive통해, 새로운 플레이어들의 정보들이 리스트에 저장이 되고, 리스트의 정보를 보냄.
    public void ListPlayersSend()
    {
        object[] package = new object[allPlayers.Count + 1];
        package[0]       = state; // package 0번은 게임상태에 대한 정보를 담고있다.      

        for(int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i + 1] = piece;  // piece의 정보들을 package[1] ~ package[n+1]까지에 저장
                                      // package[0]번은 게임상태에 대한 정보
        }

        // 모든 클라이언트의 OnEvent 호출(리스트 갱신)
        // eventCode         자리 : ListPlayers
        // eventContent      자리 : package 
        // raiseEventOptions 자리 : All☆
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListPlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void ListPlayersReceive(object[] dataReceived)
    {
        allPlayers.Clear();

        state = (GameState)dataReceived[0];           // ListPlayerSend()에서 package[0]은 게임상태에 대한 정보를 담고있다.

        for(int i = 1; i < dataReceived.Length; i++)  // 플레이어의 정보들을 담고있는, package[1]부터 시작한다.
        {
            object[] piece = (object[])dataReceived[i];

            PlayerInfo player = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
                );

            allPlayers.Add(player);

            // 자신의 인덱스 저장(package 1번부터 시작하기 때문에 i-1 해줘야 함)
            if(PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i - 1;
            }
        }

        StateCheck();  // GameState체크
    }

    // 각각 Shoot함수 Die함수에서 실행되어 진다.
    public void UpdateStatsSend(int actorSending, int statToUpdate, int amountToChange)
    {
                                        // 상호작용 할 사람 // 킬 or 데쓰  // 증감양      
        object[] package = new object[] { actorSending, statToUpdate, amountToChange };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdateStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void UpdateStatsReceive(object[] dataReceived)
    {
        int actor    = (int)dataReceived[0]; // actorSending   = 이름(상호작용 할 사람)
        int statType = (int)dataReceived[1]; // stateToUpdate  = kill or death
        int amount   = (int)dataReceived[2]; // amountToChange = 1(고정)

        // kill death가 바뀐 actor를 찾고
        for(int i = 0; i < allPlayers.Count; i++)
        {
            // 바뀐 엑터를 찾으면, 킬뎃 증감 이벤트 스테이트타입에 따라서, switch하고
            if(allPlayers[i].actor == actor)
            {
                switch(statType)
                {
                    case 0: //kills
                        allPlayers[i].kills += amount;
                        Debug.Log("Player " + allPlayers[i].name + " : kills " + allPlayers[i].kills);
                        break;

                    case 1: //deaths
                        allPlayers[i].deaths += amount;
                        Debug.Log("Player " + allPlayers[i].name + " : deaths " + allPlayers[i].deaths);
                        break;
                }

                // 자신의 인덱스면 업데이트(UpdateStatesDisplay();만 해도 상관없을거 같긴 함)
                if(i == index)
                {
                    UpdateStatsDisplay();
                }

                // 리더보드가 켜져있는 상태면, 킬뎃이 바뀐 통신을 했을 때
                if(UIController.instance.leaderboard.activeInHierarchy)
                {
                    ShowLeaderboard();
                }

                break;
            }
        }

        ScoreCheck();
    }

    public void UpdateStatsDisplay()
    {
        if (allPlayers.Count > index) // 안전장치 (정리가 되지 않고, 호출시 생기는 오류)
        {

            UIController.instance.killsText.text = "Kills: " + allPlayers[index].kills;
            UIController.instance.deathsText.text = "Deaths: " + allPlayers[index].deaths;
        }
        else
        {
            UIController.instance.killsText.text = "Kills: 0";
            UIController.instance.deathsText.text = "Deaths: 0";
        }
    }

    void ShowLeaderboard()
    {
        UIController.instance.leaderboard.SetActive(true);

        foreach(LeaderboardPlayer lp in lboardPlayers)
        {
            Destroy(lp.gameObject);
        }
        lboardPlayers.Clear();

        UIController.instance.leaderboardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = SortPlayers(allPlayers);

        foreach(PlayerInfo player in sorted)
        {
            LeaderboardPlayer newPlayerDisplay = Instantiate(UIController.instance.leaderboardPlayerDisplay, UIController.instance.leaderboardPlayerDisplay.transform.parent);

            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths);

            newPlayerDisplay.gameObject.SetActive(true);

            lboardPlayers.Add(newPlayerDisplay);
        }
    }

    // 리더보드를 킬 내림차순 정렬하는 함수
    private List<PlayerInfo> SortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();   // players리스트가 정렬되어 넣어질 리스트

        while(sorted.Count < players.Count)
        {
            int        highest        = -1;         // 최고킬수 초기화        
            PlayerInfo selectedPlayer = players[0]; // 선택된 플레이어 초기화

            // players만큼 반복해서, 정렬수행
            foreach(PlayerInfo player in players)
            {
                if (!sorted.Contains(player))       // 정렬된 리스트에 들어가 있지 있지않고,
                {
                    if (player.kills > highest)     // 현재 highest 킬보다 값이 높으면,
                    {
                        selectedPlayer = player;    // 플레이어 선택됨.
                        highest = player.kills;     // 킬값 선택됨.
                    }
                }
            }

            sorted.Add(selectedPlayer); // 3명중 최고킬 선택됨. ADD -> 2명중 최고킬 선택됨. ADD -> 끝
        }

        return sorted; // 정렬된 리스트 리턴
    }

    // Room을 나갈 때, 호출
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        SceneManager.LoadScene(0);
    }

    void ScoreCheck()
    {
        bool winnerFound = false;
    
        foreach(PlayerInfo player in allPlayers)
        {
            if(player.kills >= killsToWin && killsToWin > 0)
            {
                winnerFound = true;
                break;
            }
        }
    
        if(winnerFound)
        {
            if(PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                state = GameState.Ending;
                ListPlayersSend();  // 끝나는 시점의 플레이어의 리스트(이름/킬/뎃) 정보를 다시, 모든 플레이어에게 보내 최신정보를 갱신한다.
            }
        }
    }

    void StateCheck()
    {
        if(state == GameState.Ending)
        {
            EndGame();
        }
    }

    // 마스터 클라이언트가, 모든 클라이언트들의 PhotonNetwork.Instantiate()로 만들어진, GameObject들을 없앰.
    void EndGame()
    {
        state = GameState.Ending;

        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }

        UIController.instance.endScreen.SetActive(true);
        ShowLeaderboard();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera.main.transform.position = mapCamPoint.position;
        Camera.main.transform.rotation = mapCamPoint.rotation;

        StartCoroutine(EndCo());
    }

    private IEnumerator EndCo()
    {
        yield return new WaitForSeconds(waitAfterEnding);
    
        // 계속진행 false
         if (!perpetual)
         {
             // 화면 동기화 X -> 각자의 타이밍에 의해서 나가기
             // (PhotonNetwork.LoadLevel(씬이름);를 이용해서,
             // 모든 ROOM에 들어와있는 클라이언트들의 씬을 마스터 클라이언트에 의해서 동기화 할지 말지)
             PhotonNetwork.AutomaticallySyncScene = false;
             // ROOM 강제로 나가기 -> OnLeftRoom() 호출
             PhotonNetwork.LeaveRoom();
         }
         // 계속진행 true 
         else
        {
            if(PhotonNetwork.IsMasterClient)
            {
                // 같은 맵 진행(true 다른맵 가능, false 같은맵만 진행)
                if (!Launcher.instance.changeMapBetweenRounds)
                {
                    NextMatchSend();
                }
                // 다른 맵 진행 
                else
                {
                    int newLevel = Random.Range(0, Launcher.instance.allMaps.Length);
    
                    // 새로운 match맵의 이름과, 현재씬의 이름이 같으면, NextMatchSend(); 과정을 거쳐야 함.
                    if(Launcher.instance.allMaps[newLevel] == SceneManager.GetActiveScene().name)
                    {
                        NextMatchSend();
                    }
                    // 하지만, 새로운 메치의 맵이 다르면, 그 sceen을 로드하면 된다. 
                    else
                    {
                        PhotonNetwork.LoadLevel(Launcher.instance.allMaps[newLevel]);
                    }
                }
            }
        }
    }

    public void NextMatchSend()
    {
        // eventCode : NextMatch // ecentContent : null ---> 이벤트 타입만 보내면됨. 보낼 데이터는 없음.
        PhotonNetwork.RaiseEvent((byte)EventCodes.NextMatch, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
    }

    // 다음 경기 정보 설정
    public void NextMatchReceive()
    {
        state = GameState.Playing;                // 상태 변경
    
        UIController.instance.endScreen.SetActive(false);
        UIController.instance.leaderboard.SetActive(false);
    
        foreach (PlayerInfo player in allPlayers) // 킬뎃 초기화
        {
            player.kills = 0;
            player.deaths = 0;
        }
    
        UpdateStatsDisplay();                     // 기본화면 킬뎃을 초기화된 킬뎃으로 한번 다시 뽑기
        PlayerSpawner.instance.SpawnPlayer();     // 플레이어 다시 스폰
        SetupTimer();                             // 타이머 재설정
    }

    public void SetupTimer()
    {
        if(matchLength > 0)
        {
            currentMatchTime = matchLength;
            UpdateTimerDisplay();
        }
    }

    public void UpdateTimerDisplay()
    {
        var timeToDisplay = System.TimeSpan.FromSeconds(currentMatchTime);
    
        UIController.instance.timerText.text = timeToDisplay.Minutes.ToString("00") + ":" + timeToDisplay.Seconds.ToString("00");
    }

    public void TimerSend()
    {
        object[] package = new object[] { (int)currentMatchTime, state };   // 0번 게임상태, 1번 참가햇을 때 시간
    
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.TimerSync,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void TimerReceive(object[] dataReceived)
    {
        currentMatchTime = (int)dataReceived[0];
        state = (GameState)dataReceived[1];
    
        UpdateTimerDisplay();
    
        UIController.instance.timerText.gameObject.SetActive(true);
    }
}

[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor, kills, deaths;     // 네트워크 연결 시 할당되는 번호

    public PlayerInfo(string _name, int _actor, int _kills, int _deaths)
    {
        name = _name;
        actor = _actor;
        kills = _kills;
        deaths = _deaths;
    }
}
