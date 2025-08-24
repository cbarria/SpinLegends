using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class HealthBarManager : MonoBehaviour
{
    private Dictionary<int, GameObject> healthBars = new Dictionary<int, GameObject>();
    private Dictionary<PlayerController, float> targetHealth = new Dictionary<PlayerController, float>();
    private Dictionary<PlayerController, Image> fillImages = new Dictionary<PlayerController, Image>();
    private Dictionary<PlayerController, float> targetAnchorX = new Dictionary<PlayerController, float>();

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
        
        // Improved name fetching with sync check
        int actorNumber = player.photonView.Owner.ActorNumber;
        Player photonPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        string playerName = (photonPlayer != null && !string.IsNullOrEmpty(photonPlayer.NickName)) 
            ? photonPlayer.NickName 
            : $"Player {actorNumber}";
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
        fillImages[player] = fillImg;
        targetHealth[player] = player.CurrentHealth / player.MaxHealth;
        targetAnchorX[player] = 1f; // Initial full health
        
        Debug.Log("Created health bar for player " + id + " with name: " + nameText.text + " and font: " + (font != null ? font.name : "NONE"));
        
        // Add a small delay to ensure NickName is synced
        StartCoroutine(DelayedNameSetup(player, id));
    }

    System.Collections.IEnumerator DelayedNameSetup(PlayerController player, int id)
    {
        yield return new WaitForSeconds(0.5f); // Small delay for sync
        
        GameObject canvasGO = healthBars[id];
        TextMeshProUGUI nameText = canvasGO.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
        {
            int actorNumber = player.photonView.Owner.ActorNumber;
            Player photonPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            string playerName = (photonPlayer != null && !string.IsNullOrEmpty(photonPlayer.NickName)) 
                ? photonPlayer.NickName 
                : $"Player {actorNumber}";
            nameText.text = playerName;
        }
    }

    // New RPC to force name update on all clients
    public void UpdatePlayerName(int actorNumber, string newName)
    {
        PlayerController player = FindPlayerByActor(actorNumber);
        if (player != null && healthBars.ContainsKey(player.photonView.ViewID))
        {
            GameObject canvasGO = healthBars[player.photonView.ViewID];
            TextMeshProUGUI nameText = canvasGO.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = newName;
            }
        }
    }

    // Helper to find player by actor number
    PlayerController FindPlayerByActor(int actorNumber)
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            if (p.photonView.Owner.ActorNumber == actorNumber)
                return p;
        }
        return null;
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
        
        // Update health with lerp
        Transform fillTransform = canvasGO.transform.Find("BG/Fill");
        if (fillTransform != null)
        {
            RectTransform fillRect = fillTransform.GetComponent<RectTransform>();
            Image fillImg = fillTransform.GetComponent<Image>();
            
            float healthPercent = player.CurrentHealth / player.MaxHealth;
            
            // Lerp the anchorMax.x
            float currentX = fillRect.anchorMax.x;
            float targetX = healthPercent;
            fillRect.anchorMax = new Vector2(Mathf.Lerp(currentX, targetX, Time.deltaTime * 5f), 1f);
            
            // Color update
            if (healthPercent > 0.6f) fillImg.color = Color.green;
            else if (healthPercent > 0.3f) fillImg.color = Color.yellow;
            else fillImg.color = Color.red;
            
            // Damage popup if decreased
            if (healthPercent < targetAnchorX[player])
            {
                float damage = (targetAnchorX[player] - healthPercent) * player.MaxHealth;
                CreateDamageText(canvasGO, damage);
            }
            targetAnchorX[player] = healthPercent;
        }
        
        // Debug ray
        Debug.DrawRay(canvasGO.transform.position, Vector3.up * 2f, Color.red, 1f);
    }

    private void CreateDamageText(GameObject canvasGO, float damage)
    {
        GameObject textGO = new GameObject("DamageText");
        textGO.transform.SetParent(canvasGO.transform, false);
        var text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = $"-{Mathf.RoundToInt(damage)}";
        text.fontSize = 0.5f;
        text.color = Color.red;
        text.alignment = TextAlignmentOptions.Center;

        var rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, 0.5f);

        // Simple animation: move up and fade
        StartCoroutine(AnimateDamageText(textGO));
    }

    private System.Collections.IEnumerator AnimateDamageText(GameObject textGO)
    {
        TextMeshProUGUI text = textGO.GetComponent<TextMeshProUGUI>();
        RectTransform rect = textGO.GetComponent<RectTransform>();
        float timer = 0f;
        while (timer < 1f)
        {
            if (textGO == null || text == null || rect == null) yield break;
            timer += Time.deltaTime;
            rect.anchoredPosition += new Vector2(0, Time.deltaTime * 1f);
            text.color = new Color(1, 0, 0, 1 - timer);
            yield return null;
        }
        if (textGO != null) Destroy(textGO);
    }

    public void RemoveHealthBarForPlayer(int actorNumber)
    {
        foreach (var kvp in healthBars)
        {
            if (kvp.Key == actorNumber) // Assuming id is actorNumber, adjust if needed
            {
                Destroy(kvp.Value);
                healthBars.Remove(kvp.Key);
                break;
            }
        }
    }
    
    void CleanupHealthBars()
    {
        List<int> toRemove = new List<int>();
        
        foreach (var kvp in healthBars)
        {
            int id = kvp.Key;
            bool playerExists = false;
            
            if (FindObjectsByType<PlayerController>(FindObjectsSortMode.None) == null) // Adjust accordingly
            {
                toRemove.Add(id);
                if (kvp.Value != null) Destroy(kvp.Value);
                continue;
            }

            foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
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