using UnityEngine;

public class AndroidSettings : MonoBehaviour
{
    [Header("Android Settings")]
    public bool keepScreenOn = true;
    public bool allowSleep = false;
    public int targetFrameRate = 60;
    public bool useAccelerometer = true;
    
    void Start()
    {
        SetupAndroidSettings();
    }
    
    void SetupAndroidSettings()
    {
        // Set target frame rate
        Application.targetFrameRate = targetFrameRate;
        
        // Keep screen on during gameplay
        if (keepScreenOn)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
        
        // Enable accelerometer for mobile controls
        if (useAccelerometer)
        {
            Input.gyro.enabled = true;
        }
        
        // Set quality settings for mobile
        QualitySettings.vSyncCount = 0;
        QualitySettings.antiAliasing = 0;
        
        // Optimize for mobile
        Application.runInBackground = true;
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Game paused - save state
            SaveGameState();
        }
        else
        {
            // Game resumed - restore state
            RestoreGameState();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // Game lost focus - pause if needed
            // GameManager.Instance.ShowPauseMenu(); // Eliminado porque ya no existe
            Time.timeScale = 0f; // Opcional: Pausa el juego
        }
    }
    
    void SaveGameState()
    {
        // Save current game state
        PlayerPrefs.SetFloat("PlayerHealth", 100f);
        PlayerPrefs.SetInt("PlayerScore", 0);
        PlayerPrefs.Save();
    }
    
    void RestoreGameState()
    {
        // Restore game state if needed
        float health = PlayerPrefs.GetFloat("PlayerHealth", 100f);
        int score = PlayerPrefs.GetInt("PlayerScore", 0);
        
        // Apply restored values
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateHealthUI(health, 100f);
        }
    }
    
    void OnDestroy()
    {
        // Clean up when game is destroyed
        SaveGameState();
    }
} 