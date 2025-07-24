using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class AndroidDebug : MonoBehaviour
{
    [Header("Debug UI")]
    public TextMeshProUGUI debugText;
    public GameObject debugPanel;
    
    [Header("Debug Settings")]
    public bool showDebugInfo = false; // Cambiado a false por defecto
    public bool showFPS = true;
    public bool showInputInfo = true;
    public bool showNetworkInfo = true;
    
    private float deltaTime = 0.0f;
    private int frameCount = 0;
    private float timeElapsed = 0f;
    private float fps = 0f;
    
    void Start()
    {
        // Solo mostrar debug en Android si está habilitado
        if (Application.platform != RuntimePlatform.Android || !showDebugInfo)
        {
            if (debugPanel != null)
                debugPanel.SetActive(false);
            return;
        }
        
        // Crear UI de debug si no existe
        if (debugText == null)
        {
            CreateDebugUI();
        }
    }
    
    void CreateDebugUI()
    {
        // Crear panel de debug
        if (debugPanel == null)
        {
            GameObject panel = new GameObject("DebugPanel");
            panel.transform.SetParent(FindFirstObjectByType<Canvas>()?.transform);
            
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);
            
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            // Cambiar posición a esquina superior derecha para no tapar el joystick
            panelRect.anchorMin = new Vector2(0.6f, 0.7f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            debugPanel = panel;
        }
        
        // Crear texto de debug
        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(debugPanel.transform);
        
        debugText = textObj.AddComponent<TextMeshProUGUI>();
        debugText.fontSize = 10; // Reducir tamaño de fuente
        debugText.color = Color.white;
        debugText.text = "Debug Info";
        
        RectTransform textRect = debugText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 5);
        textRect.offsetMax = new Vector2(-5, -5);
    }
    
    void Update()
    {
        if (!showDebugInfo || debugText == null) return;
        
        // Calcular FPS
        if (showFPS)
        {
            deltaTime += Time.deltaTime;
            timeElapsed += Time.deltaTime;
            frameCount++;
            
            if (timeElapsed >= 0.5f)
            {
                fps = frameCount / timeElapsed;
                frameCount = 0;
                timeElapsed = 0f;
            }
        }
        
        // Actualizar texto de debug
        UpdateDebugText();
    }
    
    void UpdateDebugText()
    {
        string debugInfo = "";
        
        if (showFPS)
        {
            debugInfo += $"FPS: {fps:F1}\n";
        }
        
        if (showInputInfo)
        {
            // Información del joystick
            Joystick joystick = FindFirstObjectByType<FixedJoystick>();
            if (joystick == null)
                joystick = FindFirstObjectByType<FloatingJoystick>();
            if (joystick == null)
                joystick = FindFirstObjectByType<VariableJoystick>();
                
            if (joystick != null)
            {
                debugInfo += $"Joystick: ({joystick.Horizontal:F2}, {joystick.Vertical:F2})\n";
            }
            else
            {
                debugInfo += "Joystick: No encontrado\n";
            }
            
            // Información del mouse
            debugInfo += $"Mouse: ({Input.mousePosition.x:F0}, {Input.mousePosition.y:F0})\n";
        }
        
        if (showNetworkInfo)
        {
            // Información de red
            if (PhotonNetwork.IsConnected)
            {
                debugInfo += $"Conectado: Sí\n";
                if (PhotonNetwork.InRoom)
                {
                    debugInfo += $"Sala: {PhotonNetwork.CurrentRoom.Name}\n";
                    debugInfo += $"Jugadores: {PhotonNetwork.CurrentRoom.PlayerCount}\n";
                }
                else
                {
                    debugInfo += "Sala: No\n";
                }
            }
            else
            {
                debugInfo += "Conectado: No\n";
            }
        }
        
        // Información de jugadores
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        debugInfo += $"Jugadores: {players.Length}\n";
        
        foreach (PlayerController player in players)
        {
            if (player.photonView.IsMine)
            {
                debugInfo += $"Mi posición: {player.transform.position}\n";
                debugInfo += $"Mi salud: {player.CurrentHealth:F0}\n";
                break;
            }
        }
        
        debugText.text = debugInfo;
    }
    
    public void LogError(string error)
    {
        if (debugText != null)
        {
            debugText.text += $"\nERROR: {error}";
        }
        Debug.LogError(error);
    }
    
    public void LogWarning(string warning)
    {
        if (debugText != null)
        {
            debugText.text += $"\nWARNING: {warning}";
        }
        Debug.LogWarning(warning);
    }
    
    // Método para activar el debug
    public void EnableDebug()
    {
        showDebugInfo = true;
        if (Application.platform == RuntimePlatform.Android)
        {
            if (debugPanel == null)
            {
                CreateDebugUI();
            }
            if (debugPanel != null)
            {
                debugPanel.SetActive(true);
            }
        }
    }
    
    // Método para desactivar el debug
    public void DisableDebug()
    {
        showDebugInfo = false;
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
    }
    
    // Método para alternar el debug
    public void ToggleDebug()
    {
        if (showDebugInfo)
        {
            DisableDebug();
        }
        else
        {
            EnableDebug();
        }
    }
} 