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
        
        // Inicializar lista de spawn points disponibles
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
            // Seleccionar spawn point aleatorio
            int randomIndex = Random.Range(0, availableSpawnPoints.Count);
            selectedSpawnPoint = availableSpawnPoints[randomIndex];
            availableSpawnPoints.RemoveAt(randomIndex);
        }
        else
        {
            // Usar spawn point secuencial
            selectedSpawnPoint = availableSpawnPoints[0];
            availableSpawnPoints.RemoveAt(0);
        }
        
        // Registrar el spawn point usado por este jugador
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
    
    public void SpawnPlayer(int playerId)
    {
        Debug.Log($"SpawnPlayer called with ID: {playerId}");
        Debug.Log($"Player prefab name: {playerPrefabName}");
        
        Transform spawnPoint = GetSpawnPoint(playerId);
        if (spawnPoint == null) 
        {
            Debug.LogError("No spawn point available!");
            return;
        }
        
        Debug.Log($"Using spawn point: {spawnPoint.name} at position: {spawnPoint.position}");
        
        // Instanciar jugador en la red
        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.position, spawnPoint.rotation);
        
        if (player == null)
        {
            Debug.LogError("Failed to instantiate player!");
            return;
        }
        
        Debug.Log($"Player instantiated: {player.name}");
        
        // Configurar el jugador
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Asignar ID del jugador
            playerController.photonView.OwnerActorNr = playerId;
            
            // Configurar nombre del jugador
            string playerName = PhotonNetwork.CurrentRoom.GetPlayer(playerId)?.NickName ?? "Player" + playerId;
            player.name = playerName;
            
            Debug.Log($"Player configured: {playerName}");
        }
        else
        {
            Debug.LogError("PlayerController component not found on instantiated player!");
        }
        
        Debug.Log($"Jugador {playerId} spawneado en {spawnPoint.name}");
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
    
    // Método para verificar si un spawn point está disponible
    public bool IsSpawnPointAvailable(Transform spawnPoint)
    {
        return availableSpawnPoints.Contains(spawnPoint);
    }
    
    // Método para obtener todos los spawn points ocupados
    public Transform[] GetOccupiedSpawnPoints()
    {
        return playerSpawnPoints.Values.ToArray();
    }
    
    // Método para limpiar todos los spawn points
    public void ClearAllSpawnPoints()
    {
        availableSpawnPoints.Clear();
        playerSpawnPoints.Clear();
        RefreshAvailableSpawnPoints();
    }
    
    // Método para agregar spawn points dinámicamente
    public void AddSpawnPoint(Transform newSpawnPoint)
    {
        if (newSpawnPoint != null && !availableSpawnPoints.Contains(newSpawnPoint))
        {
            availableSpawnPoints.Add(newSpawnPoint);
        }
    }
    
    // Método para remover spawn points
    public void RemoveSpawnPoint(Transform spawnPoint)
    {
        if (availableSpawnPoints.Contains(spawnPoint))
        {
            availableSpawnPoints.Remove(spawnPoint);
        }
        
        // Remover de spawn points ocupados si es necesario
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