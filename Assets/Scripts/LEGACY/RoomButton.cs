using Photon.Pun;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    private RoomInfo info;       // Photon.Realtime의 내장타입(방 정모들을 담을 수 있음)
    
    public TMP_Text videoNameText;  // 공유 비디오 이름
    public TMP_Text makeID_Text;    // 개설자 아이디
    
    // OnRoomListUpdate에서 방 정보가 업데이트 되고, 텍스트 변경 때 호출
    public void SetButtonDetails(RoomInfo inputInfo)
    {
        info = inputInfo;            // 정보 저장
        
        string[] splitParts = info.Name.Split('$');
        string frontPart    = splitParts[0];
        string backPart     = splitParts[1];
        
        videoNameText.text = frontPart; // 비디오 이름
        makeID_Text.text   = backPart;  // 개설자 ID
    }
   
    // 룸 버튼이 눌리는 순간, JoinRoom()함수에, info를 넣어서 실행
    public void SharedRoomJoin()
    {
        PhotonNetwork.JoinRoom(info.Name);
        
        // PunSystem.instance.CloseMenus();
        // loadingText.text = "Joining Room";
        // loadingScreen.SetActive(true);
    }
}
