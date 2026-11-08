using UnityEngine;

public class TicketCardItem : MonoBehaviour
{
    [Header("Miasta i punkty (wpisz je tak, jak na grafice z Canvy)")]
    public string city1;
    public string city2;
    public int points;

    [Header("Komponent graficzny (SpriteRenderer lub UI Image)")]
    public SpriteRenderer spriteRenderer;
    // public UnityEngine.UI.Image uiImage; // Odkomentuj, jeśli to obiekt UI na Canvasie

    // Metoda pomocnicza do ustawiania danych
    public void Setup(string c1, string c2, int pts, Sprite cardSprite)
    {
        city1 = c1;
        city2 = c2;
        points = pts;

        if (spriteRenderer != null && cardSprite != null)
        {
            spriteRenderer.sprite = cardSprite;
        }
    }
}