using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;
    
    [Header("Network Settings")]
    public string gameVersion = "1.0";
    public int maxPlayersPerRoom = 4;
    
    [Header("Player Prefab")]
    public GameObject playerPrefab;
    
    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    
    private bool isConnecting = false;
    private float joinRoomTimeout = 5f; // Reducir a 5 segundos
    private float joinRoomTimer = 0f;
    
    public bool IsConnecting => isConnecting;
    private List<PlayerController> networkPlayers = new List<PlayerController>();
    private PlayerSpawnManager spawnManager;
    
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
    
    void Start()
    {
        // Configurar Photon
        PhotonNetwork.AutomaticallySyncScene = true;
        
        // Configurar spawn manager
        SetupSpawnManager();
        
        ConnectToServer();
    }
    
    void Update()
    {
        // Timeout para unirse a sala
        if (PhotonNetwork.IsConnected && !PhotonNetwork.InRoom && !isConnecting)
        {
            joinRoomTimer += Time.deltaTime;
            
            // Log cada segundo para debug
            if (Mathf.FloorToInt(joinRoomTimer) % 1 == 0 && Time.frameCount % 60 == 0)
            {
                Debug.Log($"Waiting to join room... {joinRoomTimer:F1}s / {joinRoomTimeout}s");
            }
            
            if (joinRoomTimer >= joinRoomTimeout)
            {
                Debug.Log("Join room timeout. Creating new room...");
                CreateRoom();
                joinRoomTimer = 0f;
            }
        }
    }
    
    public void ConnectToServer()
    {
        if (PhotonNetwork.IsConnected) return;
        
        isConnecting = true;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
        
        Debug.Log("Connecting to Photon...");
    }
    
    public void JoinRandomRoom()
    {
        if (!PhotonNetwork.IsConnected) return;
        
        PhotonNetwork.JoinRandomRoom();
        Debug.Log("Joining random room...");
        joinRoomTimer = 0f; // Reset timer
    }
    
    public void CreateRoom(string roomName = null)
    {
        if (!PhotonNetwork.IsConnected) return;
        
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true
        };
        
        string roomNameToUse = roomName ?? "BeybladeRoom_" + Random.Range(1000, 9999);
        PhotonNetwork.CreateRoom(roomNameToUse, roomOptions);
        Debug.Log("Creating room: " + roomNameToUse);
    }
    
    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }
    
    // Photon Callbacks
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon server!");
        isConnecting = false;
        
        // Unirse automáticamente a una sala
        JoinRandomRoom();
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from server: " + cause);
        isConnecting = false;
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Players in room: " + PhotonNetwork.CurrentRoom.PlayerCount);
        
        // Spawn del jugador local
        Debug.Log("Attempting to spawn player...");
        SpawnPlayer();
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New player joined: " + newPlayer.NickName);
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left: " + otherPlayer.NickName);
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to create room: " + message);
        // Try to join a random room instead
        JoinRandomRoom();
    }
    

    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to join room: " + message);
        // Try to create a new room instead
        CreateRoom();
    }
    
    void SetupSpawnManager()
    {
        // Crear spawn manager si no existe
        if (spawnManager == null)
        {
            GameObject spawnManagerObj = new GameObject("PlayerSpawnManager");
            spawnManager = spawnManagerObj.AddComponent<PlayerSpawnManager>();
            spawnManager.spawnPoints = spawnPoints;
            spawnManager.playerPrefabName = playerPrefab != null ? playerPrefab.name : "playerspin";
        }
    }
    
    public void SpawnPlayer()
    {
        Debug.Log("SpawnPlayer called");
        
        if (spawnManager == null)
        {
            Debug.Log("SpawnManager is null, setting up...");
            SetupSpawnManager();
        }
        
        // Usar spawn manager para spawn del jugador
        int playerId = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log($"Spawning player with ID: {playerId}");
        spawnManager.SpawnPlayer(playerId);
        
        // Buscar el jugador spawneado y configurarlo
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        Debug.Log($"Found {players.Length} PlayerController instances");
        
        foreach (PlayerController playerController in players)
        {
            Debug.Log($"Checking player: {playerController.name}, IsMine: {playerController.photonView.IsMine}");
            if (playerController.photonView.IsMine && !networkPlayers.Contains(playerController))
            {
                Debug.Log("Found local player, setting up...");
                networkPlayers.Add(playerController);
                SetupLocalPlayer(playerController);
                break;
            }
        }
    }
    
    void SetupLocalPlayer(PlayerController playerController)
    {
        // Configurar cámara en tercera persona
        SetupThirdPersonCamera(playerController);
        
        // Configurar UI para el jugador local
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLocalPlayer(playerController);
        }
    }
    
    void SetupThirdPersonCamera(PlayerController playerController)
    {
        // Buscar o crear ThirdPersonCamera
        ThirdPersonCamera thirdPersonCamera = Camera.main.GetComponent<ThirdPersonCamera>();
        if (thirdPersonCamera == null)
        {
            thirdPersonCamera = Camera.main.gameObject.AddComponent<ThirdPersonCamera>();
        }
        
        // Configurar la cámara para seguir al jugador
        thirdPersonCamera.SetTarget(playerController.transform);
        
        Debug.Log("Third person camera configured for player: " + playerController.name);
    }
    
    public List<PlayerController> GetNetworkPlayers()
    {
        return networkPlayers;
    }
    
    public bool IsConnected()
    {
        return PhotonNetwork.IsConnected;
    }
    
    public bool IsInRoom()
    {
        return PhotonNetwork.InRoom;
    }
    
    public int GetPlayerCount()
    {
        return PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
    }
    
    public void RespawnPlayer()
    {
        Debug.Log("RespawnPlayer called");
        
        // Limpiar jugadores muertos
        networkPlayers.RemoveAll(p => p == null || !p.gameObject.activeInHierarchy);
        
        // Spawn nuevo jugador
        SpawnPlayer();
    }
} 