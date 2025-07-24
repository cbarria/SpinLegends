using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneSetup : MonoBehaviour
{
    [Header("Setup Settings")]
    public bool autoSetupOnStart = true;
    
    // Variables de clase para compartir entre métodos
    private Canvas canvas;
    private MultiplayerUI multiplayerUI;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        Debug.Log("🔧 Configurando escena...");
        
        // Buscar o crear Canvas
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Buscar MultiplayerUI
        multiplayerUI = FindFirstObjectByType<MultiplayerUI>();
        if (multiplayerUI == null)
        {
            GameObject multiplayerUIObj = new GameObject("MultiplayerUI");
            multiplayerUI = multiplayerUIObj.AddComponent<MultiplayerUI>();
        }
        
        // Configurar UI
        SetupStatusUI();
        SetupRoomInfoUI();
        
        Debug.Log("✅ Configuración de escena completada");
    }
    
    void SetupNetworkManager()
    {
        NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager == null)
        {
            GameObject networkManagerObj = new GameObject("NetworkManager");
            networkManager = networkManagerObj.AddComponent<NetworkManager>();
            Debug.Log("✅ NetworkManager created");
        }
        else
        {
            Debug.Log("✅ NetworkManager found");
        }
    }
    
    void SetupPlayerSpawnManager()
    {
        PlayerSpawnManager spawnManager = FindFirstObjectByType<PlayerSpawnManager>();
        if (spawnManager == null)
        {
            GameObject spawnManagerObj = new GameObject("PlayerSpawnManager");
            spawnManager = spawnManagerObj.AddComponent<PlayerSpawnManager>();
            Debug.Log("✅ PlayerSpawnManager created");
        }
        else
        {
            Debug.Log("✅ PlayerSpawnManager found");
        }
    }
    
    void SetupMultiplayerUI()
    {
        MultiplayerUI multiplayerUI = FindFirstObjectByType<MultiplayerUI>();
        if (multiplayerUI == null)
        {
            // Crear Canvas si no existe
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("✅ Canvas created");
            }
            
            // Crear MultiplayerUI
            GameObject uiObj = new GameObject("MultiplayerUI");
            uiObj.transform.SetParent(canvas.transform, false);
            multiplayerUI = uiObj.AddComponent<MultiplayerUI>();
            Debug.Log("✅ MultiplayerUI created");
        }
        else
        {
            Debug.Log("✅ MultiplayerUI found");
        }
    }
    
    void SetupPlayerPrefab()
    {
        // Verificar si el prefab existe en Resources
        GameObject prefab = Resources.Load<GameObject>("playerspin");
        if (prefab == null)
        {
            Debug.LogWarning("⚠️ Player prefab not found in Resources folder!");
            Debug.Log("Please move 'playerspin.prefab' to 'Assets/Resources/' folder");
        }
        else
        {
            Debug.Log("✅ Player prefab found in Resources");
        }
    }
    
    void SetupStatusUI()
    {
        // Crear panel de estado en la esquina superior derecha
        GameObject statusPanel = new GameObject("StatusPanel");
        statusPanel.transform.SetParent(canvas.transform);
        
        // Configurar panel de estado
        Image statusPanelImage = statusPanel.AddComponent<Image>();
        statusPanelImage.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform statusPanelRect = statusPanel.GetComponent<RectTransform>();
        statusPanelRect.anchorMin = new Vector2(0.7f, 0.85f);
        statusPanelRect.anchorMax = new Vector2(0.98f, 0.98f);
        statusPanelRect.offsetMin = Vector2.zero;
        statusPanelRect.offsetMax = Vector2.zero;
        
        // Crear icono de estado
        GameObject statusIconObj = new GameObject("StatusIcon");
        statusIconObj.transform.SetParent(statusPanel.transform);
        
        Image statusIcon = statusIconObj.AddComponent<Image>();
        statusIcon.color = Color.white;
        statusIcon.sprite = IconGenerator.CreateWifiIcon(Color.white, 24f);
        
        RectTransform statusIconRect = statusIcon.GetComponent<RectTransform>();
        statusIconRect.anchorMin = new Vector2(0, 0.5f);
        statusIconRect.anchorMax = new Vector2(0.2f, 1f);
        statusIconRect.offsetMin = new Vector2(5, 5);
        statusIconRect.offsetMax = new Vector2(-5, -5);
        
        // Crear texto de estado
        GameObject statusTextObj = new GameObject("StatusText");
        statusTextObj.transform.SetParent(statusPanel.transform);
        
        TextMeshProUGUI statusText = statusTextObj.AddComponent<TextMeshProUGUI>();
        statusText.fontSize = 12;
        statusText.color = Color.white;
        statusText.fontStyle = FontStyles.Bold;
        statusText.alignment = TextAlignmentOptions.Left;
        statusText.text = "Conectando...";
        
        RectTransform statusTextRect = statusText.GetComponent<RectTransform>();
        statusTextRect.anchorMin = new Vector2(0.25f, 0);
        statusTextRect.anchorMax = new Vector2(1f, 1f);
        statusTextRect.offsetMin = new Vector2(5, 5);
        statusTextRect.offsetMax = new Vector2(-5, -5);
        
        // Asignar al MultiplayerUI
        if (multiplayerUI != null)
        {
            multiplayerUI.statusPanel = statusPanel;
            multiplayerUI.statusText = statusText;
            multiplayerUI.statusIcon = statusIcon;
        }
        
        Debug.Log("✅ Panel de estado creado en esquina superior derecha");
    }
    
    void SetupRoomInfoUI()
    {
        // Crear panel de información de sala en la esquina superior izquierda
        GameObject roomInfoPanel = new GameObject("RoomInfoPanel");
        roomInfoPanel.transform.SetParent(canvas.transform);
        
        // Configurar panel de información de sala
        Image roomInfoPanelImage = roomInfoPanel.AddComponent<Image>();
        roomInfoPanelImage.color = new Color(0, 0, 0, 0.7f);
        
        RectTransform roomInfoPanelRect = roomInfoPanel.GetComponent<RectTransform>();
        roomInfoPanelRect.anchorMin = new Vector2(0.02f, 0.85f);
        roomInfoPanelRect.anchorMax = new Vector2(0.3f, 0.98f);
        roomInfoPanelRect.offsetMin = Vector2.zero;
        roomInfoPanelRect.offsetMax = Vector2.zero;
        
        // Crear icono de sala
        GameObject roomIconObj = new GameObject("RoomIcon");
        roomIconObj.transform.SetParent(roomInfoPanel.transform);
        
        Image roomIcon = roomIconObj.AddComponent<Image>();
        roomIcon.color = Color.cyan;
        roomIcon.sprite = IconGenerator.CreateRoomIcon(Color.cyan, 20f);
        
        RectTransform roomIconRect = roomIcon.GetComponent<RectTransform>();
        roomIconRect.anchorMin = new Vector2(0, 0.5f);
        roomIconRect.anchorMax = new Vector2(0.15f, 1f);
        roomIconRect.offsetMin = new Vector2(5, 5);
        roomIconRect.offsetMax = new Vector2(-5, -5);
        
        // Crear texto de nombre de sala
        GameObject roomNameTextObj = new GameObject("RoomNameText");
        roomNameTextObj.transform.SetParent(roomInfoPanel.transform);
        
        TextMeshProUGUI roomNameText = roomNameTextObj.AddComponent<TextMeshProUGUI>();
        roomNameText.fontSize = 11;
        roomNameText.color = Color.white;
        roomNameText.fontStyle = FontStyles.Bold;
        roomNameText.alignment = TextAlignmentOptions.Left;
        roomNameText.text = "Sala";
        
        RectTransform roomNameTextRect = roomNameText.GetComponent<RectTransform>();
        roomNameTextRect.anchorMin = new Vector2(0.2f, 0.5f);
        roomNameTextRect.anchorMax = new Vector2(1f, 1f);
        roomNameTextRect.offsetMin = new Vector2(5, 5);
        roomNameTextRect.offsetMax = new Vector2(-5, -5);
        
        // Crear icono de jugadores
        GameObject playerIconObj = new GameObject("PlayerIcon");
        playerIconObj.transform.SetParent(roomInfoPanel.transform);
        
        Image playerIcon = playerIconObj.AddComponent<Image>();
        playerIcon.color = Color.green;
        playerIcon.sprite = IconGenerator.CreatePlayerIcon(Color.green, 20f);
        
        RectTransform playerIconRect = playerIcon.GetComponent<RectTransform>();
        playerIconRect.anchorMin = new Vector2(0, 0);
        playerIconRect.anchorMax = new Vector2(0.15f, 0.5f);
        playerIconRect.offsetMin = new Vector2(5, 5);
        playerIconRect.offsetMax = new Vector2(-5, -5);
        
        // Crear texto de contador de jugadores
        GameObject playerCountTextObj = new GameObject("PlayerCountText");
        playerCountTextObj.transform.SetParent(roomInfoPanel.transform);
        
        TextMeshProUGUI playerCountText = playerCountTextObj.AddComponent<TextMeshProUGUI>();
        playerCountText.fontSize = 11;
        playerCountText.color = Color.white;
        playerCountText.fontStyle = FontStyles.Bold;
        playerCountText.alignment = TextAlignmentOptions.Left;
        playerCountText.text = "0/4";
        
        RectTransform playerCountTextRect = playerCountText.GetComponent<RectTransform>();
        playerCountTextRect.anchorMin = new Vector2(0.2f, 0);
        playerCountTextRect.anchorMax = new Vector2(1f, 0.5f);
        playerCountTextRect.offsetMin = new Vector2(5, 5);
        playerCountTextRect.offsetMax = new Vector2(-5, -5);
        
        // Asignar al MultiplayerUI
        if (multiplayerUI != null)
        {
            multiplayerUI.roomInfoPanel = roomInfoPanel;
            multiplayerUI.roomNameText = roomNameText;
            multiplayerUI.playerCountText = playerCountText;
            multiplayerUI.roomIcon = roomIcon;
            multiplayerUI.playerIcon = playerIcon;
        }
        
        Debug.Log("✅ Panel de información de sala creado en esquina superior izquierda");
    }
} 