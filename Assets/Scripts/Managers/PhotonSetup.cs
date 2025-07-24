using UnityEngine;
using Photon.Pun;

public class PhotonSetup : MonoBehaviour
{
    [Header("Network Performance")]
    public int sendRateOnSerialize = 30; // Enviar datos 30 veces por segundo para más suavidad
    public int serializationRate = 30; // Serializar 30 veces por segundo para más suavidad
    
    void Awake()
    {
        // Configurar Photon para mejor rendimiento
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.SendRate = sendRateOnSerialize;
        PhotonNetwork.SerializationRate = serializationRate;
        
        // Configurar versión del juego
        PhotonNetwork.GameVersion = "1.0";
        
        // Configurar nick del jugador
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);
        }
        
        // Configurar opciones de red para mejor rendimiento
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 10000; // 10 segundos
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.TransportProtocol = ExitGames.Client.Photon.ConnectionProtocol.Udp;
        
        Debug.Log("Photon configurado para mejor rendimiento de red");
    }
    
    void Start()
    {
        // Configurar calidad de red
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        
        // Configurar física para mejor rendimiento
        Physics.defaultSolverIterations = 6;
        Physics.defaultSolverVelocityIterations = 2;
        
        Debug.Log("Configuración de rendimiento aplicada");
    }
    
    // Método para cambiar el nombre del jugador
    public void SetPlayerName(string newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            PhotonNetwork.NickName = newName;
            Debug.Log("Nombre del jugador cambiado a: " + newName);
        }
    }
    
    // Método para obtener el nombre del jugador
    public string GetPlayerName()
    {
        return PhotonNetwork.NickName;
    }
    
    // Método para verificar si Photon está configurado
    public bool IsPhotonConfigured()
    {
        return !string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime);
    }
} 