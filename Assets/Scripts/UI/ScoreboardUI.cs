using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ScoreboardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject scoreboardPanel;
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerEntryPrefab;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button toggleButton;
    
    [Header("On-Screen Score Display")]
    [SerializeField] private GameObject onScreenScorePanel;
    [SerializeField] private TextMeshProUGUI onScreenScoreText;
    [SerializeField] private TextMeshProUGUI onScreenKillsText;
    
    [Header("Visual Settings")]
    [SerializeField] private Color firstPlaceColor = new Color(1f, 0.84f, 0f, 1f); // Gold
    [SerializeField] private Color secondPlaceColor = new Color(0.75f, 0.75f, 0.75f, 1f); // Silver
    [SerializeField] private Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.2f, 1f); // Bronze
    [SerializeField] private Color normalColor = Color.white;
    
    private bool isVisible = false;
    private List<GameObject> playerEntries = new List<GameObject>();
    private ScoreManager scoreManager;
    
    void Awake()
    {
        // Solo configurar referencias existentes, NO crear nada
        if (scoreboardPanel == null) scoreboardPanel = transform.Find("ScoreboardPanel")?.gameObject;
        if (playerListContainer == null) playerListContainer = transform.Find("ScoreboardPanel/PlayerListContainer");
        if (titleText == null) titleText = transform.Find("ScoreboardPanel/TitleText")?.GetComponent<TextMeshProUGUI>();
        if (toggleButton == null) toggleButton = transform.Find("ToggleButton")?.GetComponent<Button>();
    }
    
    void Start()
    {
        // Usar Invoke para asegurar que todos los componentes estén listos
        Invoke(nameof(InitializeUI), 0.1f);
    }
    
    void InitializeUI()
    {
        // Crear UI si no existe
        if (scoreboardPanel == null) CreateScoreboardUI();
        
        // Configurar botón toggle
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleScoreboard);
            Debug.Log("🏆 Botón toggle configurado correctamente");
        }
        else
        {
            Debug.LogWarning("⚠️ Botón toggle NO encontrado");
        }
        
        // Ocultar por defecto - FORZAR que esté oculto
        if (scoreboardPanel != null)
        {
            scoreboardPanel.SetActive(false);
            isVisible = false;
            Debug.Log("🏆 Scoreboard oculto por defecto");
        }
        
        scoreManager = ScoreManager.Instance;
        if (scoreManager == null)
        {
            Debug.LogWarning("⚠️ ScoreboardUI: ScoreManager no encontrado");
        }
        
        // Crear prefab de entrada de jugador si no existe
        if (playerEntryPrefab == null)
        {
            CreatePlayerEntryPrefab();
        }
        
        // Crear display de puntuación en pantalla si no existe
        if (onScreenScorePanel == null)
        {
            CreateOnScreenScoreDisplay();
        }
    }
    
    void CreateScoreboardUI()
    {
        // Panel principal
        scoreboardPanel = new GameObject("ScoreboardPanel");
        scoreboardPanel.transform.SetParent(transform, false);
        
        // Canvas y RectTransform
        Canvas canvas = scoreboardPanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        
        CanvasScaler scaler = scoreboardPanel.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        GraphicRaycaster raycaster = scoreboardPanel.AddComponent<GraphicRaycaster>();
        
        RectTransform panelRect = scoreboardPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0.5f);
        panelRect.anchorMax = new Vector2(1, 0.5f);
        panelRect.sizeDelta = new Vector2(350, 500);
        panelRect.anchoredPosition = new Vector2(-175, 0);
        
        // Fondo del panel
        GameObject background = new GameObject("Background");
        background.transform.SetParent(scoreboardPanel.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Título
        GameObject titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(scoreboardPanel.transform, false);
        titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "SCOREBOARD"; // Removido emoji para evitar problemas de fuente
        titleText.fontSize = 24;
        titleText.color = Color.yellow;
        titleText.alignment = TextAlignmentOptions.Center;
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(0, 40);
        titleRect.offsetMin = new Vector2(10, -40);
        titleRect.offsetMax = new Vector2(-10, 0);
        
        // Contenedor de lista de jugadores
        GameObject containerGO = new GameObject("PlayerListContainer");
        containerGO.transform.SetParent(scoreboardPanel.transform, false);
        RectTransform containerRect = containerGO.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.offsetMin = new Vector2(10, 10);
        containerRect.offsetMax = new Vector2(-10, -50);
        
        // ScrollView para la lista
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(containerGO.transform, false);
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        RectTransform scrollRectTransform = scrollView.GetComponent<RectTransform>();
        if (scrollRectTransform == null) scrollRectTransform = scrollView.AddComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        if (viewportRect == null) viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewportRect.sizeDelta = Vector2.zero;
        
        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        if (contentRect == null) contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        contentRect.anchoredPosition = Vector2.zero;
        
        // Configurar ScrollRect
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 10f;
        
        // Asignar referencias
        playerListContainer = content.transform;
        
        // Botón toggle
        GameObject toggleGO = new GameObject("ToggleButton");
        toggleGO.transform.SetParent(transform, false);
        toggleButton = toggleGO.AddComponent<Button>();
        Image toggleImage = toggleGO.AddComponent<Image>();
        toggleImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        GameObject toggleTextGO = new GameObject("Text");
        toggleTextGO.transform.SetParent(toggleGO.transform, false);
        TextMeshProUGUI toggleText = toggleTextGO.AddComponent<TextMeshProUGUI>();
        toggleText.text = "SCORE"; // Removido emoji
        toggleText.fontSize = 12;
        toggleText.color = Color.white;
        toggleText.alignment = TextAlignmentOptions.Center;
        
        RectTransform toggleRect = toggleGO.GetComponent<RectTransform>();
        if (toggleRect == null) toggleRect = toggleGO.AddComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(1, 1);
        toggleRect.anchorMax = new Vector2(1, 1);
        toggleRect.sizeDelta = new Vector2(60, 30);
        toggleRect.anchoredPosition = new Vector2(-30, -15);
        
        RectTransform toggleTextRect = toggleTextGO.GetComponent<RectTransform>();
        if (toggleTextRect == null) toggleTextRect = toggleTextGO.AddComponent<RectTransform>();
        toggleTextRect.anchorMin = Vector2.zero;
        toggleTextRect.anchorMax = Vector2.one;
        toggleTextRect.offsetMin = Vector2.zero;
        toggleTextRect.offsetMax = Vector2.zero;
        
        Debug.Log("🏆 ScoreboardUI creado automáticamente");
        
        // FORZAR que esté oculto inmediatamente después de crearlo
        scoreboardPanel.SetActive(false);
        isVisible = false;
        Debug.Log("🏆 Scoreboard forzado a estar oculto después de crear");
    }
    
    void CreateOnScreenScoreDisplay()
    {
        // Panel de puntuación en pantalla
        onScreenScorePanel = new GameObject("OnScreenScorePanel");
        onScreenScorePanel.transform.SetParent(transform, false);
        
        // Canvas para el panel en pantalla
        Canvas onScreenCanvas = onScreenScorePanel.AddComponent<Canvas>();
        onScreenCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        onScreenCanvas.sortingOrder = 999;
        
        // Asegurar que esté por encima de todo
        onScreenCanvas.overrideSorting = true;
        onScreenCanvas.sortingOrder = 1001;
        
        CanvasScaler onScreenScaler = onScreenScorePanel.AddComponent<CanvasScaler>();
        onScreenScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        onScreenScaler.referenceResolution = new Vector2(1920, 1080);
        
        RectTransform onScreenRect = onScreenScorePanel.GetComponent<RectTransform>();
        if (onScreenRect == null) onScreenRect = onScreenScorePanel.AddComponent<RectTransform>();
        onScreenRect.anchorMin = new Vector2(1, 1);
        onScreenRect.anchorMax = new Vector2(1, 1);
        onScreenRect.sizeDelta = new Vector2(120, 50);
        onScreenRect.anchoredPosition = new Vector2(-60, -25);
        
        // FORZAR que el display en pantalla esté en la esquina superior derecha
        onScreenRect.anchoredPosition = new Vector2(-60, -25);
        Debug.Log("🏆 Display en pantalla posicionado en esquina superior derecha");
        
        // Fondo del panel
        GameObject onScreenBg = new GameObject("Background");
        onScreenBg.transform.SetParent(onScreenScorePanel.transform, false);
        Image onScreenBgImage = onScreenBg.AddComponent<Image>();
        onScreenBgImage.color = new Color(0, 0, 0, 0.7f);
        RectTransform onScreenBgRect = onScreenBg.GetComponent<RectTransform>();
        if (onScreenBgRect == null) onScreenBgRect = onScreenBg.AddComponent<RectTransform>();
        onScreenBgRect.anchorMin = Vector2.zero;
        onScreenBgRect.anchorMax = Vector2.one;
        onScreenBgRect.offsetMin = Vector2.zero;
        onScreenBgRect.offsetMax = Vector2.zero;
        
        // Texto de puntuación
        GameObject scoreTextGO = new GameObject("ScoreText");
        scoreTextGO.transform.SetParent(onScreenScorePanel.transform, false);
        onScreenScoreText = scoreTextGO.AddComponent<TextMeshProUGUI>();
        onScreenScoreText.text = "Score: 0";
        onScreenScoreText.fontSize = 14;
        onScreenScoreText.color = Color.yellow;
        onScreenScoreText.alignment = TextAlignmentOptions.Center;
        RectTransform scoreTextRect = scoreTextGO.GetComponent<RectTransform>();
        if (scoreTextRect == null) scoreTextRect = scoreTextGO.AddComponent<RectTransform>();
        scoreTextRect.anchorMin = new Vector2(0, 0.5f);
        scoreTextRect.anchorMax = new Vector2(1, 1);
        scoreTextRect.offsetMin = new Vector2(5, 2);
        scoreTextRect.offsetMax = new Vector2(-5, -2);
        
        // Texto de kills
        GameObject killsTextGO = new GameObject("KillsText");
        killsTextGO.transform.SetParent(onScreenScorePanel.transform, false);
        onScreenKillsText = killsTextGO.AddComponent<TextMeshProUGUI>();
        onScreenKillsText.text = "Kills: 0";
        onScreenKillsText.fontSize = 12;
        onScreenKillsText.color = Color.red;
        onScreenKillsText.alignment = TextAlignmentOptions.Center;
        RectTransform killsTextRect = killsTextGO.GetComponent<RectTransform>();
        if (killsTextRect == null) killsTextRect = killsTextGO.AddComponent<RectTransform>();
        killsTextRect.anchorMin = new Vector2(0, 0);
        killsTextRect.anchorMax = new Vector2(1, 0.5f);
        killsTextRect.offsetMin = new Vector2(5, 2);
        killsTextRect.offsetMax = new Vector2(-5, -2);
        
        Debug.Log("🏆 On-Screen Score Display creado automáticamente");
        Debug.Log($"🏆 Posición del display: {onScreenRect.anchoredPosition}, Tamaño: {onScreenRect.sizeDelta}");
    }
    
    void CreatePlayerEntryPrefab()
    {
        playerEntryPrefab = new GameObject("PlayerEntryPrefab");
        
        // Fondo de la entrada
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(playerEntryPrefab.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        if (bgRect == null) bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Nombre del jugador
        GameObject nameGO = new GameObject("PlayerName");
        nameGO.transform.SetParent(playerEntryPrefab.transform, false);
        TextMeshProUGUI nameText = nameGO.AddComponent<TextMeshProUGUI>();
        nameText.text = "Player";
        nameText.fontSize = 18;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Left;
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        if (nameRect == null) nameRect = nameGO.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0);
        nameRect.anchorMax = new Vector2(0.4f, 1);
        nameRect.offsetMin = new Vector2(10, 5);
        nameRect.offsetMax = new Vector2(-5, -5);
        
        // Kills
        GameObject killsGO = new GameObject("Kills");
        killsGO.transform.SetParent(playerEntryPrefab.transform, false);
        TextMeshProUGUI killsText = killsGO.AddComponent<TextMeshProUGUI>();
        killsText.text = "0";
        killsText.fontSize = 16;
        killsText.color = Color.red;
        killsText.alignment = TextAlignmentOptions.Center;
        RectTransform killsRect = killsGO.GetComponent<RectTransform>();
        if (killsRect == null) killsRect = killsGO.AddComponent<RectTransform>();
        killsRect.anchorMin = new Vector2(0.4f, 0);
        killsRect.anchorMax = new Vector2(0.6f, 1);
        killsRect.offsetMin = Vector2.zero;
        killsRect.offsetMax = Vector2.zero;
        
        // Deaths
        GameObject deathsGO = new GameObject("Deaths");
        deathsGO.transform.SetParent(playerEntryPrefab.transform, false);
        TextMeshProUGUI deathsText = deathsGO.AddComponent<TextMeshProUGUI>();
        deathsText.text = "0";
        deathsText.fontSize = 16;
        deathsText.color = Color.gray;
        deathsText.alignment = TextAlignmentOptions.Center;
        RectTransform deathsRect = deathsGO.GetComponent<RectTransform>();
        if (deathsRect == null) deathsRect = deathsGO.AddComponent<RectTransform>();
        deathsRect.anchorMin = new Vector2(0.6f, 0);
        deathsRect.anchorMax = new Vector2(0.8f, 1);
        deathsRect.offsetMin = Vector2.zero;
        deathsRect.offsetMax = Vector2.zero;
        
        // Score
        GameObject scoreGO = new GameObject("Score");
        scoreGO.transform.SetParent(playerEntryPrefab.transform, false);
        TextMeshProUGUI scoreText = scoreGO.AddComponent<TextMeshProUGUI>();
        scoreText.text = "0";
        scoreText.fontSize = 18;
        scoreText.color = Color.yellow;
        scoreText.alignment = TextAlignmentOptions.Center;
        RectTransform scoreRect = scoreGO.GetComponent<RectTransform>();
        if (scoreRect == null) scoreRect = scoreGO.AddComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.8f, 0);
        scoreRect.anchorMax = new Vector2(1, 1);
        scoreRect.offsetMin = new Vector2(5, 5);
        scoreRect.offsetMax = new Vector2(-10, -5);
        
        // Configurar tamaño del prefab
        RectTransform prefabRect = playerEntryPrefab.GetComponent<RectTransform>();
        if (prefabRect == null) prefabRect = playerEntryPrefab.AddComponent<RectTransform>();
        prefabRect.sizeDelta = new Vector2(0, 35);
        
        // Ocultar el prefab
        playerEntryPrefab.SetActive(false);
        
        Debug.Log("🏆 PlayerEntryPrefab creado automáticamente");
    }
    
    void UpdateOnScreenScore()
    {
        if (onScreenScoreText == null || onScreenKillsText == null || scoreManager == null) return;
        
        // Obtener jugador local
        var localPlayer = PhotonNetwork.LocalPlayer;
        if (localPlayer == null) return;
        
        // Actualizar puntuación
        int score = scoreManager.GetScore(localPlayer.ActorNumber);
        onScreenScoreText.text = $"Score: {score}";
        
        // Actualizar kills
        int kills = scoreManager.GetKills(localPlayer.ActorNumber);
        onScreenKillsText.text = $"Kills: {kills}";
    }
    
    void Update()
    {
        if (scoreManager != null)
        {
            // Actualizar display en pantalla siempre
            UpdateOnScreenScore();
            
            // Actualizar scoreboard solo si está visible
            if (isVisible)
            {
                UpdateScoreboard();
            }
        }
    }
    
    public void ToggleScoreboard()
    {
        if (scoreboardPanel != null)
        {
            isVisible = !isVisible;
            scoreboardPanel.SetActive(isVisible);
            Debug.Log($"🏆 Scoreboard toggle: {isVisible}");
            
            if (isVisible)
            {
                UpdateScoreboard();
            }
        }
    }
    
    void UpdateScoreboard()
    {
        if (scoreManager == null || playerListContainer == null) return;
        
        // Limpiar entradas existentes
        foreach (var entry in playerEntries)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        playerEntries.Clear();
        
        // Obtener jugadores ordenados por score
        var players = PhotonNetwork.PlayerList.ToList();
        var playerStats = new List<(Player player, int score, int kills, int deaths)>();
        
        foreach (var player in players)
        {
            int score = scoreManager.GetScore(player.ActorNumber);
            int kills = scoreManager.GetKills(player.ActorNumber);
            int deaths = scoreManager.GetDeaths(player.ActorNumber);
            playerStats.Add((player, score, kills, deaths));
        }
        
        // Ordenar por kills descendente (más kills = mejor posición)
        playerStats = playerStats.OrderByDescending(p => p.kills).ToList();
        
        // Crear entradas para cada jugador
        for (int i = 0; i < playerStats.Count; i++)
        {
            var (player, score, kills, deaths) = playerStats[i];
            CreatePlayerEntry(player, score, kills, deaths, i);
        }
        
        // Ajustar tamaño del content
        if (playerListContainer != null)
        {
            RectTransform contentRect = playerListContainer.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(0, playerStats.Count * 37);
        }
    }
    
    void CreatePlayerEntry(Player player, int score, int kills, int deaths, int position)
    {
        if (playerEntryPrefab == null) return;
        
        GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
        entry.SetActive(true);
        playerEntries.Add(entry);
        
        // Configurar posición
        RectTransform entryRect = entry.GetComponent<RectTransform>();
        entryRect.anchoredPosition = new Vector2(0, -position * 37);
        
        // Configurar colores según posición
        Color positionColor = GetPositionColor(position);
        entry.transform.Find("Background").GetComponent<Image>().color = positionColor;
        
        // Configurar textos
        entry.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>().text = player.NickName;
        entry.transform.Find("Kills").GetComponent<TextMeshProUGUI>().text = kills.ToString();
        entry.transform.Find("Deaths").GetComponent<TextMeshProUGUI>().text = deaths.ToString();
        entry.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = score.ToString();
        
        // Agregar indicador de posición
        if (position < 3)
        {
            string[] medals = { "🥇", "🥈", "🥉" };
            entry.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>().text = 
                medals[position] + " " + player.NickName;
        }
    }
    
    Color GetPositionColor(int position)
    {
        switch (position)
        {
            case 0: return firstPlaceColor;
            case 1: return secondPlaceColor;
            case 2: return thirdPlaceColor;
            default: return normalColor;
        }
    }
    
    // Método estático para crear ScoreboardUI fácilmente (comentado para evitar problemas)
    /*
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateScoreboardUIInstance()
    {
        if (FindFirstObjectByType<ScoreboardUI>() == null)
        {
            GameObject scoreboardGO = new GameObject("ScoreboardUI");
            scoreboardGO.AddComponent<ScoreboardUI>();
            DontDestroyOnLoad(scoreboardGO);
            Debug.Log("🏆 ScoreboardUI creado automáticamente en la escena");
        }
    }
    */
}
