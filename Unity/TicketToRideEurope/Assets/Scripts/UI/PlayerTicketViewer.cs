using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerTicketViewer : MonoBehaviour
{
    public Image topTicketImage; // Przeciągnij tutaj komponent Image obiektu TopTicket
    private List<Sprite> myTickets = new List<Sprite>();
    private int currentIndex = 0;

    // Metoda wywoływana przez DrawTicketsController po zatwierdzeniu
    public void AddTicketToPlayerStack(Sprite newTicketSprite)
    {
        myTickets.Add(newTicketSprite);
        currentIndex = myTickets.Count - 1; // Pokaż ostatnio dodany
        UpdateDisplayedTicket();
    }

    // Metoda podpięta pod przycisk kliknięcia w stos biletów (przeklikiwanie)
    public void OnTicketStackClicked()
    {
        if (myTickets.Count == 0) return;

        currentIndex = (currentIndex + 1) % myTickets.Count; // Przechodzi cyklicznie do następnego biletu
        UpdateDisplayedTicket();
    }

    private void UpdateDisplayedTicket()
    {
        if (topTicketImage != null && myTickets.Count > 0)
        {
            topTicketImage.sprite = myTickets[currentIndex];
            topTicketImage.gameObject.SetActive(true);
        }
    }
}