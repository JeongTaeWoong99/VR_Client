using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardPlayer : MonoBehaviour
{
    public TMP_Text playerNameText, killsText, deathsText;

    // 각자의 UIController의 leaderboardPlayerDisplay 즉 세미 프리팹에 하나씩 들어가 있는, 오브젝트의  text들을 설정
    public void SetDetails(string name, int kills, int deaths)
    {
        playerNameText.text = name;
        killsText.text = kills.ToString();
        deathsText.text = deaths.ToString();
    }
}
