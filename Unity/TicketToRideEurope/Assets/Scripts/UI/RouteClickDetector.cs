using UnityEngine;

public class RouteClickDetector : MonoBehaviour
{
    private bool isClaimed = false;

    [Header("Grafika wagonika dla tego gracza / kolory")]
    public SpriteRenderer wagonRenderer; // Jeœli masz przypisany konkretny sprite renderer trasy

    [Header("Tablica grafik wagoników dla graczy (0 = Gracz 1, 1 = Gracz 2 itd.)")]
    public Sprite[] playerWagonSprites;

    void OnMouseDown()
    {
        if (isClaimed) return;

        // Pobieramy numer aktywnego gracza z prostego licznika tur (tak jak dzia³a³o wczeœniej)
        int activePlayer = 1;
        if (TurnUIController.currentActivePlayer > 0)
        {
            activePlayer = TurnUIController.currentActivePlayer;
        }

        // Oznaczamy trasê jako zajêt¹, ¿eby nie mo¿na by³o klikn¹æ jej ponownie
        isClaimed = true;

        // Podmieniamy grafikê wagoników na trasie na podstawie koloru/grafiki gracza
        SpriteRenderer[] trainCarsRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer car in trainCarsRenderers)
        {
            if (car != null && playerWagonSprites != null && playerWagonSprites.Length > 0)
            {
                int index = Mathf.Clamp(activePlayer - 1, 0, playerWagonSprites.Length - 1);
                if (playerWagonSprites[index] != null)
                {
                    car.sprite = playerWagonSprites[index];
                    car.color = Color.white;
                }
            }
        }

        Debug.Log($"[TRASA] Zajêta przez gracza {activePlayer}!");
    }
}