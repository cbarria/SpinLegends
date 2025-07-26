using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MultiplayerUI : MonoBehaviour
{
    [Header("Connection UI")]
    public GameObject connectionPanel;
    public TextMeshProUGUI connectionStatusText;
    public Button connectButton;
    public Button disconnectButton;
    
    [Header("Status UI")]
    public GameObject statusPanel;
    public TextMeshProUGUI statusText;
    public Image statusIcon; // Nuevo: icono para el estado
    public Color connectedColor = Color.green;
    public Color connectingColor = Color.yellow;
    public Color disconnectedColor = Color.red;
    
    [Header("Room Info UI")]
    public GameObject roomInfoPanel;
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI playerCountText;
    public Image roomIcon; // Nuevo: icono para la sala
    public Image playerIcon; // Nuevo: icono para jugadores
    public Button leaveRoomButton;
    
    [Header("Player List")]
    public Transform playerListContent;
    public GameObject playerListItemPrefab;
    
    private NetworkManager networkManager;
    private string currentStatus = "";
    
    void Start()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found in MultiplayerUI!");
            return;
        }
        
        SetupUI();
        UpdateUI();
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    void SetupUI()
    {
        if (connectButton != null)
            connectButton.onClick.AddListener(OnConnectButtonPressed);
        
        if (disconnectButton != null)
            disconnectButton.onClick.AddListener(OnDisconnectButtonPressed);
        
        if (leaveRoomButton != null)
            leaveRoomButton.onClick.AddListener(OnLeaveRoomButtonPressed);
    }
    
    void UpdateUI()
    {
        if (networkManager == null) 
        {
            // Try to find NetworkManager again if it was lost
            networkManager = FindFirstObjectByType<NetworkManager>();
            if (networkManager == null) return;
        }
        
        // Actualizar estado de conexión
        UpdateConnectionUI();
        
        // Actualizar información de sala
        UpdateRoomUI();
        
        // Actualizar lista de jugadores
        UpdatePlayerList();
        
        // Actualizar texto de estado
        UpdateStatusText();
    }
    
    void UpdateConnectionUI()
    {
        if (connectionPanel == null || networkManager == null) return;
        
        bool isConnected = networkManager.IsConnected();
        connectionPanel.SetActive(!isConnected);
        
        if (connectionStatusText != null)
        {
            if (isConnected)
            {
                connectionStatusText.text = "Conectado al servidor";
                connectionStatusText.color = Color.green;
            }
            else
            {
                connectionStatusText.text = "Desconectado";
                connectionStatusText.color = Color.red;
            }
        }
    }
    
    void UpdateRoomUI()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
        {
            // Mostrar información de sala en panel separado
            if (roomInfoPanel != null)
            {
                roomInfoPanel.SetActive(true);
                
                // Actualizar nombre de sala
                if (roomNameText != null)
                {
                    roomNameText.text = PhotonNetwork.CurrentRoom.Name;
                }
                
                // Actualizar contador de jugadores
                if (playerCountText != null)
                {
                    int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
                    int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
                    playerCountText.text = $"{playerCount}/{maxPlayers}";
                    
                    // Cambiar color según el número de jugadores
                    if (playerCount >= maxPlayers)
                    {
                        playerCountText.color = Color.red;
                        if (playerIcon != null) playerIcon.color = Color.red;
                    }
                    else if (playerCount > 1)
                    {
                        playerCountText.color = Color.green;
                        if (playerIcon != null) playerIcon.color = Color.green;
                    }
                    else
                    {
                        playerCountText.color = Color.yellow;
                        if (playerIcon != null) playerIcon.color = Color.yellow;
                    }
                }
            }
            
            // Mostrar botón de salir sala
            if (leaveRoomButton != null)
            {
                leaveRoomButton.gameObject.SetActive(true);
            }
        }
        else
        {
            // Ocultar información de sala si no estamos en una
            if (roomInfoPanel != null)
            {
                roomInfoPanel.SetActive(false);
            }
            
            if (leaveRoomButton != null)
            {
                leaveRoomButton.gameObject.SetActive(false);
            }
        }
    }
    
    void UpdatePlayerList()
    {
        if (playerListContent == null || playerListItemPrefab == null) return;
        
        // Limpiar lista actual
        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }
        
        // Agregar jugadores actuales
        if (PhotonNetwork.InRoom && PhotonNetwork.PlayerList != null)
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (player == null) continue;
                
                GameObject playerItem = Instantiate(playerListItemPrefab, playerListContent);
                TextMeshProUGUI playerText = playerItem.GetComponentInChildren<TextMeshProUGUI>();
                if (playerText != null)
                {
                    string playerName = player.NickName ?? "Unknown";
                    if (player.IsMasterClient)
                        playerName += " (Host)";
                    if (player.IsLocal)
                        playerName += " (Tú)";
                    
                    playerText.text = playerName;
                }
            }
        }
    }
    
    void UpdateStatusText()
    {
        string currentStatus = GetCurrentStatus();
        
        if (statusText != null)
        {
            statusText.text = currentStatus;
            
            // Configurar color basado en el estado
            if (currentStatus.Contains("Conectado") && !currentStatus.Contains("Buscando"))
            {
                statusText.color = connectedColor;
                if (statusIcon != null)
                {
                    statusIcon.color = connectedColor;
                    // Cambiar icono a checkmark o wifi
                    statusIcon.sprite = GetStatusIcon("connected");
                }
            }
            else if (currentStatus.Contains("Conectando") || currentStatus.Contains("Esperando") || currentStatus.Contains("Buscando"))
            {
                statusText.color = connectingColor;
                if (statusIcon != null)
                {
                    statusIcon.color = connectingColor;
                    // Cambiar icono a loading
                    statusIcon.sprite = GetStatusIcon("connecting");
                }
            }
            else if (currentStatus.Contains("Error") || currentStatus.Contains("Desconectado"))
            {
                statusText.color = disconnectedColor;
                if (statusIcon != null)
                {
                    statusIcon.color = disconnectedColor;
                    // Cambiar icono a error
                    statusIcon.sprite = GetStatusIcon("disconnected");
                }
            }
            else
            {
                statusText.color = Color.white;
                if (statusIcon != null)
                {
                    statusIcon.color = Color.white;
                }
            }
            
            // Asegurar que el texto sea legible en Android
            #if UNITY_ANDROID
            if (statusText.fontSize > 16)
            {
                statusText.fontSize = 14; // Reducido de 24 a 14
                statusText.fontStyle = FontStyles.Bold;
            }
            #endif
        }
        
        // Mostrar/ocultar panel de estado
        if (statusPanel != null)
        {
            statusPanel.SetActive(!string.IsNullOrEmpty(currentStatus));
        }
    }
    
    string GetCurrentStatus()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return "Desconectado del servidor";
        }
        
        if (networkManager != null && networkManager.IsConnecting)
        {
            return "Conectando al servidor...";
        }
        
        if (PhotonNetwork.IsConnected && !PhotonNetwork.InRoom)
        {
            if (networkManager != null && networkManager.IsConnecting)
            {
                return "Esperando para unirse a una sala...";
            }
            return "Conectado al servidor - Buscando sala...";
        }
        
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
        {
            string roomName = PhotonNetwork.CurrentRoom.Name;
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
            return $"In room: {roomName} ({playerCount}/{maxPlayers})";
        }
        
        return "";
    }
    
    public void OnConnectButtonPressed()
    {
        if (networkManager != null)
        {
            networkManager.ConnectToServer();
        }
    }
    
    public void OnDisconnectButtonPressed()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }
    
    public void OnLeaveRoomButtonPressed()
    {
        if (networkManager != null)
        {
            networkManager.LeaveRoom();
        }
    }
    
    public void ShowConnectionPanel()
    {
        if (connectionPanel != null)
            connectionPanel.SetActive(true);
    }
    
    public void HideConnectionPanel()
    {
        if (connectionPanel != null)
            connectionPanel.SetActive(false);
    }
    
    // Método público para mostrar mensajes de estado personalizados
    public void ShowStatusMessage(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
            currentStatus = message;
        }
    }

    // Método para obtener iconos de estado (puedes crear sprites personalizados)
    Sprite GetStatusIcon(string status)
    {
        switch (status)
        {
            case "connected":
                return IconGenerator.CreateWifiIcon(connectedColor, 24f);
            case "connecting":
                return IconGenerator.CreateCircleIcon(connectingColor, 24f);
            case "disconnected":
                return IconGenerator.CreateCircleIcon(disconnectedColor, 24f);
            default:
                return IconGenerator.CreateCircleIcon(Color.white, 24f);
        }
    }
} 