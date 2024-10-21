using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner instance;
    
    public  GameObject playerPrefab;     // 플레이어 프리펩
    private GameObject player;           // 포톤네트워크에 만들어진 플레이어 정보
    public  GameObject deathEffect;      // 사망 파티클
    public  float      respawnTime = 5f; // 부활 지연시간

    private void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        if(PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }
    
    public void SpawnPlayer()
    {
        // PhotonNetwork.Instantiate에서는 string 첫번째 자리에 들어가야 함. playerPrefab.name는 리소스 폴더에 있는, "Player"와 같다.
        // PhotonNetwork.Instantiate 때문에, 연결되어 있는 모든 player의 씬에 모두 생성된다.
        // IsMine은 만든 본인이 가진다. 
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("실행");
    }

    public void Die(string damager)
    {
        // 죽음 텍스트 활성화(damager의 이름 추가)
        // 킬뎃업데이트(데스가 올라갈 사람 즉 자기자신,death,증감양)    -> Die함수가 실행 시 자신이 죽은 것 이기 때문에!!
        UI_Manager.instance.deathText.text = "You were killed by " + damager;
        MatchManager.instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
    
        if(player != null)
        {
            StartCoroutine(DieCo());
        }
    }
    
    public IEnumerator DieCo()
    {
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);
    
        PhotonNetwork.Destroy(player);
        player = null;
        UI_Manager.instance.deathScreen.SetActive(true);
    
        yield return new WaitForSeconds(respawnTime);
    
        UI_Manager.instance.deathScreen.SetActive(false);
    
        if (MatchManager.instance.state == MatchManager.GameState.Playing && player == null)
        {
            SpawnPlayer();
        }
    }
}
