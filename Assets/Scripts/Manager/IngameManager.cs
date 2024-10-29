using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

// 미러링 + 화면 공유 공통 매니저
// 여기다가 ReturnToMainMenu같은 미러링방 + 화면공유방 공통으로 씌일 녀석들을 넣으면 될 듯
public class IngameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static IngameManager instance;

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
