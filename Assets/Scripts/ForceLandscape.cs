using UnityEngine;

public class ForceLandscape : MonoBehaviour
{
    void Awake()
    {
        // Forzar orientación horizontal izquierda
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        
        // Deshabilitar la rotación automática
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        
        // Aplicar los cambios inmediatamente
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
    
    void Start()
    {
        // Asegurar que la orientación se mantenga
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
} 