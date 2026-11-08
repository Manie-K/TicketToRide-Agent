using UnityEngine;
using UnityEngine.EventSystems;

public class StationClickDetector : MonoBehaviour
{
    private bool isStationPlaced = false;
    private SpriteRenderer spriteRenderer;

    [Header("Tablica grafik dworców dla graczy (0 = Gracz 1, 1 = Gracz 2 itd.)")]
    public Sprite[] playerStationSprites;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        // Blokada, jeśli klikamy przez otwarty interfejs UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (isStationPlaced) return;

        if (PlayerHandUIController.instance != null)
        {
            // W PRODUKCJI: Zespół logiki pobierze koszt z GameManager'a, np:
            // int cost = GameManager.instance.GetNextStationCost();
            // Na potrzeby Twoich ostatnich testów UI wpisujemy stałą wartość lub zostawiamy komunikat:

            string message = "Building a Station: Lay down required cards of the same color and click Confirm";

            // Otwieramy popup: przekazujemy komunikat, a kontener tunelu wyłączamy (false)
            PlayerHandUIController.instance.OpenRoutePopup(message, false);
        }
        else
        {
            Debug.LogError("PlayerHandUIController instance not found on the scene!");
        }
    }

    /// <summary>
    /// Metoda wywoływana przez GameManager po udanym opłaceniu stacji w interfejsie.
    /// </summary>
    public void BuildStationVisuals()
    {
        if (isStationPlaced) return;

        int activePlayer = TurnUIController.currentActivePlayer > 0 ? TurnUIController.currentActivePlayer : 1;
        int arrayIndex = activePlayer - 1;

        if (playerStationSprites == null || playerStationSprites.Length <= arrayIndex || playerStationSprites[arrayIndex] == null)
        {
            Debug.LogError($"Missing station sprite for player {activePlayer}!");
            return;
        }

        isStationPlaced = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = playerStationSprites[arrayIndex];
            spriteRenderer.color = Color.white;
        }

        Debug.Log($"[STATION] Placed in city: {gameObject.name}!");
    }
}

//// TEST

//using UnityEngine;
//using UnityEngine.EventSystems;

//public class StationClickDetector : MonoBehaviour
//{
//    private bool isStationPlaced = false;
//    private SpriteRenderer spriteRenderer;

//    [Header("Tablica grafik dworców dla graczy (0 = Gracz 1, 1 = Gracz 2 itd.)")]
//    public Sprite[] playerStationSprites;

//    private static int totalStationsBuiltTest = 0;

//    void Start()
//    {
//        spriteRenderer = GetComponent<SpriteRenderer>();
//    }

//    void OnMouseDown()
//    {
//        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
//        {
//            return;
//        }

//        if (isStationPlaced) return;

//        int calculatedCost = Mathf.Clamp(totalStationsBuiltTest + 1, 1, 3);

//        if (PlayerHandUIController.instance != null)
//        {
//            PlayerHandUIController.instance.OpenRoutePopup(false, true, calculatedCost);
//            PlayerHandUIController.instance.SetCurrentActiveStationTest(this);
//            Debug.Log($"[STACJA] Kliknięto miasto {gameObject.name}. Wyliczony koszt testowy: {calculatedCost} kart(y).");
//        }
//        else
//        {
//            Debug.LogError("Nie znaleziono instancji PlayerHandUIController na scenie!");
//        }
//    }

//    public void BuildStationVisuals()
//    {
//        if (isStationPlaced) return;

//        int currentActivePlayerNumber = 1;
//        if (TurnUIController.currentActivePlayer > 0)
//        {
//            currentActivePlayerNumber = TurnUIController.currentActivePlayer;
//        }

//        int arrayIndex = currentActivePlayerNumber - 1;

//        if (playerStationSprites == null || playerStationSprites.Length <= arrayIndex || playerStationSprites[arrayIndex] == null)
//        {
//            Debug.LogError($"Brak przypisanej grafiki dworca dla gracza numer {currentActivePlayerNumber}!");
//            return;
//        }

//        isStationPlaced = true;
//        totalStationsBuiltTest++;

//        if (spriteRenderer != null)
//        {
//            spriteRenderer.sprite = playerStationSprites[arrayIndex];
//            spriteRenderer.color = Color.white;
//        }

//        Debug.Log($"[SUKCES] Dworzec wybudowany wizualnie w mieście: {gameObject.name}!");
//    }
//}



// Poprzednia wersja

//using UnityEngine;

//public class StationClickDetector : MonoBehaviour
//{
//    private bool isStationPlaced = false;
//    private SpriteRenderer spriteRenderer;

//    [Header("Tablica grafik dworców dla graczy (0 = Gracz 1, 1 = Gracz 2 itd.)")]
//    public Sprite[] playerStationSprites;

//    void Start()
//    {
//        // Automatycznie pobieramy SpriteRenderer z tego obiektu miasteczka
//        spriteRenderer = GetComponent<SpriteRenderer>();
//    }

//    void OnMouseDown()
//    {
//        if (isStationPlaced) return;

//        int currentActivePlayerNumber = TurnUIController.currentActivePlayer;

//        PlaceStationForPlayer(currentActivePlayerNumber);
//    }

//    public void PlaceStationForPlayer(int playerNumber)
//    {
//        if (isStationPlaced) return;

//        int arrayIndex = playerNumber - 1; // Indeksy w tablicach odliczamy od 0

//        if (playerStationSprites == null || playerStationSprites.Length <= arrayIndex || playerStationSprites[arrayIndex] == null)
//        {
//            Debug.LogError($"Brak przypisanej grafiki dworca dla gracza numer {playerNumber}!");
//            return;
//        }

//        isStationPlaced = true;

//        if (spriteRenderer != null)
//        {
//            // Podmieniamy grafikę na kolor właściwy dla tego gracza!
//            spriteRenderer.sprite = playerStationSprites[arrayIndex];
//            spriteRenderer.color = Color.white; // Resetujemy kolor do czystej bieli
//        }

//        Debug.Log($"[SUKCES] Gracz {playerNumber} postawił swój dworzec w mieście: {gameObject.name}!");
//    }
//}