using UnityEngine;
using UnityEngine.EventSystems;

public class RouteClickDetector : MonoBehaviour
{
    private bool isClaimed = false;

    [Header("USTAWIENIA TRASY")]
    [Tooltip("Zaznacz, jeœli ta konkretna trasa jest tunelem")]
    public bool isTunnel = false;

    [Header("Grafika wagonika dla tego gracza")]
    public SpriteRenderer wagonRenderer;

    [Header("Tablica grafik wagoników dla graczy (0 = Gracz 1, 1 = Gracz 2 itd.)")]
    public Sprite[] playerWagonSprites;

    void OnMouseDown()
    {
        // Blokada, jeœli klikamy przez otwarty interfejs UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (isClaimed) return;

        if (PlayerHandUIController.instance != null)
        {
            // PRODUCENCKIE WYWO£ANIE: Przekazujemy tekst powitania oraz to, czy pokazaæ okienko tunelu
            string message = isTunnel
                ? "Building a Tunnel: Select the basic cards from your hand and click Confirm"
                : "Select cards from your hand and click Confirm";

            PlayerHandUIController.instance.OpenRoutePopup(message, isTunnel);
        }
        else
        {
            Debug.LogError("PlayerHandUIController instance not found on the scene!");
        }
    }

    /// <summary>
    /// Metoda wywo³ywana przez GameManager po udanym op³aceniu i zatwierdzeniu kart w UI.
    /// </summary>
    public void BuildRouteVisuals()
    {
        if (isClaimed) return;
        isClaimed = true;

        int activePlayer = TurnUIController.currentActivePlayer > 0 ? TurnUIController.currentActivePlayer : 1;
        int index = Mathf.Clamp(activePlayer - 1, 0, playerWagonSprites.Length - 1);

        SpriteRenderer[] trainCarsRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer car in trainCarsRenderers)
        {
            if (car != null && playerWagonSprites != null && playerWagonSprites.Length > index && playerWagonSprites[index] != null)
            {
                car.sprite = playerWagonSprites[index];
                car.color = Color.white;
            }
        }
        Debug.Log($"[ROUTE] Claimed by player {activePlayer}!");
    }
}

//// TEST

//using UnityEngine;

//public class RouteClickDetector : MonoBehaviour
//{
//    private bool isClaimed = false;

//    [Header("USTAWIENIA TESTOWE TUNELU")]
//    [Tooltip("Zaznacz to ptaszkiem w Inspektorze, jeœli TA konkretna trasa jest tunelem!")]
//    public bool isTunnel = false;

//    [Header("Grafika wagonika dla tego gracza / kolory")]
//    public SpriteRenderer wagonRenderer;

//    [Header("Tablica grafik wagoników dla graczy (0 = Gracz 1, 1 = Gracz 2 itd.)")]
//    public Sprite[] playerWagonSprites;

//    void OnMouseDown()
//    {
//        // Jeœli myszka klika na jakikolwiek element UI (przycisk, okienko itp.), trasa natychmiast ignoruje to klikniêcie i przerywa wykonywanie funkcji
//        if (UnityEngine.EventSystems.EventSystem.current != null &&
//            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
//        {
//            return;
//        }

//        if (isClaimed) return;

//        // KROK 1: Wywo³ujemy otwarcie okienka popup i przekazujemy informacjê, czy to tunel
//        if (PlayerHandUIController.instance != null)
//        {
//            PlayerHandUIController.instance.OpenRoutePopup(isTunnel);

//            // KROK 2: Dajemy znaæ panelowi, któr¹ konkretnie trasê na mapie w³aœnie budujemy, ¿eby móg³ postawiæ na niej wagoniki, gdy gracz kliknie "PotwierdŸ"
//            PlayerHandUIController.instance.SetCurrentActiveRouteTest(this);
//        }
//        else
//        {
//            Debug.LogError("Nie znaleziono instancji PlayerHandUIController na scenie!");
//        }
//    }

//    // Ta funkcja zostanie wywo³ana automatycznie ze skryptu popupu, dopiero gdy gracz zatwierdzi i wyda odpowiednie karty
//    public void BuildRouteVisuals()
//    {
//        if (isClaimed) return;

//        isClaimed = true;

//        int activePlayer = 1;
//        if (TurnUIController.currentActivePlayer > 0)
//        {
//            activePlayer = TurnUIController.currentActivePlayer;
//        }

//        // Podmieniamy grafikê wagoników na trasie
//        SpriteRenderer[] trainCarsRenderers = GetComponentsInChildren<SpriteRenderer>();
//        foreach (SpriteRenderer car in trainCarsRenderers)
//        {
//            if (car != null && playerWagonSprites != null && playerWagonSprites.Length > 0)
//            {
//                int index = Mathf.Clamp(activePlayer - 1, 0, playerWagonSprites.Length - 1);
//                if (playerWagonSprites[index] != null)
//                {
//                    car.sprite = playerWagonSprites[index];
//                    car.color = Color.white;
//                }
//            }
//        }

//        Debug.Log($"[TRASA] Oficjalnie zajêta i op³acona przez gracza {activePlayer}!");
//    }
//}