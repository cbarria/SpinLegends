using UnityEngine;
using TMPro;
using Photon.Pun;

public class ScoreboardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    void Awake()
    {
        if (text == null) text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (text == null) return;
        var sm = ScoreManager.Instance;
        if (sm == null)
        {
            text.text = "";
            return;
        }
        var players = PhotonNetwork.PlayerList;
        System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
        for (int i = 0; i < players.Length; i++)
        {
            int actor = players[i].ActorNumber;
            int sc = sm.GetScore(actor);
            sb.Append(players[i].NickName).Append(": ").Append(sc);
            if (i < players.Length - 1) sb.Append("   ");
        }
        text.text = sb.ToString();
    }
}
