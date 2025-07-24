using UnityEngine;
using UnityEngine.UI;

public class IconGenerator : MonoBehaviour
{
    public static Sprite CreateCircleIcon(Color color, float size = 32f)
    {
        Texture2D texture = new Texture2D((int)size, (int)size);
        Color[] pixels = new Color[(int)(size * size)];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f; // Dejar un borde
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    pixels[(int)(y * size + x)] = color;
                }
                else
                {
                    pixels[(int)(y * size + x)] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    public static Sprite CreateWifiIcon(Color color, float size = 32f)
    {
        Texture2D texture = new Texture2D((int)size, (int)size);
        Color[] pixels = new Color[(int)(size * size)];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float angle = Mathf.Atan2(y - center.y, x - center.x) * Mathf.Rad2Deg;
                
                // Crear forma de wifi (arcos concéntricos)
                bool isWifi = false;
                if (angle >= -45f && angle <= 45f) // Solo la parte superior
                {
                    if (distance >= size * 0.1f && distance <= size * 0.3f) // Arco exterior
                        isWifi = true;
                    else if (distance >= size * 0.2f && distance <= size * 0.4f) // Arco medio
                        isWifi = true;
                    else if (distance >= size * 0.3f && distance <= size * 0.5f) // Arco interior
                        isWifi = true;
                }
                
                pixels[(int)(y * size + x)] = isWifi ? color : Color.clear;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    public static Sprite CreatePlayerIcon(Color color, float size = 32f)
    {
        Texture2D texture = new Texture2D((int)size, (int)size);
        Color[] pixels = new Color[(int)(size * size)];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                // Crear forma de persona (círculo para cabeza, rectángulo para cuerpo)
                bool isPerson = false;
                
                // Cabeza (círculo pequeño en la parte superior)
                Vector2 headCenter = center + Vector2.up * (size * 0.2f);
                float headDistance = Vector2.Distance(new Vector2(x, y), headCenter);
                if (headDistance <= size * 0.15f)
                    isPerson = true;
                
                // Cuerpo (rectángulo en la parte inferior)
                if (y >= center.y - size * 0.1f && y <= center.y + size * 0.3f)
                {
                    if (Mathf.Abs(x - center.x) <= size * 0.12f)
                        isPerson = true;
                }
                
                pixels[(int)(y * size + x)] = isPerson ? color : Color.clear;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    public static Sprite CreateRoomIcon(Color color, float size = 32f)
    {
        Texture2D texture = new Texture2D((int)size, (int)size);
        Color[] pixels = new Color[(int)(size * size)];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // Crear forma de casa/sala (triángulo para techo, rectángulo para base)
                bool isRoom = false;
                
                // Base rectangular
                if (y >= center.y - size * 0.3f && y <= center.y + size * 0.2f)
                {
                    if (Mathf.Abs(x - center.x) <= size * 0.25f)
                        isRoom = true;
                }
                
                // Techo triangular
                float roofHeight = size * 0.3f;
                float roofWidth = size * 0.25f;
                if (y >= center.y + size * 0.2f && y <= center.y + roofHeight)
                {
                    float roofSlope = (y - (center.y + size * 0.2f)) / (roofHeight - size * 0.2f);
                    float currentWidth = roofWidth * (1f - roofSlope);
                    if (Mathf.Abs(x - center.x) <= currentWidth)
                        isRoom = true;
                }
                
                pixels[(int)(y * size + x)] = isRoom ? color : Color.clear;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
} 