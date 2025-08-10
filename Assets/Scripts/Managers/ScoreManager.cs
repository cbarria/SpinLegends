using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviourPun
{
    public static ScoreManager Instance;

    private Dictionary<int, int> actorToScore = new Dictionary<int, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Solo debe llamarse en el Master
    public void AddScoreMaster(int actorNumber, int amount)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!actorToScore.ContainsKey(actorNumber)) actorToScore[actorNumber] = 0;
        actorToScore[actorNumber] += amount;
        photonView.RPC("RPC_SyncScore", RpcTarget.All, actorNumber, actorToScore[actorNumber]);
    }

    [PunRPC]
    void RPC_SyncScore(int actorNumber, int newScore)
    {
        actorToScore[actorNumber] = newScore;
        // Aquí podrías actualizar una UI de scoreboard si existe
        Debug.Log($"Score -> Actor {actorNumber}: {newScore}");
    }

    public int GetScore(int actorNumber)
    {
        if (actorToScore.TryGetValue(actorNumber, out int s)) return s;
        return 0;
    }
}
