//using UnityEngine;

//public class RouteClickDetector : MonoBehaviour
//{
//    private bool isClaimed = false;

//    [Header("Wzór wagonika gracza (Do testu):")]
//    public Sprite claimedCarSprite;

//    void OnMouseDown()
//    {
//        if (isClaimed) return;

//        ClaimRoute(claimedCarSprite);
//    }

//    public void ClaimRoute(Sprite newSprite)
//    {
//        isClaimed = true;

//        SpriteRenderer[] trainCarsRenderers = GetComponentsInChildren<SpriteRenderer>();

//        foreach (SpriteRenderer car in trainCarsRenderers)
//        {
//            if (car != null)
//            {
//                if (newSprite != null)
//                {
//                    car.sprite = newSprite;
//                    car.color = Color.white;
//                }
//                else
//                {
//                    // Awaryjny test, jeœli nie przypiszesz grafiki w Inspectorze
//                    car.color = Color.red;
//                }
//            }
//        }
//    }
//}


// TEST

using UnityEngine;

public class RouteClickDetector : MonoBehaviour
{
    private bool isClaimed = false;

    // --- JEDNA DIAMENTOWA GRAFIKA DO TESTU ---
    // Wrzuæ tu w Inspectorze dowolny NOWY wagonik gracza (np. ten czerwony), 
    // ¿eby zobaczyæ jak stary szary/lawendowy klocek natychmiast siê podmienia!
    public Sprite testClaimedWagonSprite;

    // Ta funkcja odpali siê automatycznie w Unity, gdy klikniesz myszk¹ w trasê
    void OnMouseDown()
    {
        if (isClaimed) return;

        if (testClaimedWagonSprite == null)
        {
            // Jeœli zapomnisz wrzuciæ grafikê do testu, Unity da Wam znaæ w konsoli i zafarbuje trasê na zielono!
            Debug.LogWarning($"[TEST] Klikniêto trasê: {gameObject.name}, ale pole testClaimedWagonSprite jest puste! Farbujê awaryjnie na zielono.");
            ClaimRouteWithColor(Color.green);
            return;
        }

        // WYWO£ANIE AKCJI: Podmieniamy grafiki na ca³ej trasie!
        ClaimRouteWithSprite(testClaimedWagonSprite);
    }

    private void ClaimRouteWithSprite(Sprite newSprite)
    {
        isClaimed = true;

        // Szukamy wszystkich klocków na tej konkretnej linii
        SpriteRenderer[] trainCarsRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer car in trainCarsRenderers)
        {
            if (car != null)
            {
                car.sprite = newSprite;  // Wskakuje Wasz nowy wagonik gracza!
                car.color = Color.white; // Resetujemy fabryczny szary/lawendowy odcieñ do czystej bieli
            }
        }

        Debug.Log($"[SUKCES] Klikalnoœæ dzia³a idealnie! Trasa '{gameObject.name}' zosta³a pomyœlnie zajêta nowym wagonikiem.");
    }

    private void ClaimRouteWithColor(Color colorToApply)
    {
        isClaimed = true;
        SpriteRenderer[] trainCarsRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer car in trainCarsRenderers)
        {
            if (car != null) car.color = colorToApply;
        }
    }
}
