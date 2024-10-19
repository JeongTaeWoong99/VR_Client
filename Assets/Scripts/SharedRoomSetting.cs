using UnityEngine;
using TMPro;
using Photon.Realtime;

public class SharedRoomSetting : MonoBehaviour
{
    private RoomInfo info;          // Photon.Realtime의 내장타입(방 정모들을 담을 수 있음)
    
    public  TMP_Text videoNameText;
    public  TMP_Text makeID_Text;

    // OnRoomListUpdate에서 방 정보가 업데이트 되고, 텍스트 변경 때 호출
    public void SetButtonDetails(RoomInfo inputInfo)
    {
        info = inputInfo;               // 정보 저장
        string[] splitParts = info.Name.Split('$');
        string frontPart    = splitParts[0];    // 비디오 이름
        string backPart     = splitParts[1];    // 만든사람 ID
        
        videoNameText.text = frontPart; // 정보에서 방 이름을 가져와, 버튼텍스트 변경
        makeID_Text.text   = backPart;  // 만든사람 ID 표시
    }
   
    // 룸 버튼이 눌리는 순간, JoinRoom()함수에, info를 넣어서 실행
    public void OpenRoom()
    {
        //PunSystem.instance.JoinRoom(info);
    }
}
