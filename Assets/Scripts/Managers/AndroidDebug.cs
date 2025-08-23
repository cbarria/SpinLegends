using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class AndroidDebug : MonoBehaviour
{
    [Header("Debug UI")]
    [SerializeField] private TextMeshProUGUI debugText;
    
    [Header("Debug Settings")]
    public bool showDebugInfo = false; // Desactivado por defecto para no interferir con el scoreboard
    public bool showFPS = false; // Desactivado por defecto
    public bool showInputInfo = false; // Desactivado por defecto
    public bool showNetworkInfo = false; // Desactivado por defecto
    public bool showPhotonViews = false; // Desactivado por defecto
    public bool hideLeftHud = true; // oculta HUD duplicado si existe
    
    private int frameCount = 0;
    private float timeElapsed = 0f;
    private float fps = 0f;

    private GameObject overlayPanel;
    
    void Start()
    {
        if (hideLeftHud)
        {
            // Intento best-effort: buscar un canvas/objeto típico de HUD izquierdo por nombre
            var leftHud = GameObject.Find("HUD_Left");
            if (leftHud != null) leftHud.SetActive(false);
        }
        // Solo crear debug en Android y solo si está explícitamente activado
        if (Application.platform != RuntimePlatform.Android || !showDebugInfo)
        {
            if (overlayPanel != null)
                overlayPanel.SetActive(false);
            return;
        }
        
        // Verificación adicional: solo crear si realmente se necesita
        if (debugText == null && showDebugInfo)
        {
            CreateDebugUI();
        }
    }
    
    void CreateDebugUI()
    {
        if (overlayPanel != null) return;
        GameObject panel = new GameObject("DebugPanel");
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGO = new GameObject("WorldCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        panel.transform.SetParent(canvas.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.4f);
        panelImage.raycastTarget = false;
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.7f, 0.7f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        overlayPanel = panel;

        GameObject btnObj = new GameObject("ToggleButton");
        btnObj.transform.SetParent(panel.transform, false);
        var btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(1f, 1f, 1f, 0.2f);
        btnImage.raycastTarget = true;
        var btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(ToggleDebug);
        var btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0f, 1f);
        btnRect.anchorMax = new Vector2(0f, 1f);
        btnRect.pivot = new Vector2(0f, 1f);
        btnRect.sizeDelta = new Vector2(80, 32);
        btnRect.anchoredPosition = new Vector2(8, -8);
        var btnTextGO = new GameObject("BtnText");
        btnTextGO.transform.SetParent(btnObj.transform, false);
        var btnText = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnText.text = "DEBUG";
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.fontSize = 18;
        btnText.raycastTarget = false;
        var btnTextRect = btnText.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(panel.transform, false);
        var scrollRect = scrollObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        var cg = scrollObj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
        var maskImage = scrollObj.AddComponent<Image>();
        maskImage.color = new Color(0, 0, 0, 0f);
        maskImage.raycastTarget = false;
        var mask = scrollObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        var scrollRectRT = scrollObj.GetComponent<RectTransform>();
        scrollRectRT.anchorMin = new Vector2(0f, 0f);
        scrollRectRT.anchorMax = new Vector2(1f, 1f);
        scrollRectRT.offsetMin = new Vector2(8, 8);
        scrollRectRT.offsetMax = new Vector2(-8, -48);

        GameObject content = new GameObject("Content");
        content.transform.SetParent(scrollObj.transform, false);
        var contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.sizeDelta = new Vector2(0, 1200);

        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(content.transform, false);
        debugText = textObj.AddComponent<TextMeshProUGUI>();
        debugText.fontSize = 22;
        debugText.textWrappingMode = TextWrappingModes.NoWrap;
        debugText.color = Color.white;
        debugText.text = "Debug Info";
        debugText.raycastTarget = false;
        var textRect = debugText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.sizeDelta = new Vector2(0, 1200);

        scrollRect.content = contentRT;
    }
    
    void Update()
    {
        if (!showDebugInfo || debugText == null) return;
        if (showFPS)
        {
            timeElapsed += Time.deltaTime;
            frameCount++;
            if (timeElapsed >= 0.5f)
            {
                fps = frameCount / timeElapsed;
                frameCount = 0;
                timeElapsed = 0f;
            }
        }
        UpdateDebugText();
    }
    
    void UpdateDebugText()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder(2048);
        if (showFPS)
        {
            sb.AppendLine($"FPS: {fps:F1}");
        }
        if (showInputInfo)
        {
            Joystick joystick = FindFirstObjectByType<Joystick>();
            if (joystick != null)
                sb.AppendLine($"Joystick: ({joystick.Horizontal:F2}, {joystick.Vertical:F2})");
            else
                sb.AppendLine("Joystick: No encontrado");
        }
        if (showNetworkInfo)
        {
            if (PhotonNetwork.IsConnected)
            {
                sb.AppendLine("Conectado: Sí");
                if (PhotonNetwork.InRoom)
                {
                    sb.AppendLine($"Room: {PhotonNetwork.CurrentRoom.Name}");
                    sb.AppendLine($"Players: {PhotonNetwork.CurrentRoom.PlayerCount}");
                    // Scoreboard
                    var scoreMgr = FindFirstObjectByType<ScoreManager>();
                    if (scoreMgr != null)
                    {
                        sb.AppendLine("Scores:");
                        foreach (var p in PhotonNetwork.PlayerList)
                        {
                            sb.AppendLine($" - {p.NickName} ({p.ActorNumber}): {scoreMgr.GetScore(p.ActorNumber)}");
                        }
                    }
                }
                else sb.AppendLine("Room: No");
            }
            else sb.AppendLine("Conectado: No");
        }
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        sb.AppendLine($"Players: {players.Length}");
        foreach (var p in players)
        {
            sb.AppendLine($" - {p.name} IsMine={p.photonView.IsMine} Pos={p.transform.position}");
        }
        if (showPhotonViews)
        {
            var views = FindObjectsByType<PhotonView>(FindObjectsSortMode.None);
            sb.AppendLine($"PhotonViews: {views.Length}");
            foreach (var v in views)
            {
                sb.AppendLine($" * {v.gameObject.name} VID={v.ViewID} IsMine={v.IsMine} Owner={v.OwnerActorNr}");
            }
        }
        debugText.text = sb.ToString();
    }
    
    public void EnableDebug()
    {
        showDebugInfo = true;
        if (Application.platform == RuntimePlatform.Android)
        {
            if (overlayPanel == null) CreateDebugUI();
            if (overlayPanel != null) overlayPanel.SetActive(true);
        }
    }
    public void DisableDebug()
    {
        showDebugInfo = false;
        if (overlayPanel != null) overlayPanel.SetActive(false);
    }
    public void ToggleDebug()
    {
        if (showDebugInfo) DisableDebug(); else EnableDebug();
    }
} 