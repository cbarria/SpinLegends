using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class SimpleScoreboard : MonoBehaviourPunCallbacks
{
    private ScoreManager scoreManager;
    private GameObject scoreboardPanel;
    
    void Start()
    {
        scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager == null)
        {
            Debug.LogError("❌ ScoreManager no encontrado!");
            return;
        }
        
        CreateSimpleScoreboard();
    }
    
    void CreateSimpleScoreboard()
    {
        // Buscar Canvas principal
        Canvas mainCanvas = FindFirstObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("❌ No se encontró Canvas principal!");
            return;
        }
        
        // Crear panel principal - SIMPLE Y DIRECTO
        GameObject panel = new GameObject("ScoreboardPanel");
        panel.transform.SetParent(mainCanvas.transform, false);
        
        // Agregar componentes básicos
        var rectTransform = panel.AddComponent<RectTransform>();
        var image = panel.AddComponent<Image>();
        
        // Configuración SIMPLE del panel con borde
        image.color = new Color(0.1f, 0.1f, 0.3f, 0.9f);
        image.sprite = CreateBorderSprite();
        rectTransform.anchorMin = new Vector2(0.75f, 0.7f);
        rectTransform.anchorMax = new Vector2(0.98f, 0.95f);
        rectTransform.sizeDelta = Vector2.zero;
        
        scoreboardPanel = panel;
        
        // Crear título
        CreateTitle(panel);
        
        // Crear lista de jugadores
        CreatePlayerList(panel);
        
        Debug.Log("🏆 Scoreboard SIMPLE creado");
    }
    
    Sprite CreateBorderSprite()
    {
        // Crear una textura 9-slice para el borde
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        
        Color[] pixels = new Color[size * size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // Borde blanco en los bordes
                if (x < 2 || x > size - 3 || y < 2 || y > size - 3)
                {
                    pixels[y * size + x] = Color.white;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Crear sprite con 9-slice para escalar correctamente
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(8, 8, 8, 8));
        
        return sprite;
    }
    
    void CreateTitle(GameObject parent)
    {
        GameObject title = new GameObject("Title");
        title.transform.SetParent(parent.transform, false);
        
        var titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "PLAYERS";
        titleText.fontSize = 16;
        titleText.color = Color.yellow;
        titleText.alignment = TextAlignmentOptions.Center;
        
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.85f);
        titleRect.anchorMax = new Vector2(1, 1f);
        titleRect.sizeDelta = Vector2.zero;
    }
    
    void CreatePlayerList(GameObject parent)
    {
        // Contenedor para la lista
        GameObject listContainer = new GameObject("PlayerList");
        listContainer.transform.SetParent(parent.transform, false);
        
        var listRect = listContainer.AddComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0, 0.1f);
        listRect.anchorMax = new Vector2(1, 0.8f);
        listRect.sizeDelta = Vector2.zero;
        
        // Crear entradas de jugadores
        CreatePlayerEntries(listContainer);
    }
    
    void CreatePlayerEntries(GameObject container)
    {
        // Crear 4 entradas fijas para mostrar
        for (int i = 0; i < 4; i++)
        {
            CreatePlayerEntry(container, i);
        }
    }
    
    void CreatePlayerEntry(GameObject parent, int index)
    {
        GameObject entry = new GameObject($"PlayerEntry_{index}");
        entry.transform.SetParent(parent.transform, false);
        
        var entryRect = entry.AddComponent<RectTransform>();
        entryRect.anchorMin = new Vector2(0, 0.75f - (index * 0.15f));
        entryRect.anchorMax = new Vector2(1, 0.9f - (index * 0.15f));
        entryRect.sizeDelta = Vector2.zero;
        
        // Nombre del jugador
        GameObject nameGO = new GameObject("Name");
        nameGO.transform.SetParent(entry.transform, false);
        
        var nameText = nameGO.AddComponent<TextMeshProUGUI>();
        nameText.text = $"Player {index + 1}";
        nameText.fontSize = 12;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Left;
        
        var nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0);
        nameRect.anchorMax = new Vector2(0.6f, 1f);
        nameRect.sizeDelta = Vector2.zero;
        nameRect.offsetMin = new Vector2(5, 0);
        nameRect.offsetMax = new Vector2(-5, 0);
        
        // Kills
        GameObject killsGO = new GameObject("Kills");
        killsGO.transform.SetParent(entry.transform, false);
        
        var killsText = killsGO.AddComponent<TextMeshProUGUI>();
        killsText.text = "0";
        killsText.fontSize = 12;
        killsText.color = Color.yellow;
        killsText.alignment = TextAlignmentOptions.Center;
        
        var killsRect = killsGO.GetComponent<RectTransform>();
        killsRect.anchorMin = new Vector2(0.6f, 0);
        killsRect.anchorMax = new Vector2(0.8f, 1f);
        killsRect.sizeDelta = Vector2.zero;
        
        // Deaths
        GameObject deathsGO = new GameObject("Deaths");
        deathsGO.transform.SetParent(entry.transform, false);
        
        var deathsText = deathsGO.AddComponent<TextMeshProUGUI>();
        deathsText.text = "0";
        deathsText.fontSize = 12;
        deathsText.color = Color.gray;
        deathsText.alignment = TextAlignmentOptions.Center;
        
        var deathsRect = deathsGO.GetComponent<RectTransform>();
        deathsRect.anchorMin = new Vector2(0.8f, 0);
        deathsRect.anchorMax = new Vector2(1, 1f);
        deathsRect.sizeDelta = Vector2.zero;
    }
    
    void Update()
    {
        UpdatePlayerData();
    }
    
    void UpdatePlayerData()
    {
        if (scoreManager == null || scoreboardPanel == null) return;
        
        var players = PhotonNetwork.PlayerList;
        if (players.Length == 0) return;
        
        // Actualizar solo las entradas que existen
        for (int i = 0; i < Mathf.Min(players.Length, 4); i++)
        {
            var player = players[i];
            var entry = scoreboardPanel.transform.Find("PlayerList").Find($"PlayerEntry_{i}");
            
            if (entry != null)
            {
                var nameText = entry.Find("Name").GetComponent<TextMeshProUGUI>();
                var killsText = entry.Find("Kills").GetComponent<TextMeshProUGUI>();
                var deathsText = entry.Find("Deaths").GetComponent<TextMeshProUGUI>();
                
                // Usar nombre del jugador o "Player X"
                string playerName = string.IsNullOrEmpty(player.NickName) ? $"Player {player.ActorNumber}" : player.NickName;
                nameText.text = playerName;
                killsText.text = scoreManager.GetKills(player.ActorNumber).ToString();
                deathsText.text = scoreManager.GetDeaths(player.ActorNumber).ToString();
                
                // Color especial para el jugador local
                if (player == PhotonNetwork.LocalPlayer)
                {
                    nameText.color = Color.yellow;
                    killsText.color = Color.yellow;
                    deathsText.color = Color.yellow;
                }
            }
        }
    }
}
