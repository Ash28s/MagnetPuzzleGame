using UnityEngine;

public class SpriteCreator : MonoBehaviour
{
    public static Sprite CreateSquareSprite(Color color, int size = 32)
    {
        Texture2D texture = new Texture2D(size, size);
        
        // Fill with color
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Create sprite
        Sprite sprite = Sprite.Create(texture, 
                                    new Rect(0, 0, size, size), 
                                    new Vector2(0.5f, 0.5f), 
                                    size);
        return sprite;
    }
    
    public static Sprite CreateBorderSprite(Color borderColor, Color fillColor, int size = 32, int borderWidth = 2)
    {
        Texture2D texture = new Texture2D(size, size);
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Check if pixel is on border
                if (x < borderWidth || x >= size - borderWidth || 
                    y < borderWidth || y >= size - borderWidth)
                {
                    texture.SetPixel(x, y, borderColor);
                }
                else
                {
                    texture.SetPixel(x, y, fillColor);
                }
            }
        }
        
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, 
                                    new Rect(0, 0, size, size), 
                                    new Vector2(0.5f, 0.5f), 
                                    size);
        return sprite;
    }
}