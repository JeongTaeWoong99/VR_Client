using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Manager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static Manager instance;

    public GameViewEncoder _gameViewEncoder;
    
    private void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        // 게임 중(룸 안에 들어와 있는 상태), 네트워크 연결이 끊기면, 메인 메인메뉴로 돌아가기(로비로) // 룸 -> 로비
        if(!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        // 초기 세팅
        else
        {
            _gameViewEncoder.label = PhotonNetwork.LocalPlayer.ActorNumber; // 엑터 넘버를 라벨 번호로 설정.
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

    
    // Room을 나갈 때, 호출
    // UIController의 ReturnToMainMenu()를 사용하기 위해서, 필요
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(0);
    }
}
