using UnityEngine;
using Photon.Pun;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AutoSetup : MonoBehaviour
{
    [Header("Auto Setup Settings")]
    public bool autoSetupOnStart = true;
    public bool createSpawnPoints = true;
    public int numberOfSpawnPoints = 4;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            // Primero configurar el prefab
            SetupPrefabForPhoton();
            
            // Luego configurar el multiplayer
            SetupMultiplayerComponents();
        }
    }
    
    void SetupPrefabForPhoton()
    {
        PrefabManager prefabManager = FindFirstObjectByType<PrefabManager>();
        if (prefabManager == null)
        {
            GameObject prefabManagerObj = new GameObject("PrefabManager");
            prefabManager = prefabManagerObj.AddComponent<PrefabManager>();
        }
        
        prefabManager.SetupPrefabForPhoton();
    }
    
    [ContextMenu("Setup Multiplayer Components")]
    public void SetupMultiplayerComponents()
    {
        Debug.Log("🔧 Setting up multiplayer components automatically...");
        
        // 1. Setup NetworkManager
        SetupNetworkManager();
        
        // 2. Setup Spawn Points
        if (createSpawnPoints)
        {
            SetupSpawnPoints();
        }
        
        // 3. Setup Player Prefab
        SetupPlayerPrefab();
        
        // 4. Setup UI and Scene
        SetupUI();
        SetupScene();
        
        Debug.Log("✅ Multiplayer setup completed!");
    }
    
    void SetupNetworkManager()
    {
        NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager == null)
        {
            GameObject networkManagerObj = new GameObject("NetworkManager");
            networkManager = networkManagerObj.AddComponent<NetworkManager>();
            Debug.Log("NetworkManager created");
        }
        else
        {
            Debug.Log("NetworkManager already exists");
        }
    }
    
    void SetupSpawnPoints()
    {
        // Buscar spawn points existentes
        Transform[] existingSpawnPoints = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        System.Collections.Generic.List<Transform> spawnPoints = new System.Collections.Generic.List<Transform>();
        
        foreach (Transform t in existingSpawnPoints)
        {
            if (t.name.ToLower().Contains("spawn"))
            {
                spawnPoints.Add(t);
            }
        }
        
        // Si no hay spawn points, crear algunos
        if (spawnPoints.Count == 0)
        {
            Debug.Log("No spawn points found. Creating default spawn points...");
            
            for (int i = 0; i < numberOfSpawnPoints; i++)
            {
                GameObject spawnPoint = new GameObject($"SpawnPoint_{i + 1}");
                
                // Posicionar en un círculo alrededor del centro
                float angle = i * (360f / numberOfSpawnPoints);
                float radius = 5f;
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                
                spawnPoint.transform.position = new Vector3(x, 1f, z);
                spawnPoints.Add(spawnPoint.transform);
                
                Debug.Log($"Created spawn point {i + 1} at position {spawnPoint.transform.position}");
            }
        }
        
        // Asignar spawn points al NetworkManager
        NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager != null)
        {
            networkManager.spawnPoints = spawnPoints.ToArray();
            Debug.Log($"Assigned {spawnPoints.Count} spawn points to NetworkManager");
        }
    }
    
    void SetupPlayerPrefab()
    {
        NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager == null) return;
        
        // Buscar el prefab playerspin en diferentes ubicaciones
        GameObject playerPrefab = null;
        
        // 1. Buscar en Resources/playerspin
        playerPrefab = Resources.Load<GameObject>("playerspin");
        
        // 2. Si no está en Resources, buscar en Assets/Prefabs
        if (playerPrefab == null)
        {
            // Intentar cargar desde Assets/Prefabs usando AssetDatabase (solo en editor)
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("playerspin t:Prefab");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                playerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Debug.Log($"Found player prefab at: {path}");
            }
            #endif
        }
        
        if (playerPrefab != null)
        {
            networkManager.playerPrefab = playerPrefab;
            Debug.Log("Player prefab assigned to NetworkManager");
            
            // Verificar que tenga PhotonView
            PhotonView photonView = playerPrefab.GetComponent<PhotonView>();
            if (photonView == null)
            {
                Debug.LogWarning("Player prefab doesn't have PhotonView! Adding it automatically...");
                #if UNITY_EDITOR
                photonView = playerPrefab.AddComponent<PhotonView>();
                
                // Configurar PhotonView
                photonView.ObservedComponents = new System.Collections.Generic.List<Component>();
                
                // Agregar PlayerController como componente observado
                PlayerController playerController = playerPrefab.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    photonView.ObservedComponents.Add(playerController);
                    Debug.Log("PlayerController added as observed component");
                }
                
                // Agregar NetworkPlayerCollision si existe
                NetworkPlayerCollision collision = playerPrefab.GetComponent<NetworkPlayerCollision>();
                if (collision == null)
                {
                    collision = playerPrefab.AddComponent<NetworkPlayerCollision>();
                    Debug.Log("NetworkPlayerCollision component added");
                }
                photonView.ObservedComponents.Add(collision);
                Debug.Log("NetworkPlayerCollision added as observed component");
                
                // Configurar Ownership Transfer
                photonView.OwnershipTransfer = OwnershipOption.Takeover;
                
                // Marcar el prefab como modificado
                EditorUtility.SetDirty(playerPrefab);
                AssetDatabase.SaveAssets();
                Debug.Log("PhotonView added and configured to player prefab");
                #endif
            }
            else
            {
                Debug.Log("Player prefab has PhotonView configured");
            }
        }
        else
        {
            Debug.LogError("Player prefab not found! Please ensure 'playerspin' prefab exists and has PhotonView component.");
        }
    }
    
    void SetupUI()
    {
        MultiplayerUI multiplayerUI = FindFirstObjectByType<MultiplayerUI>();
        if (multiplayerUI == null)
        {
            Debug.Log("MultiplayerUI not found. You may need to add it manually to the scene.");
        }
        else
        {
            Debug.Log("MultiplayerUI found and configured");
        }
    }
    
    void SetupScene()
    {
        // Usar SceneSetup para configurar la escena completa
        SceneSetup sceneSetup = FindFirstObjectByType<SceneSetup>();
        if (sceneSetup == null)
        {
            GameObject sceneSetupObj = new GameObject("SceneSetup");
            sceneSetup = sceneSetupObj.AddComponent<SceneSetup>();
        }
        
        sceneSetup.SetupScene();
        
        // Configurar Android si es necesario
        SetupAndroidConfiguration();
        
        // Configurar y arreglar el joystick
        SetupJoystickFixer();
    }
    
    void SetupAndroidConfiguration()
    {
        Debug.Log("🔧 Configurando Android...");
        
        // Configurar AndroidSettings
        AndroidSettings androidSettings = FindFirstObjectByType<AndroidSettings>();
        if (androidSettings == null)
        {
            GameObject androidSettingsObj = new GameObject("AndroidSettings");
            androidSettings = androidSettingsObj.AddComponent<AndroidSettings>();
        }
        
        // Configurar AndroidDebug
        AndroidDebug androidDebug = FindFirstObjectByType<AndroidDebug>();
        if (androidDebug == null)
        {
            GameObject androidDebugObj = new GameObject("AndroidDebug");
            androidDebug = androidDebugObj.AddComponent<AndroidDebug>();
        }
        
        Debug.Log("✅ Configuración de Android completada");
    }
    
    void SetupJoystickFixer()
    {
        Debug.Log("🔧 Configurando JoystickFixer...");
        
        // Configurar JoystickFixer
        JoystickFixer joystickFixer = FindFirstObjectByType<JoystickFixer>();
        if (joystickFixer == null)
        {
            GameObject joystickFixerObj = new GameObject("JoystickFixer");
            joystickFixer = joystickFixerObj.AddComponent<JoystickFixer>();
        }
        
        Debug.Log("✅ JoystickFixer configurado");
        
        // Configurar JoystickInputTester para debugging
        SetupJoystickInputTester();
    }
    
    void SetupJoystickInputTester()
    {
        Debug.Log("🔧 Configurando JoystickInputTester...");
        
        // Configurar JoystickInputTester
        JoystickInputTester joystickTester = FindFirstObjectByType<JoystickInputTester>();
        if (joystickTester == null)
        {
            GameObject joystickTesterObj = new GameObject("JoystickInputTester");
            joystickTester = joystickTesterObj.AddComponent<JoystickInputTester>();
        }
        
        Debug.Log("✅ JoystickInputTester configurado");
        
        // Configurar AndroidJoystickEnabler específicamente para Android
        SetupAndroidJoystickEnabler();
    }
    
    void SetupAndroidJoystickEnabler()
    {
        Debug.Log("🔧 Configurando AndroidJoystickEnabler...");
        
        // Configurar AndroidJoystickEnabler
        AndroidJoystickEnabler androidJoystickEnabler = FindFirstObjectByType<AndroidJoystickEnabler>();
        if (androidJoystickEnabler == null)
        {
            GameObject androidJoystickEnablerObj = new GameObject("AndroidJoystickEnabler");
            androidJoystickEnabler = androidJoystickEnablerObj.AddComponent<AndroidJoystickEnabler>();
        }
        
        Debug.Log("✅ AndroidJoystickEnabler configurado");
    }
    
    [ContextMenu("Verify Setup")]
    public void VerifySetup()
    {
        Debug.Log("🔍 Verifying multiplayer setup...");
        
        // Verificar NetworkManager
        NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("❌ NetworkManager not found!");
        }
        else
        {
            Debug.Log("✅ NetworkManager found");
            
            if (networkManager.playerPrefab == null)
                Debug.LogWarning("⚠️ Player prefab not assigned");
            else
                Debug.Log("✅ Player prefab assigned");
                
            if (networkManager.spawnPoints == null || networkManager.spawnPoints.Length == 0)
                Debug.LogWarning("⚠️ No spawn points assigned");
            else
                Debug.Log($"✅ {networkManager.spawnPoints.Length} spawn points assigned");
        }
        
        // Verificar Photon configuration
        if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime))
        {
            Debug.LogError("❌ Photon App ID not configured!");
        }
        else
        {
            Debug.Log("✅ Photon App ID configured");
        }
        
        Debug.Log("🎯 Verification completed!");
    }
} 