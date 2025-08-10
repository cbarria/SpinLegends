using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon; // for EventData and SendOptions

public class NetworkManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static NetworkManager Instance;
    
    [Header("Network Settings")]
    public string gameVersion = "1.0";
    public int maxPlayersPerRoom = 4;
    
    [Header("Player Prefab")]
    public GameObject playerPrefab;
    public string playerPrefabName = "playerspin"; // nombre consistente para todos los clientes (Resources)
    
    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    
    private bool isConnecting = false;
    private float joinRoomTimeout = 5f; // Reducir a 5 segundos
    private float joinRoomTimer = 0f;

    public float JoinRoomTimeout => joinRoomTimeout;
    public float CurrentJoinTimer => joinRoomTimer;
    
    public bool IsConnecting => isConnecting;
    private List<PlayerController> networkPlayers = new List<PlayerController>();
    private PlayerSpawnManager spawnManager;

    private PhotonView pv; // para RPC

    private const byte SpawnRequestEventCode = 101;
    
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
        // Asegurar PhotonView para RPCs
        pv = GetComponent<PhotonView>();
        if (pv == null) pv = gameObject.AddComponent<PhotonView>();
    }

    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        SetupSpawnManager();
        ConnectToServer();
    }
    
    void Update()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.InRoom && !isConnecting)
        {
            joinRoomTimer += Time.deltaTime;
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
        var views = FindObjectsByType<PhotonView>(FindObjectsSortMode.None);
        Debug.Log($"🔎 Found {views.Length} PhotonViews in scene on join");
        foreach (var v in views)
        {
            Debug.Log($"   PV name={v.gameObject.name} ViewID={v.ViewID} IsMine={v.IsMine} Owner={v.OwnerActorNr}");
        }
        if (PhotonNetwork.IsMasterClient && spawnManager != null)
        {
            foreach (var kv in PhotonNetwork.CurrentRoom.Players)
            {
                int actor = kv.Value.ActorNumber;
                spawnManager.SpawnPlayerForActor(actor);
            }
        }
        Invoke(nameof(RequestSpawnIfMissing), 2f);
        Invoke(nameof(VerifyPlayersVisible), 2.5f);
    }

    void RequestSpawnIfMissing()
    {
        int myActor = PhotonNetwork.LocalPlayer.ActorNumber;
        bool haveMine = false;
        var pcs = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var p in pcs)
        {
            if (p != null && p.photonView != null && p.photonView.OwnerActorNr == myActor)
            {
                haveMine = true; break;
            }
        }
        if (!haveMine)
        {
            Debug.LogWarning("⚠️ No llegó mi RoomObject todavía. Solicitando al Master...");
            StartCoroutine(SendSpawnRequestWhenViewReady(myActor));
        }
    }

    System.Collections.IEnumerator SendSpawnRequestWhenViewReady(int actor)
    {
        float timeout = 2f;
        float elapsed = 0f;
        while ((pv == null || pv.ViewID == 0) && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }
        if (pv != null && pv.ViewID != 0)
        {
            pv.RPC("RPC_RequestSpawnForActor", RpcTarget.MasterClient, actor);
        }
        else
        {
            Debug.LogWarning("⚠️ PhotonView aún sin ViewID, usando RaiseEvent fallback para solicitar spawn.");
            var content = new object[] { actor };
            var options = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            PhotonNetwork.RaiseEvent(SpawnRequestEventCode, content, options, SendOptions.SendReliable);
        }
    }

    [PunRPC]
    void RPC_RequestSpawnForActor(int actorNumber, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (spawnManager != null)
        {
            spawnManager.SpawnPlayerForActor(actorNumber);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == SpawnRequestEventCode && PhotonNetwork.IsMasterClient)
        {
            var data = photonEvent.CustomData as object[];
            if (data != null && data.Length > 0 && data[0] is int actorNumber)
            {
                if (spawnManager != null)
                {
                    spawnManager.SpawnPlayerForActor(actorNumber);
                }
            }
        }
    }
    
    void VerifyPlayersVisible()
    {
        int expected = PhotonNetwork.CurrentRoom.PlayerCount;
        int actual = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length;
        if (actual < expected)
        {
            Debug.LogWarning($"⚠️ Late-join visibility mismatch: expected {expected} players but see {actual}. PrefabName='{playerPrefabName}'. Asegúrate de Resources.");
        }
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New player joined: " + newPlayer.NickName);
        if (PhotonNetwork.IsMasterClient && spawnManager != null)
        {
            spawnManager.SpawnPlayerForActor(newPlayer.ActorNumber);
        }
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left: " + otherPlayer.NickName);
        if (PhotonNetwork.IsMasterClient)
        {
            var pcs = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var p in pcs)
            {
                if (p != null && p.photonView != null && p.photonView.OwnerActorNr == otherPlayer.ActorNumber)
                {
                    PhotonNetwork.Destroy(p.gameObject);
                }
            }
        }
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to create room: " + message);
        JoinRandomRoom();
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to join room: " + message);
        CreateRoom();
    }
    
    void SetupSpawnManager()
    {
        if (spawnManager == null)
        {
            GameObject spawnManagerObj = new GameObject("PlayerSpawnManager");
            spawnManager = spawnManagerObj.AddComponent<PlayerSpawnManager>();
            spawnManager.spawnPoints = spawnPoints;
            spawnManager.playerPrefabName = playerPrefabName;
        }
        else
        {
            spawnManager.playerPrefabName = playerPrefabName;
        }
    }
    
    void SetupThirdPersonCamera(PlayerController playerController)
    {
        ThirdPersonCamera thirdPersonCamera = Camera.main.GetComponent<ThirdPersonCamera>();
        if (thirdPersonCamera == null)
        {
            thirdPersonCamera = Camera.main.gameObject.AddComponent<ThirdPersonCamera>();
        }
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
        Debug.Log("RespawnPlayer legacy call ignored: Master handles respawn.");
    }
} 