using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Maneja la sincronización de jugadores existentes cuando un nuevo cliente entra a la sala.
/// Soluciona el problema donde los clientes no ven a los jugadores que ya estaban en la sala.
/// </summary>
public class PlayerSyncManager : MonoBehaviourPunCallbacks
{
    [Header("Sync Settings")]
    public float syncDelay = 1f;
    public bool enableDebugLogs = true;
    public int maxSyncAttempts = 3;
    
    [Header("Player Prefab")]
    public GameObject playerPrefab;
    public string playerPrefabName = "playerspin";
    
    private bool hasSyncedPlayers = false;
    private int syncAttempts = 0;
    private Dictionary<int, PlayerInfo> existingPlayers = new Dictionary<int, PlayerInfo>();
    
    [System.Serializable]
    public class PlayerInfo
    {
        public int actorNumber;
        public string playerName;
        public Vector3 position;
        public Quaternion rotation;
        public bool isSpinning;
        public float health;
        public float spinSpeed;
        public Vector3 velocity;
        
        public PlayerInfo(int actorNr, string name, Vector3 pos, Quaternion rot)
        {
            actorNumber = actorNr;
            playerName = name;
            position = pos;
            rotation = rot;
            isSpinning = false;
            health = 100f;
            spinSpeed = 0f;
            velocity = Vector3.zero;
        }
    }
    
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        // Asegurar que tenemos un PhotonView
        EnsurePhotonView();
    }
    
    void EnsurePhotonView()
    {
        // Verificar si ya tenemos un PhotonView
        PhotonView existingPhotonView = GetComponent<PhotonView>();
        if (existingPhotonView == null)
        {
            if (enableDebugLogs)
                Debug.Log("🔄 Creando PhotonView en PlayerSyncManager...");
            
            // Crear el PhotonView
            PhotonView newPhotonView = gameObject.AddComponent<PhotonView>();
            
            // El Owner se asignará automáticamente por Photon cuando el objeto se registre
            // No podemos asignarlo directamente ya que es de solo lectura
            
            if (enableDebugLogs)
                Debug.Log("✅ PhotonView creado en PlayerSyncManager");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("✅ PhotonView ya existe en PlayerSyncManager");
        }
    }
    
    void Start()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 PlayerSyncManager iniciado");
        
        // Asegurar que tenemos un PhotonView después de un delay más largo
        Invoke(nameof(EnsurePhotonView), 2f);
    }
    
    // Photon Callbacks
    public override void OnJoinedRoom()
    {
        if (enableDebugLogs)
            Debug.Log($"🔄 OnJoinedRoom - Players in room: {PhotonNetwork.CurrentRoom.PlayerCount}");
        
        // Si no somos el Master Client, necesitamos sincronizar jugadores existentes
        if (!PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SyncExistingPlayers());
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("✅ Somos Master Client - no necesitamos sincronizar");
        }
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (enableDebugLogs)
            Debug.Log($"🔄 Nuevo jugador entró: {newPlayer.NickName} (ID: {newPlayer.ActorNumber})");
        
        // Si somos el Master Client, enviar información de jugadores existentes al nuevo jugador
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SendExistingPlayersToNewPlayer(newPlayer));
        }
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (enableDebugLogs)
            Debug.Log($"🔄 Jugador salió: {otherPlayer.NickName} (ID: {otherPlayer.ActorNumber})");
        
        // Limpiar información del jugador que salió
        if (existingPlayers.ContainsKey(otherPlayer.ActorNumber))
        {
            existingPlayers.Remove(otherPlayer.ActorNumber);
        }
    }
    
    // Coroutine para sincronizar jugadores existentes (para clientes que entran)
    IEnumerator SyncExistingPlayers()
    {
        if (hasSyncedPlayers)
        {
            if (enableDebugLogs)
                Debug.Log("✅ Ya se sincronizaron los jugadores");
            yield break;
        }
        
        yield return new WaitForSeconds(syncDelay);
        
        if (enableDebugLogs)
            Debug.Log("🔄 Solicitando sincronización de jugadores existentes...");
        
        // Esperar a que Photon esté completamente conectado
        while (!PhotonNetwork.IsConnected)
        {
            if (enableDebugLogs)
                Debug.Log("⏳ Esperando conexión a Photon...");
            yield return new WaitForSeconds(0.5f);
        }
        
        // Asegurar que tenemos un PhotonView configurado
        EnsurePhotonView();
        
        // Verificar que tenemos PhotonView y que esté configurado
        if (photonView == null)
        {
            if (enableDebugLogs)
                Debug.LogError("❌ PhotonView no encontrado en PlayerSyncManager");
            yield break;
        }
        
        // Esperar a que el PhotonView esté completamente inicializado
        float initTimeout = 10f; // Aumentado el timeout
        float initElapsed = 0f;
        while (photonView.ViewID == 0 && initElapsed < initTimeout)
        {
            initElapsed += Time.deltaTime;
            if (enableDebugLogs && initElapsed > 5f)
                Debug.Log($"⏳ Esperando inicialización del PhotonView... {initElapsed:F1}s");
            yield return null;
        }
        
        if (photonView.ViewID == 0)
        {
            if (enableDebugLogs)
                Debug.LogError("❌ PhotonView no se inicializó correctamente después del timeout");
            
            // Intentar reconfigurar el PhotonView
            EnsurePhotonView();
            yield return new WaitForSeconds(1f);
            
            if (photonView.ViewID == 0)
            {
                if (enableDebugLogs)
                    Debug.LogError("❌ PhotonView sigue sin inicializarse, abortando sincronización");
                yield break;
            }
        }
        
        if (enableDebugLogs)
            Debug.Log($"✅ PhotonView inicializado correctamente con ViewID: {photonView.ViewID}");
        
        // Solicitar información de jugadores existentes al Master Client
        photonView.RPC("RequestExistingPlayers", RpcTarget.MasterClient);
        
        // Esperar respuesta con timeout
        float responseTimeout = 5f;
        float responseElapsed = 0f;
        
        while (!hasSyncedPlayers && responseElapsed < responseTimeout)
        {
            responseElapsed += Time.deltaTime;
            yield return null;
        }
        
        if (!hasSyncedPlayers)
        {
            syncAttempts++;
            if (syncAttempts < maxSyncAttempts)
            {
                if (enableDebugLogs)
                    Debug.Log($"⚠️ Timeout en sincronización, reintentando... ({syncAttempts}/{maxSyncAttempts})");
                StartCoroutine(SyncExistingPlayers());
            }
            else
            {
                if (enableDebugLogs)
                    Debug.LogError("❌ Falló la sincronización después de múltiples intentos");
            }
        }
    }
    
    // Coroutine para enviar información de jugadores existentes (para Master Client)
    IEnumerator SendExistingPlayersToNewPlayer(Player newPlayer)
    {
        yield return new WaitForSeconds(syncDelay);
        
        if (enableDebugLogs)
            Debug.Log($"🔄 Enviando información de jugadores existentes a {newPlayer.NickName}");
        
        // Asegurar que tenemos un PhotonView configurado
        EnsurePhotonView();
        
        // Verificar que tenemos PhotonView y que esté configurado
        if (photonView == null)
        {
            if (enableDebugLogs)
                Debug.LogError("❌ PhotonView no encontrado en PlayerSyncManager");
            yield break;
        }
        
        // Esperar a que el PhotonView esté completamente inicializado
        float sendTimeout = 10f; // Aumentado el timeout
        float sendElapsed = 0f;
        while (photonView.ViewID == 0 && sendElapsed < sendTimeout)
        {
            sendElapsed += Time.deltaTime;
            if (enableDebugLogs && sendElapsed > 5f)
                Debug.Log($"⏳ Esperando inicialización del PhotonView para envío... {sendElapsed:F1}s");
            yield return null;
        }
        
        if (photonView.ViewID == 0)
        {
            if (enableDebugLogs)
                Debug.LogError("❌ PhotonView no se inicializó correctamente para envío");
            
            // Intentar reconfigurar el PhotonView
            EnsurePhotonView();
            yield return new WaitForSeconds(1f);
            
            if (photonView.ViewID == 0)
            {
                if (enableDebugLogs)
                    Debug.LogError("❌ PhotonView sigue sin inicializarse para envío, abortando");
                yield break;
            }
        }
        
        if (enableDebugLogs)
            Debug.Log($"✅ PhotonView listo para envío con ViewID: {photonView.ViewID}");
        
        // Recolectar información de jugadores existentes
        CollectExistingPlayersInfo();
        
        // Enviar información al nuevo jugador
        if (existingPlayers.Count > 0)
        {
            photonView.RPC("ReceiveExistingPlayers", newPlayer, existingPlayers);
            if (enableDebugLogs)
                Debug.Log($"✅ Enviada información de {existingPlayers.Count} jugadores a {newPlayer.NickName}");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("ℹ️ No hay jugadores existentes para sincronizar");
        }
    }
    
    // Recolectar información de jugadores existentes en la sala
    void CollectExistingPlayersInfo()
    {
        existingPlayers.Clear();
        
        // Buscar todos los PlayerController en la escena
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        
        foreach (PlayerController player in players)
        {
            if (player != null && player.photonView != null)
            {
                PlayerInfo info = new PlayerInfo(
                    player.photonView.OwnerActorNr,
                    player.name,
                    player.transform.position,
                    player.transform.rotation
                );
                
                // Obtener información adicional del jugador
                info.isSpinning = player.IsSpinning();
                info.health = player.CurrentHealth;
                
                if (player.Rigidbody != null)
                {
                    info.velocity = player.Rigidbody.linearVelocity;
                }
                
                existingPlayers[player.photonView.OwnerActorNr] = info;
                
                if (enableDebugLogs)
                    Debug.Log($"📋 Recolectada info de jugador: {player.name} (ID: {player.photonView.OwnerActorNr})");
            }
        }
    }
    
    // RPC: Solicitar información de jugadores existentes
    [PunRPC]
    void RequestExistingPlayers()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Recibida solicitud de sincronización de jugadores");
        
        // Recolectar y enviar información de jugadores existentes
        CollectExistingPlayersInfo();
        
        if (existingPlayers.Count > 0)
        {
            photonView.RPC("ReceiveExistingPlayers", RpcTarget.Others, existingPlayers);
            if (enableDebugLogs)
                Debug.Log($"✅ Enviada información de {existingPlayers.Count} jugadores");
        }
    }
    
    // RPC: Recibir información de jugadores existentes
    [PunRPC]
    void ReceiveExistingPlayers(Dictionary<int, PlayerInfo> playersInfo)
    {
        if (enableDebugLogs)
            Debug.Log($"🔄 Recibida información de {playersInfo.Count} jugadores existentes");
        
        // Crear instancias de jugadores existentes
        foreach (var kvp in playersInfo)
        {
            PlayerInfo info = kvp.Value;
            
            // Verificar si el jugador ya existe
            PlayerController existingPlayer = FindPlayerByActorNumber(info.actorNumber);
            if (existingPlayer == null)
            {
                // Crear nueva instancia del jugador
                CreatePlayerInstance(info);
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log($"ℹ️ Jugador {info.playerName} ya existe, actualizando posición");
                UpdatePlayerInstance(existingPlayer, info);
            }
        }
        
        hasSyncedPlayers = true;
        if (enableDebugLogs)
            Debug.Log("✅ Sincronización de jugadores completada");
    }
    
    // Buscar jugador por ActorNumber
    PlayerController FindPlayerByActorNumber(int actorNumber)
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        
        foreach (PlayerController player in players)
        {
            if (player != null && player.photonView != null && 
                player.photonView.OwnerActorNr == actorNumber)
            {
                return player;
            }
        }
        
        return null;
    }
    
    // Crear nueva instancia de jugador
    void CreatePlayerInstance(PlayerInfo info)
    {
        if (enableDebugLogs)
            Debug.Log($"🔄 Creando instancia de jugador: {info.playerName} en {info.position}");
        
        // Instanciar jugador en la posición correcta
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabName, info.position, info.rotation);
        
        if (playerObj != null)
        {
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Configurar el jugador
                playerController.name = info.playerName;
                // No podemos asignar OwnerActorNr directamente ya que es de solo lectura
                // Photon asignará automáticamente el OwnerActorNr correcto
                
                // Aplicar estado del jugador
                ApplyPlayerState(playerController, info);
                
                if (enableDebugLogs)
                    Debug.Log($"✅ Jugador {info.playerName} creado exitosamente");
            }
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogError($"❌ Falló al crear jugador {info.playerName}");
        }
    }
    
    // Actualizar instancia existente de jugador
    void UpdatePlayerInstance(PlayerController player, PlayerInfo info)
    {
        // Actualizar posición y rotación
        player.transform.position = info.position;
        player.transform.rotation = info.rotation;
        
        // Aplicar estado del jugador
        ApplyPlayerState(player, info);
    }
    
    // Aplicar estado del jugador
    void ApplyPlayerState(PlayerController player, PlayerInfo info)
    {
        // Aplicar velocidad si tiene Rigidbody
        if (player.Rigidbody != null)
        {
            player.Rigidbody.linearVelocity = info.velocity;
        }
        
        // Aplicar estado de giro
        if (info.isSpinning)
        {
            player.StartSpin();
        }
        
        // Notificar al GameManager si es necesario
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateHealthUI(info.health, player.MaxHealth);
        }
    }
    
    // Métodos públicos para control manual
    [ContextMenu("Force Sync Players")]
    public void ForceSyncPlayers()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Forzando sincronización de jugadores...");
        
        hasSyncedPlayers = false;
        syncAttempts = 0;
        
        if (!PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SyncExistingPlayers());
        }
        else
        {
            CollectExistingPlayersInfo();
            if (enableDebugLogs)
                Debug.Log($"📋 Información de {existingPlayers.Count} jugadores recolectada");
        }
    }
    
    [ContextMenu("Check Player Status")]
    public void CheckPlayerStatus()
    {
        Debug.Log("🔍 Estado de sincronización de jugadores:");
        Debug.Log($"   Sincronizado: {hasSyncedPlayers}");
        Debug.Log($"   Intentos: {syncAttempts}/{maxSyncAttempts}");
        Debug.Log($"   Jugadores en diccionario: {existingPlayers.Count}");
        
        // Verificar estado del PhotonView
        CheckPhotonViewStatus();
        
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        Debug.Log($"   PlayerController en escena: {players.Length}");
        
        foreach (PlayerController player in players)
        {
            if (player != null && player.photonView != null)
            {
                Debug.Log($"     - {player.name} (ID: {player.photonView.OwnerActorNr}, IsMine: {player.photonView.IsMine})");
            }
        }
    }
    
    void CheckPhotonViewStatus()
    {
        Debug.Log("📡 Estado del PhotonView:");
        Debug.Log($"   PhotonView existe: {(photonView != null ? "✅" : "❌")}");
        
        if (photonView != null)
        {
            Debug.Log($"   ViewID: {photonView.ViewID}");
            Debug.Log($"   Owner: {photonView.Owner?.NickName ?? "None"}");
            Debug.Log($"   IsMine: {photonView.IsMine}");
            Debug.Log($"   Observed Components: {photonView.ObservedComponents?.Count ?? 0}");
        }
        else
        {
            Debug.LogWarning("⚠️ PhotonView es null, intentando crear uno...");
            EnsurePhotonView();
        }
    }
    
    [ContextMenu("Clear Sync State")]
    public void ClearSyncState()
    {
        hasSyncedPlayers = false;
        syncAttempts = 0;
        existingPlayers.Clear();
        
        if (enableDebugLogs)
            Debug.Log("🔄 Estado de sincronización limpiado");
    }
    
    [ContextMenu("Force Reconfigure PhotonView")]
    public void ForceReconfigurePhotonView()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Forzando reconfiguración del PhotonView...");
        
        // Remover el PhotonView existente si existe
        if (photonView != null)
        {
            DestroyImmediate(photonView);
            if (enableDebugLogs)
                Debug.Log("🗑️ PhotonView anterior removido");
        }
        
        // Crear un nuevo PhotonView
        EnsurePhotonView();
        
        if (enableDebugLogs)
            Debug.Log("✅ PhotonView reconfigurado");
    }
}
