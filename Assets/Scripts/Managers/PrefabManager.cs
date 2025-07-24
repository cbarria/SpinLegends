using UnityEngine;
using Photon.Pun;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PrefabManager : MonoBehaviour
{
    [Header("Prefab Configuration")]
    public string playerPrefabName = "playerspin";
    public bool autoSetupOnStart = true;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupPrefabForPhoton();
        }
    }
    
    [ContextMenu("Setup Player Prefab for Photon")]
    public void SetupPrefabForPhoton()
    {
        Debug.Log("🔧 Configurando prefab del jugador para Photon...");
        
        // 1. Mover prefab a Resources si no está ahí
        MovePrefabToResources();
        
        // 2. Verificar y configurar componentes
        VerifyPrefabSetup();
        
        Debug.Log("✅ Prefab del jugador configurado para Photon");
    }
    
    void MovePrefabToResources()
    {
        // Verificar si el prefab ya está en Resources
        GameObject prefabInResources = Resources.Load<GameObject>(playerPrefabName);
        if (prefabInResources != null)
        {
            Debug.Log("✅ Prefab encontrado en Resources: " + playerPrefabName);
            return;
        }
        
        #if UNITY_EDITOR
        // Buscar el prefab en Assets/Prefabs
        string[] guids = AssetDatabase.FindAssets(playerPrefabName + " t:Prefab");
        if (guids.Length > 0)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            string resourcesPath = "Assets/Resources/";
            
            // Crear carpeta Resources si no existe
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            
            // Copiar prefab a Resources
            string newPath = resourcesPath + playerPrefabName + ".prefab";
            AssetDatabase.CopyAsset(prefabPath, newPath);
            AssetDatabase.Refresh();
            
            Debug.Log("✅ Prefab movido a Resources: " + newPath);
        }
        else
        {
            Debug.LogError("❌ No se encontró el prefab: " + playerPrefabName);
        }
        #else
        Debug.LogWarning("⚠️ No se puede mover el prefab en build. Asegúrate de que esté en Resources.");
        #endif
    }
    
    void VerifyPrefabSetup()
    {
        GameObject prefab = Resources.Load<GameObject>(playerPrefabName);
        if (prefab == null)
        {
            Debug.LogError("❌ No se pudo cargar el prefab desde Resources: " + playerPrefabName);
            return;
        }
        
        #if UNITY_EDITOR
        // Verificar y agregar PhotonView
        PhotonView photonView = prefab.GetComponent<PhotonView>();
        if (photonView == null)
        {
            photonView = prefab.AddComponent<PhotonView>();
            Debug.Log("✅ PhotonView agregado al prefab");
        }
        
        // Configurar PhotonView
        photonView.ObservedComponents = new System.Collections.Generic.List<Component>();
        
        // Agregar PlayerController como componente observado
        PlayerController playerController = prefab.GetComponent<PlayerController>();
        if (playerController != null)
        {
            if (!photonView.ObservedComponents.Contains(playerController))
            {
                photonView.ObservedComponents.Add(playerController);
                Debug.Log("✅ PlayerController agregado como componente observado");
            }
        }
        else
        {
            Debug.LogError("❌ PlayerController no encontrado en el prefab!");
        }
        
        // Agregar NetworkPlayerCollision como componente observado
        NetworkPlayerCollision collision = prefab.GetComponent<NetworkPlayerCollision>();
        if (collision == null)
        {
            collision = prefab.AddComponent<NetworkPlayerCollision>();
            Debug.Log("✅ NetworkPlayerCollision agregado al prefab");
        }
        
        if (!photonView.ObservedComponents.Contains(collision))
        {
            photonView.ObservedComponents.Add(collision);
            Debug.Log("✅ NetworkPlayerCollision agregado como componente observado");
        }
        
        // Configurar Ownership Transfer
        photonView.OwnershipTransfer = OwnershipOption.Takeover;
        
        // Configurar Rigidbody para mejor rendimiento de red
        Rigidbody rb = prefab.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Configurar interpolación para movimiento suave
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            
            // Configurar colisiones
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            // Configurar drag para mejor control
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
            
            Debug.Log("✅ Rigidbody configurado para mejor rendimiento de red");
        }
        
        // Configurar Collider para colisiones
        Collider collider = prefab.GetComponent<Collider>();
        if (collider != null)
        {
            // Asegurar que el collider esté configurado para colisiones
            collider.isTrigger = false;
            
            // Configurar material de física si es necesario
            if (collider is SphereCollider || collider is CapsuleCollider)
            {
                // Configurar para colisiones de beyblade
                collider.material = null; // Usar material por defecto
            }
            
            Debug.Log("✅ Collider configurado para colisiones");
        }
        
        // Configurar tag para colisiones
        if (prefab.tag != "Player")
        {
            prefab.tag = "Player";
            Debug.Log("✅ Tag 'Player' configurado en el prefab");
        }
        
        // Marcar el prefab como modificado
        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();
        
        Debug.Log("✅ Configuración del prefab completada");
        #else
        Debug.Log("✅ Verificación del prefab completada (modo build)");
        #endif
    }
    
    [ContextMenu("Test Prefab Loading")]
    public void TestPrefabLoading()
    {
        GameObject prefab = Resources.Load<GameObject>(playerPrefabName);
        if (prefab != null)
        {
            Debug.Log("✅ Prefab cargado correctamente desde Resources");
            
            // Verificar componentes
            PhotonView pv = prefab.GetComponent<PhotonView>();
            PlayerController pc = prefab.GetComponent<PlayerController>();
            NetworkPlayerCollision npc = prefab.GetComponent<NetworkPlayerCollision>();
            Rigidbody rb = prefab.GetComponent<Rigidbody>();
            
            Debug.Log($"PhotonView: {(pv != null ? "✅" : "❌")}");
            Debug.Log($"PlayerController: {(pc != null ? "✅" : "❌")}");
            Debug.Log($"NetworkPlayerCollision: {(npc != null ? "✅" : "❌")}");
            Debug.Log($"Rigidbody: {(rb != null ? "✅" : "❌")}");
            
            if (pv != null)
            {
                Debug.Log($"Componentes observados: {pv.ObservedComponents.Count}");
            }
        }
        else
        {
            Debug.LogError("❌ No se pudo cargar el prefab desde Resources");
        }
    }
} 