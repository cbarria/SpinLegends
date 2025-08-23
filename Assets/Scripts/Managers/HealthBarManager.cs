using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;

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
        canvasRect.sizeDelta = new Vector2(2f, 0.3f);
        
        // Create Background (black)
        GameObject bgGO = new GameObject("BG");
        bgGO.transform.SetParent(canvasGO.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = Color.black;
        
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
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
        
        Debug.Log("Created health bar for player " + id);
    }
    
    void UpdateHealthBar(PlayerController player, int id)
    {
        if (!healthBars.ContainsKey(id)) return;
        
        GameObject canvasGO = healthBars[id];
        if (canvasGO == null) return;
        
        // Update position
        canvasGO.transform.position = player.transform.position + Vector3.up * 4f;
        
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