using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;

public class ScoreManager : MonoBehaviourPun, IOnEventCallback
{
    public static ScoreManager Instance;

    private Dictionary<int, int> actorToScore = new Dictionary<int, int>();
    private Dictionary<int, int> actorToKills = new Dictionary<int, int>();
    private Dictionary<int, int> actorToDeaths = new Dictionary<int, int>();

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
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (photonEvent.Code == 103) // Kill notification
        {
            object[] content = (object[])photonEvent.CustomData;
            int killerActor = (int)content[0];
            int victimActor = (int)content[1];
            int victimViewID = (int)content[2];
            RegisterKill(killerActor, victimActor, victimViewID);
        }
        else if (photonEvent.Code == 104) // Death notification
        {
            object[] content = (object[])photonEvent.CustomData;
            int victimActor = (int)content[0];
            int victimViewID = (int)content[1];
            RegisterDeath(victimActor, victimViewID);
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
    
    // Registrar kill (el que mató)
    public void RegisterKill(int killerActor, int victimActor, int victimViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        // Incrementar kills del atacante
        if (!actorToKills.ContainsKey(killerActor)) actorToKills[killerActor] = 0;
        actorToKills[killerActor]++;
        
        // Incrementar deaths de la víctima
        if (!actorToDeaths.ContainsKey(victimActor)) actorToDeaths[victimActor] = 0;
        actorToDeaths[victimActor]++;
        
        // Dar puntos al killer
        AddScoreMaster(killerActor, 100);
        
        // Sincronizar estadísticas
        photonView.RPC("RPC_SyncKillStats", RpcTarget.All, killerActor, actorToKills[killerActor], victimActor, actorToDeaths[victimActor], victimViewID);
    }
    
    // Registrar muerte sin killer (caída, etc.)
    public void RegisterDeath(int victimActor, int victimViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        // Solo incrementar deaths
        if (!actorToDeaths.ContainsKey(victimActor)) actorToDeaths[victimActor] = 0;
        actorToDeaths[victimActor]++;
        
        photonView.RPC("RPC_SyncDeathStats", RpcTarget.All, victimActor, actorToDeaths[victimActor], victimViewID);
    }

    [PunRPC]
    void RPC_SyncScore(int actorNumber, int newScore)
    {
        actorToScore[actorNumber] = newScore;
        Debug.Log($"💰 Score → Player {actorNumber}: {newScore} puntos");
    }
    
    [PunRPC]
    void RPC_SyncKillStats(int killerActor, int killerKills, int victimActor, int victimDeaths, int victimViewID)
    {
        actorToKills[killerActor] = killerKills;
        actorToDeaths[victimActor] = victimDeaths;
        Debug.Log($"💀 KILL → Player {killerActor} mató a Player {victimActor} (ViewID:{victimViewID}) | Kills: {killerKills} | Deaths: {victimDeaths}");
    }
    
    [PunRPC]
    void RPC_SyncDeathStats(int victimActor, int victimDeaths, int victimViewID)
    {
        actorToDeaths[victimActor] = victimDeaths;
        Debug.Log($"💀 DEATH → Player {victimActor} murió (ViewID:{victimViewID}) | Deaths: {victimDeaths}");
    }

    public int GetScore(int actorNumber)
    {
        if (actorToScore.TryGetValue(actorNumber, out int s)) return s;
        return 0;
    }
    
    public int GetKills(int actorNumber)
    {
        if (actorToKills.TryGetValue(actorNumber, out int k)) return k;
        return 0;
    }
    
    public int GetDeaths(int actorNumber)
    {
        if (actorToDeaths.TryGetValue(actorNumber, out int d)) return d;
        return 0;
    }
    
    public void PrintScoreboard()
    {
        Debug.Log("🏆 === SCOREBOARD ===");
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            int kills = GetKills(player.ActorNumber);
            int deaths = GetDeaths(player.ActorNumber);
            int score = GetScore(player.ActorNumber);
            float kdr = deaths > 0 ? (float)kills / deaths : kills;
            Debug.Log($"🎯 Player {player.ActorNumber}: {kills}K/{deaths}D | Score: {score} | K/D: {kdr:F2}");
        }
    }
}
