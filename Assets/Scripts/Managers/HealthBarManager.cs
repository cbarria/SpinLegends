using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;

public class HealthBarManager : MonoBehaviour
{
    private Dictionary<int, GameObject> healthBars = new Dictionary<int, GameObject>();

    void Update()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        
        foreach (PlayerController player in players)
        {
            if (player == null || player.photonView == null) continue;
            
            int id = player.photonView.ViewID;
            
            // Create health bar if doesn't exist
            if (!healthBars.ContainsKey(id))
            {
                CreateHealthBar(player, id);
            }
            
            // Update existing health bar
            UpdateHealthBar(player, id);
        }
        
        // Remove health bars for dead players
        CleanupHealthBars();
    }
    
    void CreateHealthBar(PlayerController player, int id)
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("HealthBar_" + id);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        
        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(4f, 1.2f);
        
        // Create Name Label
        GameObject nameGO = new GameObject("PlayerName");
        nameGO.transform.SetParent(canvasGO.transform, false);
        TextMeshProUGUI nameText = nameGO.AddComponent<TextMeshProUGUI>();
        string playerName = string.IsNullOrEmpty(player.photonView.Owner.NickName) ? $"Player {player.photonView.Owner.ActorNumber}" : player.photonView.Owner.NickName;
        nameText.text = playerName;
        nameText.fontSize = 0.5f;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.textWrappingMode = TextWrappingModes.NoWrap;
        
        // Assign default font
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font != null)
        {
            nameText.font = font;
        }
        else
        {
            Debug.LogError("Failed to load TMP font for health bar name!");
        }
        
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(1, 1f);
        nameRect.sizeDelta = Vector2.zero;
        
        // Create Background (black)
        GameObject bgGO = new GameObject("BG");
        bgGO.transform.SetParent(canvasGO.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = Color.black;
        
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 0.5f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Create Fill (green)
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(bgGO.transform, false);
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = Color.green;
        
        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        healthBars[id] = canvasGO;
        
        Debug.Log("Created health bar for player " + id + " with name: " + nameText.text + " and font: " + (font != null ? font.name : "NONE"));
    }
    
    void UpdateHealthBar(PlayerController player, int id)
    {
        if (!healthBars.ContainsKey(id)) return;
        
        GameObject canvasGO = healthBars[id];
        if (canvasGO == null) return;
        
        // Update position
        canvasGO.transform.position = player.transform.position + Vector3.up * 5f;
        
        // Make it face the camera
        canvasGO.transform.LookAt(Camera.main.transform);
        canvasGO.transform.Rotate(0, 180, 0); // Flip to face correctly
        
        // Update health
        Transform fillTransform = canvasGO.transform.Find("BG/Fill");
        if (fillTransform != null)
        {
            RectTransform fillRect = fillTransform.GetComponent<RectTransform>();
            Image fillImg = fillTransform.GetComponent<Image>();
            
            float healthPercent = player.CurrentHealth / player.MaxHealth;
            
            fillRect.anchorMax = new Vector2(healthPercent, 1f);
            
            if (healthPercent > 0.6f) fillImg.color = Color.green;
            else if (healthPercent > 0.3f) fillImg.color = Color.yellow;
            else fillImg.color = Color.red;
        }
        
        // Debug ray to visualize position
        Debug.DrawRay(canvasGO.transform.position, Vector3.up * 2f, Color.red, 1f);
    }
    
    void CleanupHealthBars()
    {
        List<int> toRemove = new List<int>();
        
        foreach (var kvp in healthBars)
        {
            int id = kvp.Key;
            bool playerExists = false;
            
            PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player != null && player.photonView != null && player.photonView.ViewID == id)
                {
                    playerExists = true;
                    break;
                }
            }
            
            if (!playerExists)
            {
                toRemove.Add(id);
                if (kvp.Value != null) Destroy(kvp.Value);
            }
        }
        
        foreach (int id in toRemove)
        {
            healthBars.Remove(id);
        }
    }
}