using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;

public class PlayerSpawnManager : MonoBehaviourPun
{
    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public float spawnDelay = 2f;
    public bool useRandomSpawn = true;
    
    [Header("Player Settings")]
    public GameObject playerPrefab;
    public string playerPrefabName = "playerspin";
    
    private List<Transform> availableSpawnPoints = new List<Transform>();
    private Dictionary<int, Transform> playerSpawnPoints = new Dictionary<int, Transform>();
    
    void Start()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No hay puntos de spawn configurados!");
            return;
        }
        RefreshAvailableSpawnPoints();
    }
    
    void RefreshAvailableSpawnPoints()
    {
        availableSpawnPoints.Clear();
        if (spawnPoints == null) return;
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                availableSpawnPoints.Add(spawnPoint);
            }
        }
        Debug.Log($"Spawn points disponibles: {availableSpawnPoints.Count}");
    }
    
    public Transform GetSpawnPoint(int playerId)
    {
        if (availableSpawnPoints.Count == 0)
        {
            RefreshAvailableSpawnPoints();
        }
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError("No hay puntos de spawn disponibles!");
            return null;
        }
        Transform selectedSpawnPoint;
        if (useRandomSpawn)
        {
            int randomIndex = Random.Range(0, availableSpawnPoints.Count);
            selectedSpawnPoint = availableSpawnPoints[randomIndex];
            availableSpawnPoints.RemoveAt(randomIndex);
        }
        else
        {
            selectedSpawnPoint = availableSpawnPoints[0];
            availableSpawnPoints.RemoveAt(0);
        }
        playerSpawnPoints[playerId] = selectedSpawnPoint;
        return selectedSpawnPoint;
    }
    
    public void ReleaseSpawnPoint(int playerId)
    {
        if (playerSpawnPoints.ContainsKey(playerId))
        {
            Transform spawnPoint = playerSpawnPoints[playerId];
            if (!availableSpawnPoints.Contains(spawnPoint))
            {
                availableSpawnPoints.Add(spawnPoint);
            }
            playerSpawnPoints.Remove(playerId);
        }
    }
    
    // Local self-spawn (kept for compatibility if needed)
    public void SpawnPlayer(int playerId)
    {
        // En flujo room-owned esta vía no debe usarse al entrar al room
        if (!ResourceExists(playerPrefabName))
        {
            Debug.LogError($"❌ Resources.Load no encontró prefab '{playerPrefabName}'. Asegúrate de tener Assets/Resources/{playerPrefabName}.prefab");
            return;
        }
        Transform spawnPoint = GetSpawnPoint(playerId);
        if (spawnPoint == null)
        {
            Debug.LogError("No spawn point available!");
            return;
        }
        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.position, spawnPoint.rotation);
        if (player == null)
        {
            Debug.LogError("Failed to instantiate player!");
            return;
        }
        PhotonView spawnedPv = player.GetComponent<PhotonView>();
        if (spawnedPv != null)
        {
            Debug.Log($"Spawned player PhotonView → ViewID={spawnedPv.ViewID}, IsMine={spawnedPv.IsMine}");
        }
        else
        {
            Debug.LogError("❌ Spawned player has no PhotonView. Ensure the prefab has a PhotonView component.");
        }
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            string playerName = PhotonNetwork.CurrentRoom.GetPlayer(playerId)?.NickName ?? "Player" + playerId;
            player.name = playerName;
        }
        Debug.Log($"Jugador {playerId} spawneado en {spawnPoint.name}");
    }
    
    // Master-authoritative spawn for any actor (uses RoomObject so late joiners always get it)
    public void SpawnPlayerForActor(int actorNumber)
    {
        Debug.Log($"🎯 SPAWN: SpawnPlayerForActor llamado para player {actorNumber}");
        
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("SpawnPlayerForActor solo debe llamarlo el MasterClient");
            return;
        }
        
        Debug.Log($"✅ SPAWN: Soy Master Client, continuando spawn para player {actorNumber}");
        if (!ResourceExists(playerPrefabName))
        {
            Debug.LogError($"❌ Resources.Load no encontró prefab '{playerPrefabName}'. Asegúrate de tener Assets/Resources/{playerPrefabName}.prefab");
            return;
        }
        // Simplificar: crear directamente, PhotonNetwork maneja duplicados automáticamente
        Debug.Log($"🚀 Creando player {actorNumber} directamente (sin limpieza previa)");
        SpawnPlayerForActorInternal(actorNumber);
    }
    
    bool HasPlayerObject(int actorNumber)
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            if (p != null && p.photonView != null && p.photonView.OwnerActorNr == actorNumber)
            {
                return true;
            }
        }
        return false;
    }
    
    bool ResourceExists(string prefabName)
    {
        var res = Resources.Load<GameObject>(prefabName);
        return res != null;
    }



    public void SpawnPlayerForActorInternal(int actorNumber)
    {
        Debug.Log($"🔧 SpawnPlayerForActorInternal iniciado para player {actorNumber}");
        if (HasPlayerObject(actorNumber))
        {
            Debug.Log($"↩️ Ya existe objeto de jugador para player {actorNumber}, no se crea otro.");
            return;
        }
        Debug.Log($"✅ No hay objeto existente, creando nuevo para player {actorNumber}");
        Transform spawnPoint = GetSpawnPoint(actorNumber);
        if (spawnPoint == null)
        {
            Debug.LogError($"❌ No hay spawn points disponibles para actor {actorNumber}");
            return;
        }
        GameObject playerObj = PhotonNetwork.InstantiateRoomObject(playerPrefabName, spawnPoint.position, spawnPoint.rotation);
        if (playerObj == null)
        {
            Debug.LogError($"❌ No se pudo instanciar prefab '{playerPrefabName}' para actor {actorNumber}");
            return;
        }
        var pv = playerObj.GetComponent<PhotonView>();
        if (pv == null)
        {
            Debug.LogError($"❌ Prefab '{playerPrefabName}' no tiene PhotonView!");
            PhotonNetwork.Destroy(playerObj);
            return;
        }
        pv.TransferOwnership(actorNumber);
        string playerName = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber)?.NickName ?? "Player" + actorNumber;
        playerObj.name = playerName;
        Debug.Log($"✅ Master creó RoomObject para player {actorNumber} con ViewID={pv.ViewID} y transfirió ownership");
    }
    
    public void RespawnPlayer(int playerId)
    {
        StartCoroutine(RespawnPlayerCoroutine(playerId));
    }
    
    System.Collections.IEnumerator RespawnPlayerCoroutine(int playerId)
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnPlayer(playerId);
    }
    
    public bool IsSpawnPointAvailable(Transform spawnPoint)
    {
        return availableSpawnPoints.Contains(spawnPoint);
    }
    
    public Transform[] GetOccupiedSpawnPoints()
    {
        return playerSpawnPoints.Values.ToArray();
    }
    
    public void ClearAllSpawnPoints()
    {
        availableSpawnPoints.Clear();
        playerSpawnPoints.Clear();
        RefreshAvailableSpawnPoints();
    }
    
    public void AddSpawnPoint(Transform newSpawnPoint)
    {
        if (newSpawnPoint != null && !availableSpawnPoints.Contains(newSpawnPoint))
        {
            availableSpawnPoints.Add(newSpawnPoint);
        }
    }
    
    public void RemoveSpawnPoint(Transform spawnPoint)
    {
        if (availableSpawnPoints.Contains(spawnPoint))
        {
            availableSpawnPoints.Remove(spawnPoint);
        }
        List<int> keysToRemove = new List<int>();
        foreach (var kvp in playerSpawnPoints)
        {
            if (kvp.Value == spawnPoint)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (int key in keysToRemove)
        {
            playerSpawnPoints.Remove(key);
        }
    }
} 