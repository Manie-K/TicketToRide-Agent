using UnityEngine;
using UnityEngine.UI;

public class TicketSlotUI : MonoBehaviour
{
    [Header("Komponent Image karty")]
    public Image cardImage;

    [Header("Ramka zaznaczenia (np. zielona ramka lub tło wskazujące wybór)")]
    public GameObject selectionBorder; // Możesz wrzucić tutaj mały obiekt ramki jako dziecko slotu

    [HideInInspector] public Sprite cardSprite;
    [HideInInspector] public string city1;
    [HideInInspector] public string city2;
    [HideInInspector] public int points;

    private bool isSelected = false;

    // Metoda ustawiająca grafikę i dane w slocie
    public void Setup(Sprite sprite, string c1, string c2, int pts)
    {
        cardSprite = sprite;
        city1 = c1;
        city2 = c2;
        points = pts;

        if (cardImage != null)
        {
            cardImage.sprite = cardSprite;
        }

        SetSelected(false); // Domyślnie niezaznaczone
    }

    // Metoda wywoływana po kliknięciu w slot
    public void OnSlotClicked()
    {
        isSelected = !isSelected; // Przełączamy stan (zaznaczony / odznaczony)
        SetSelected(isSelected);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selectionBorder != null)
        {
            selectionBorder.SetActive(isSelected); // Pokazujemy lub ukrywamy ramkę zaznaczenia
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }
}