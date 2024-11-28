using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

// 미러링 전용
public class FM_System : MonoBehaviour
{
    public static FM_System instance;

    public PhotonView       _photonView;

    public bool             isWatching; // CMS가 내 화면을 보고 있는지 여부
    
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
            if (isWatching) // CMS에서 내 미러링 화면을 보고있음...
            {
                foreach (var cmsPlayer in cmsPlayers)
                {
                    // 각 CMS 플레이어만, RPC 실행
                    _photonView.RPC("RPC_SendMessage", cmsPlayer, _bytesData, message);
                    Debug.Log("RPC_SendMessage 작동 O.(CMS 같음 방 and CMS가 내 화면 보는 있음)");
                }
            }
            else
                Debug.Log("RPC_SendMessage 작동 X.(CMS가 내 화면 보지 않음)");
        }
        else
            Debug.Log("RPC_SendMessage 작동 X.(CMS 없음)");
    }
    
    // 버튼으로 사용 + CMS RPC로도 사용
    [PunRPC]
    public void Watching(bool state)
    {
        isWatching = state;
    }
}