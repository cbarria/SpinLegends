using UnityEngine;
using Photon.Pun;

public class PrefabSetup : MonoBehaviour
{
    [Header("Prefab Configuration")]
    public GameObject playerPrefab;
    
    [ContextMenu("Setup Player Prefab")]
    public void SetupPlayerPrefab()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab no asignado!");
            return;
        }
        
        // Agregar PhotonView si no existe
        PhotonView photonView = playerPrefab.GetComponent<PhotonView>();
        if (photonView == null)
        {
            photonView = playerPrefab.AddComponent<PhotonView>();
            Debug.Log("PhotonView agregado al player prefab");
        }
        
        // Configurar PhotonView
        photonView.ObservedComponents = new System.Collections.Generic.List<Component>();
        
        // Agregar PlayerController como componente observado
        PlayerController playerController = playerPrefab.GetComponent<PlayerController>();
        if (playerController != null)
        {
            if (!photonView.ObservedComponents.Contains(playerController))
            {
                photonView.ObservedComponents.Add(playerController);
                Debug.Log("PlayerController agregado como componente observado");
            }
        }
        
        // Agregar NetworkPlayerCollision si existe
        NetworkPlayerCollision collision = playerPrefab.GetComponent<NetworkPlayerCollision>();
        if (collision != null)
        {
            if (!photonView.ObservedComponents.Contains(collision))
            {
                photonView.ObservedComponents.Add(collision);
                Debug.Log("NetworkPlayerCollision agregado como componente observado");
            }
        }
        
        // Configurar Ownership Transfer
        photonView.OwnershipTransfer = OwnershipOption.Takeover;
        
        // Configurar View ID (se asignará automáticamente)
        photonView.ViewID = 0;
        
        Debug.Log("Player prefab configurado correctamente para Photon!");
    }
    
    [ContextMenu("Verify Prefab Setup")]
    public void VerifyPrefabSetup()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab no asignado!");
            return;
        }
        
        PhotonView photonView = playerPrefab.GetComponent<PhotonView>();
        if (photonView == null)
        {
            Debug.LogError("❌ PhotonView no encontrado en player prefab!");
            return;
        }
        
        PlayerController playerController = playerPrefab.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("❌ PlayerController no encontrado en player prefab!");
            return;
        }
        
        if (photonView.ObservedComponents == null || photonView.ObservedComponents.Count == 0)
        {
            Debug.LogWarning("⚠️ No hay componentes observados en PhotonView!");
        }
        else
        {
            bool hasPlayerController = false;
            foreach (Component comp in photonView.ObservedComponents)
            {
                if (comp is PlayerController)
                {
                    hasPlayerController = true;
                    break;
                }
            }
            
            if (!hasPlayerController)
            {
                Debug.LogWarning("⚠️ PlayerController no está en la lista de componentes observados!");
            }
        }
        
        Debug.Log("✅ Verificación completada. Prefab listo para multiplayer.");
    }
    
    void Start()
    {
        // Verificar configuración al iniciar
        VerifyPrefabSetup();
    }
} 