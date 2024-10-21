using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;
    
    [HideInInspector]
    public TMP_Text overheatedMessage;
    [HideInInspector]
    public Slider weaponTempSlider;
         
    [HideInInspector]
    public GameObject deathScreen;
    [HideInInspector]
    public TMP_Text deathText;
    
    [HideInInspector]
    public Slider healthSlider;
    [HideInInspector]
    public TMP_Text killsText, deathsText;
    [HideInInspector]
    public GameObject leaderboard;
    [HideInInspector]
    public LeaderboardPlayer leaderboardPlayerDisplay;
    [HideInInspector]
    public TMP_Text timerText;
    
    public GameObject endScreen;

    public GameObject optionsScreen;
    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptions();
        }

        if (optionsScreen.activeInHierarchy && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ShowHideOptions()
    {
        if(!optionsScreen.activeInHierarchy)
        {
            optionsScreen.SetActive(true);
        } else
        {
            optionsScreen.SetActive(false);
        }
    }
    
    public void ReturnToMainMenu()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
