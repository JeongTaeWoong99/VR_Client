using System.IO;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class SharedRoomButton : MonoBehaviour
{
    private RoomInfo info;          // Photon.Realtime의 내장타입(방 정모들을 담을 수 있음)
    private string   frontPart;     // 비디오 이름 부분 
    private string   backPart;      // 아이디 부분
    
    public  TMP_Text videoNameText;
    public  TMP_Text makeID_Text;

    // OnRoomListUpdate에서 방 정보가 업데이트 되고, 텍스트 변경 때 호출
    public void SettingRoomPanel(RoomInfo inputInfo)
    {
        info = inputInfo;
        string[] splitParts = info.Name.Split('$');
        frontPart = Path.GetFileNameWithoutExtension(splitParts[0]); // 비디오 이름(+확장자 제거)
        backPart  = splitParts[1];                                   // 만든사람 ID
        
        videoNameText.text = frontPart; // 정보에서 방 이름을 가져와, 버튼텍스트 변경
        makeID_Text.text   = backPart;  // 만든사람 ID 표시
    }
    
    // 입장 버튼(-> 파일 선택 -> 체크 -> 입장 or 거부)
    public void OnCheckAndJoin()
    {   
        StartCoroutine(VideoManager.instance.CheckAndJoin(frontPart,info.Name));
    }
        
    // 다운 버튼
    public void OnVideoDownload()
    {
        StartCoroutine(VideoManager.instance.VideoDownload(frontPart));
    }
}
