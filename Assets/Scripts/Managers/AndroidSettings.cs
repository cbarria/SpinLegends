using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AndroidSettings : MonoBehaviour
{
    [Header("Android UI Settings")]
    public float androidTextScale = 1.2f;
    public float androidButtonScale = 1.1f;
    public bool enableHighDPI = true;
    
    [Header("Console Settings")]
    public bool enableLargeConsoleText = true;
    
    void Awake()
    {
        #if UNITY_ANDROID
        ConfigureForAndroid();
        #endif
    }
    
    void ConfigureForAndroid()
    {
        Debug.Log("🔧 Configurando para Android...");
        
        // Configurar DPI para mejor calidad
        if (enableHighDPI)
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        }
        
        // Configurar orientación
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        
        // Configurar UI scaling
        ConfigureUIScaling();
        
        // Configurar console para Android
        if (enableLargeConsoleText)
        {
            ConfigureConsoleForAndroid();
        }
        
        Debug.Log("✅ Configuración de Android completada");
    }
    
    void ConfigureUIScaling()
    {
        // Buscar todos los textos en la escena y escalarlos
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text != null)
            {
                // Escalar el tamaño de fuente
                text.fontSize = Mathf.RoundToInt(text.fontSize * androidTextScale);
                
                // Hacer el texto más legible
                text.fontStyle = FontStyles.Bold;
                text.enableAutoSizing = false;
                
                // Configurar para mejor legibilidad
                text.textWrappingMode = TextWrappingModes.Normal;
                text.richText = true;
            }
        }
        
        // Buscar todos los botones y escalarlos
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in allButtons)
        {
            if (button != null)
            {
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Escalar el botón
                    rectTransform.localScale = Vector3.one * androidButtonScale;
                }
                
                // Escalar el texto del botón
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.fontSize = Mathf.RoundToInt(buttonText.fontSize * androidTextScale);
                    buttonText.fontStyle = FontStyles.Bold;
                }
            }
        }
        
        Debug.Log($"✅ Escalado UI completado: {allTexts.Length} textos, {allButtons.Length} botones");
    }
    
    void ConfigureConsoleForAndroid()
    {
        // Configurar el tamaño de fuente de la consola para Android
        #if UNITY_EDITOR
        // En el editor, podemos configurar las preferencias
        UnityEditor.EditorPrefs.SetInt("ConsoleFontSize", 16); // Tamaño más grande para Android
        #endif
        
        Debug.Log("✅ Configuración de consola para Android aplicada");
    }
    
    [ContextMenu("Apply Android Settings")]
    public void ApplyAndroidSettings()
    {
        ConfigureForAndroid();
    }
    
    [ContextMenu("Scale All UI Elements")]
    public void ScaleAllUIElements()
    {
        ConfigureUIScaling();
    }
} 