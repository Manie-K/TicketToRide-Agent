using UnityEngine;
using System.Collections.Generic; 
using TMPro;

public class PlayerHandUIController : MonoBehaviour
{
    public static PlayerHandUIController instance;

    [Header("Szablon karty w d³oni")]
    public GameObject cardTemplatePrefab;

    private Dictionary<Sprite, TextMeshProUGUI> cardsInHand = new Dictionary<Sprite, TextMeshProUGUI>();
    private Dictionary<Sprite, int> cardCounts = new Dictionary<Sprite, int>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void AddCardToHand(Sprite cardImage)
    {
        if (cardTemplatePrefab == null) return;

        // Gracz ma ju¿ ten kolor karty w rêce -> zwiêkszamy licznik
        if (cardsInHand.ContainsKey(cardImage))
        {
            cardCounts[cardImage]++; 
            cardsInHand[cardImage].text = "x" + cardCounts[cardImage];
        }
        // Pierwsza taka karta w d³oni -> tworzymy nowy kafelek
        else
        {
            GameObject newCard = Instantiate(cardTemplatePrefab, transform);
            newCard.SetActive(true);

            UnityEngine.UI.Image uiImage = newCard.GetComponent<UnityEngine.UI.Image>();
            if (uiImage != null) uiImage.sprite = cardImage;

            TextMeshProUGUI counterText = newCard.GetComponentInChildren<TextMeshProUGUI>();
            if (counterText != null)
            {
                counterText.text = "x1"; 

                cardsInHand.Add(cardImage, counterText);
                cardCounts.Add(cardImage, 1);
            }
        }
    }
}
