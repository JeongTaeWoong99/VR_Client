using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

// 미러링 전용
public class FM_System : MonoBehaviour
{
    public static FM_System instance;

    public PhotonView      _photonView;
    public GameViewEncoder _gameViewEncoder;    // 미러링 전용

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if(_gameViewEncoder)
            _gameViewEncoder.label = PhotonNetwork.LocalPlayer.ActorNumber; // 엑터 넘버를 라벨 번호로 설정.
    }
    
    public void SendMessage(byte[] _bytesData, string message)
    {
        // 룸에 접속해 있는 CMS 시스템의 리스트를 받기.
        List<Photon.Realtime.Player> cmsPlayers = PhotonNetwork.PlayerListOthers.Where(C => C.CustomProperties.ContainsKey("CMS") && (bool)C.CustomProperties["CMS"]).ToList();
        
        if (cmsPlayers.Count > 0)
        {
            foreach (var cmsPlayer in cmsPlayers)
            {
                // 각 CMS 플레이어만, RPC 실행
                _photonView.RPC("RPC_SendMessage", cmsPlayer, _bytesData, message);
                Debug.Log("CMS 있음 + RPC 실행 O");
            }
        }
        else
        {
            Debug.Log("CMS 없음 + RPC 실행 X");
        }
    }
}