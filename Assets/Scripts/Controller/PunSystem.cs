using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class PunSystem : MonoBehaviour
{
    public static PunSystem instance;

    public PhotonView _photonView;

    private void Awake()
    {
        instance = this;
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
        
        // 서버 브렌치
    }
}