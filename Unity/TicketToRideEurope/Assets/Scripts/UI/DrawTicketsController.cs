using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DrawTicketsController : MonoBehaviour
{
    [Header("Główny Panel Popup")]
    public GameObject popupPanel;

    [Header("Wszystkie dostępne grafiki biletów (JPG/Sprite z folderu)")]
    public List<Sprite> allTicketSprites; // Tutaj możesz przeciągnąć swoje sprite'y z Canvy

    [Header("Trzy sloty kart na ekranie")]
    public TicketSlotUI[] ticketSlots; // Przeciągnij tutaj CardSlot_1, 2 i 3

    [Header("Przycisk Zatwierdź")]
    public Button confirmButton;

    // Metoda wywoływana po kliknięciu przycisku "Tickets" na planszy
    public void OpenDrawTicketsPopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(true);

        // Losujemy 3 unikalne obrazki z puli wszystkich dostępnych biletów
        if (allTicketSprites == null || allTicketSprites.Count < 3)
        {
            Debug.LogWarning("[BILETY] Za mało grafik biletów na liście w Inspektorze!");
            return;
        }

        List<Sprite> pool = new List<Sprite>(allTicketSprites);
        for (int i = 0; i < ticketSlots.Length; i++)
        {
            if (pool.Count == 0) break;

            int randomIndex = Random.Range(0, pool.Count);
            Sprite chosenSprite = pool[randomIndex];
            pool.RemoveAt(randomIndex);

            // Wypełniamy slot wylosowaną grafiką (możesz tu też przekazać nazwy miast, jeśli chcesz)
            ticketSlots[i].gameObject.SetActive(true);
            ticketSlots[i].Setup(chosenSprite, "MiastoA", "MiastoB", 10); // Przykładowe dane
            ticketSlots[i].SetSelected(false); // Resetujemy zaznaczenie
        }
    }

    [Header("Referencja do stosu biletów gracza (np. skrypt zarządzający stosem)")]
    public PlayerTicketViewer playerTicketViewer; // Lub referencja do skryptu stosu

    // Metoda wywoływana po kliknięciu przycisku "Confirm"
    public void OnConfirmClicked()
    {
        int selectedCount = 0;
        List<Sprite> chosenSprites = new List<Sprite>();

        foreach (var slot in ticketSlots)
        {
            if (slot.gameObject.activeSelf && slot.IsSelected())
            {
                selectedCount++;
                // Zbieramy wylosowany sprite z tego slotu
                if (slot.cardImage != null)
                {
                    chosenSprites.Add(slot.cardImage.sprite);
                }
            }
        }

        if (selectedCount < 1)
        {
            Debug.LogWarning("[BILETY] Musisz wybrać co najmniej jeden bilet przed zatwierdzeniem!");
            return;
        }

        // PRZEKAZUJEMY WYBRANE BILETY DO STOSU GRACZA:
        // (Zakładam, że masz skrypt PlayerTicketViewer lub PlayerTicketStack przypisany na panelu gracza)
        if (playerTicketViewer != null)
        {
            foreach (var sprite in chosenSprites)
            {
                playerTicketViewer.AddTicketToPlayerStack(sprite);
            }
        }

        Debug.Log($"[BILETY] Zatwierdzono i dodano {selectedCount} biletów do stosu gracza.");

        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
}