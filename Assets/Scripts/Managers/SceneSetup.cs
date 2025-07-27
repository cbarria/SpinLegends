using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Added for List

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
        Debug.Log("🔧 Setting up scene...");
        
        // Clean up any duplicate UI elements first
        CleanupDuplicateUI();
        
        // Find or create Canvas
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Find MultiplayerUI
        multiplayerUI = FindFirstObjectByType<MultiplayerUI>();
        if (multiplayerUI == null)
        {
            // Check if MultiplayerUI already exists by name
            GameObject existingMultiplayerUI = GameObject.Find("MultiplayerUI");
            if (existingMultiplayerUI != null)
            {
                multiplayerUI = existingMultiplayerUI.GetComponent<MultiplayerUI>();
                if (multiplayerUI == null)
                {
                    multiplayerUI = existingMultiplayerUI.AddComponent<MultiplayerUI>();
                }
                Debug.Log("✅ Found existing MultiplayerUI");
            }
            else
            {
                GameObject multiplayerUIObj = new GameObject("MultiplayerUI");
                multiplayerUI = multiplayerUIObj.AddComponent<MultiplayerUI>();
                Debug.Log("✅ Created new MultiplayerUI");
            }
        }
        else
        {
            Debug.Log("✅ MultiplayerUI already found");
        }
        
        // Configure UI
        SetupStatusUI();
        SetupRoomInfoUI();
        
        Debug.Log("✅ Scene setup completed");
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
        // Check if StatusPanel already exists
        if (GameObject.Find("StatusPanel") != null)
        {
            Debug.Log("✅ StatusPanel already exists, skipping creation");
            return;
        }

        // Create status panel in top-right corner
        GameObject statusPanel = new GameObject("StatusPanel");
        statusPanel.transform.SetParent(canvas.transform);
        
        // Configurar panel de estado
        Image statusPanelImage = statusPanel.AddComponent<Image>();
        statusPanelImage.color = new Color(0, 0, 0, 0.3f); // More transparent
        
        RectTransform statusPanelRect = statusPanel.GetComponent<RectTransform>();
        statusPanelRect.anchorMin = new Vector2(0.7f, 0.8f);
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
        statusText.fontSize = 11;
        statusText.color = Color.white;
        statusText.fontStyle = FontStyles.Bold;
        statusText.alignment = TextAlignmentOptions.Left;
        statusText.text = "Connecting...";
        statusText.textWrappingMode = TextWrappingModes.Normal;
        
        RectTransform statusTextRect = statusText.GetComponent<RectTransform>();
        statusTextRect.anchorMin = new Vector2(0.25f, 0);
        statusTextRect.anchorMax = new Vector2(1f, 1f);
        statusTextRect.offsetMin = new Vector2(5, 8);
        statusTextRect.offsetMax = new Vector2(-5, -8);
        
        // Assign to MultiplayerUI
        if (multiplayerUI != null)
        {
            multiplayerUI.statusPanel = statusPanel;
            multiplayerUI.statusText = statusText;
            multiplayerUI.statusIcon = statusIcon;
        }
        
        Debug.Log("✅ Status panel created in top-right corner");
    }
    
    void SetupRoomInfoUI()
    {
        // Check if RoomInfoPanel already exists
        if (GameObject.Find("RoomInfoPanel") != null)
        {
            Debug.Log("✅ RoomInfoPanel already exists, skipping creation");
            return;
        }

        // Create room info panel in top-left corner
        GameObject roomInfoPanel = new GameObject("RoomInfoPanel");
        roomInfoPanel.transform.SetParent(canvas.transform);
        
        // Configure room info panel
        Image roomInfoPanelImage = roomInfoPanel.AddComponent<Image>();
        roomInfoPanelImage.color = new Color(0, 0, 0, 0.3f); // More transparent
        
        RectTransform roomInfoPanelRect = roomInfoPanel.GetComponent<RectTransform>();
        roomInfoPanelRect.anchorMin = new Vector2(0.02f, 0.85f);
        roomInfoPanelRect.anchorMax = new Vector2(0.55f, 0.98f); // Wider panel for long room names
        roomInfoPanelRect.offsetMin = Vector2.zero;
        roomInfoPanelRect.offsetMax = Vector2.zero;
        
        // Create room icon
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
        
        // Create room name text
        GameObject roomNameTextObj = new GameObject("RoomNameText");
        roomNameTextObj.transform.SetParent(roomInfoPanel.transform);
        
        TextMeshProUGUI roomNameText = roomNameTextObj.AddComponent<TextMeshProUGUI>();
        roomNameText.fontSize = 11;
        roomNameText.color = Color.white;
        roomNameText.fontStyle = FontStyles.Bold;
        roomNameText.alignment = TextAlignmentOptions.Left;
        roomNameText.text = "Room";
        
        RectTransform roomNameTextRect = roomNameText.GetComponent<RectTransform>();
        roomNameTextRect.anchorMin = new Vector2(0.2f, 0.5f);
        roomNameTextRect.anchorMax = new Vector2(1f, 1f);
        roomNameTextRect.offsetMin = new Vector2(5, 8);
        roomNameTextRect.offsetMax = new Vector2(-5, -8);
        
        // Create player icon
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
        
        // Create player count text
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
        playerCountTextRect.offsetMin = new Vector2(5, 8);
        playerCountTextRect.offsetMax = new Vector2(-5, -8);
        
        // Assign to MultiplayerUI
        if (multiplayerUI != null)
        {
            multiplayerUI.roomInfoPanel = roomInfoPanel;
            multiplayerUI.roomNameText = roomNameText;
            multiplayerUI.playerCountText = playerCountText;
            multiplayerUI.roomIcon = roomIcon;
            multiplayerUI.playerIcon = playerIcon;
        }
        
        Debug.Log("✅ Room info panel created in top-left corner");
    }
    
    void CleanupDuplicateUI()
    {
        // Find all StatusPanel objects
        GameObject[] statusPanels = GameObject.FindGameObjectsWithTag("Untagged");
        List<GameObject> duplicateStatusPanels = new List<GameObject>();
        List<GameObject> duplicateRoomInfoPanels = new List<GameObject>();
        List<GameObject> duplicateMultiplayerUIs = new List<GameObject>();
        
        foreach (GameObject obj in statusPanels)
        {
            if (obj.name == "StatusPanel")
            {
                duplicateStatusPanels.Add(obj);
            }
            else if (obj.name == "RoomInfoPanel")
            {
                duplicateRoomInfoPanels.Add(obj);
            }
            else if (obj.name == "MultiplayerUI")
            {
                duplicateMultiplayerUIs.Add(obj);
            }
        }
        
        // Keep only the first instance of each, destroy the rest
        for (int i = 1; i < duplicateStatusPanels.Count; i++)
        {
            Debug.Log($"🗑️ Destroying duplicate StatusPanel {i}");
            DestroyImmediate(duplicateStatusPanels[i]);
        }
        
        for (int i = 1; i < duplicateRoomInfoPanels.Count; i++)
        {
            Debug.Log($"🗑️ Destroying duplicate RoomInfoPanel {i}");
            DestroyImmediate(duplicateRoomInfoPanels[i]);
        }
        
        for (int i = 1; i < duplicateMultiplayerUIs.Count; i++)
        {
            Debug.Log($"🗑️ Destroying duplicate MultiplayerUI {i}");
            DestroyImmediate(duplicateMultiplayerUIs[i]);
        }
        
        if (duplicateStatusPanels.Count > 1 || duplicateRoomInfoPanels.Count > 1 || duplicateMultiplayerUIs.Count > 1)
        {
            Debug.Log("🧹 Cleaned up duplicate UI elements");
        }
    }
} 