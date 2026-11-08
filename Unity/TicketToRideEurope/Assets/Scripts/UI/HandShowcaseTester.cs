using UnityEngine;
using System.Collections.Generic;

public class HandShowcaseTester : MonoBehaviour
{
    [Header("Wrzuc tutaj wszystkie swoje kolory kart z folderu:")]
    public List<Sprite> allPossibleCards = new List<Sprite>();

    // Funkcja uruchomi siê automatycznie zaraz po klikniêciu PLAY
    void Start()
    {
        if (PlayerHandUIController.instance == null)
        {
            Debug.LogError("Nie znaleziono PlayerHandUIController na scenie!");
            return;
        }

        // Uruchamiamy pokaz z opóŸnieniem czasowym, ¿eby wszystko ³adnie zobaczyæ
        StartCoroutine(RunShowcaseRoutine());
    }

    System.Collections.IEnumerator RunShowcaseRoutine()
    {
        // Czekamy pó³ sekundy na start, a¿ Unity za³aduje scenê
        yield return new WaitForSeconds(0.5f);

        Debug.Log("--- FAZA 1: Wyk³adamy wszystkie mo¿liwe karty po kolei ---");
        foreach (Sprite card in allPossibleCards)
        {
            if (card != null)
            {
                PlayerHandUIController.instance.AddCardToHand(card);
                // Czekamy 0.3 sekundy miêdzy ka¿d¹ kart¹, ¿eby zobaczyæ jak wachlarz roœnie!
                yield return new WaitForSeconds(0.3f);
            }
        }

        // Czekamy 1.5 sekundy przerwy przed pokazem liczników
        yield return new WaitForSeconds(1.5f);

        Debug.Log("--- FAZA 2: Zwiêkszamy liczniki dla 3 pierwszych kart ---");
        for (int i = 0; i < 3; i++)
        {
            // Sprawdzamy na wszelki wypadek czy mamy tyle kart na liœcie
            if (allPossibleCards.Count > i && allPossibleCards[i] != null)
            {
                // Dorzucamy drug¹ tak¹ sam¹ kartê -> nasz s³ownik w PlayerHandUIController to wykryje!
                PlayerHandUIController.instance.AddCardToHand(allPossibleCards[i]);

                // Krótka przerwa miêdzy klikniêciami
                yield return new WaitForSeconds(0.5f);
            }
        }

        Debug.Log("Pokaz zakoñczony sukcesem!");
    }
}
