using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PlayerHandUIController : MonoBehaviour
{
    public static PlayerHandUIController instance;

    [Header("Szablony UI")]
    [Tooltip("Szablon pojedynczego kafelka karty (posiada Image i TextMeshProUGUI)")]
    public GameObject cardTemplatePrefab;

    [Header("JOKER")]
    [Tooltip("Referencja do pliku graficznego Lokomotywy (Jokera)")]
    public Sprite locomotiveSprite;

    [Header("--- SEKCJA OKIENKA POPUP ---")]
    public GameObject routePopupPanel;
    public Button closePopupButton;
    public Button confirmPopupButton;

    [Header("--- SEKCJA POPUPU: KONTENERY ---")]
    [Tooltip("Kontener z Horizontal Layout Group na 3 karty wylosowane z talii przy tunelu")]
    public Transform tunnelCardsContainer;

    [Tooltip("Kontener z Horizontal Layout Group na karty, które gracz wy³o¿y³ do okienka")]
    public Transform spentCardsContainer;

    [Tooltip("Tekst instrukta¿owy/informacyjny na górze okienka popup")]
    public TextMeshProUGUI popupInstructionText;

    // S³owniki do zarz¹dzania UI d³oni gracza (na dole ekranu)
    private Dictionary<Sprite, GameObject> cardsInHand = new Dictionary<Sprite, GameObject>();
    private Dictionary<Sprite, int> cardCounts = new Dictionary<Sprite, int>();

    // S³owniki do zarz¹dzania UI kart wy³o¿onych w okienku popup
    private Dictionary<Sprite, GameObject> cardsInPopupContainer = new Dictionary<Sprite, GameObject>();
    private Dictionary<Sprite, int> cardCountsInPopupContainer = new Dictionary<Sprite, int>();

    // Listy pomocnicze
    private List<Sprite> spentCardsTrackList = new List<Sprite>();
    private List<GameObject> spawnedTunnelCards = new List<GameObject>();

    // Wewnêtrzny stan UI
    private bool isBuildPopupOpen = false;
    private Vector2 confirmButtonDefaultPosition;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (closePopupButton != null) closePopupButton.onClick.AddListener(CancelRouteBuild);
        if (confirmPopupButton != null) confirmPopupButton.onClick.AddListener(ProcessConfirmButton);
        if (routePopupPanel != null) routePopupPanel.SetActive(false);

        if (confirmPopupButton != null)
        {
            confirmButtonDefaultPosition = confirmPopupButton.GetComponent<RectTransform>().anchoredPosition;
        }
    }

    /// <summary>
    /// Otwiera uniwersalne okienko popup dla budowy trasy, tunelu lub stacji.
    /// </summary>
    public void OpenRoutePopup(string headerMessage, bool showTunnelContainer)
    {
        if (routePopupPanel == null) return;

        isBuildPopupOpen = true;
        ClearPopupVisuals();

        if (tunnelCardsContainer != null) tunnelCardsContainer.gameObject.SetActive(showTunnelContainer);
        if (closePopupButton != null) closePopupButton.gameObject.SetActive(true);

        if (confirmPopupButton != null)
        {
            confirmPopupButton.GetComponent<RectTransform>().anchoredPosition = confirmButtonDefaultPosition;
        }

        if (popupInstructionText != null) popupInstructionText.text = headerMessage;
        routePopupPanel.SetActive(true);
    }

    public void UpdateInstructionText(string newMessage)
    {
        if (popupInstructionText != null) popupInstructionText.text = newMessage;
    }

    public void CenterConfirmButton(bool shouldCenter)
    {
        if (confirmPopupButton == null) return;

        RectTransform rect = confirmPopupButton.GetComponent<RectTransform>();
        if (shouldCenter)
        {
            if (closePopupButton != null) closePopupButton.gameObject.SetActive(false);
            rect.anchoredPosition = new Vector2(0f, rect.anchoredPosition.y);
        }
        else
        {
            if (closePopupButton != null) closePopupButton.gameObject.SetActive(true);
            rect.anchoredPosition = confirmButtonDefaultPosition;
        }
    }

    public void DisplayDrawnTunnelCard(Sprite cardSprite)
    {
        if (tunnelCardsContainer == null || cardSprite == null) return;

        GameObject cardObj = Instantiate(cardTemplatePrefab, tunnelCardsContainer);
        cardObj.SetActive(true);

        Image cardImg = cardObj.GetComponent<Image>();
        if (cardImg != null) cardImg.sprite = cardSprite;

        Button cardBtn = cardObj.GetComponent<Button>();
        if (cardBtn != null) cardBtn.interactable = false;

        spawnedTunnelCards.Add(cardObj);
    }

    public void ClosePopup()
    {
        isBuildPopupOpen = false;
        if (routePopupPanel != null) routePopupPanel.SetActive(false);
        ClearPopupVisuals();
    }

    private void ProcessConfirmButton()
    {
        // GameManager.instance.OnConfirmPressed();
    }

    private void CancelRouteBuild()
    {
        // GameManager.instance.OnCancelPressed();
    }

    private void OnHandCardClicked(Sprite clickedCardSprite)
    {
        if (!isBuildPopupOpen) return;
        // GameManager.instance.TryToPlayCardFromHand(clickedCardSprite);
    }

    private void OnPopupCardClicked(Sprite clickedCardSprite)
    {
        // GameManager.instance.TryToWithdrawCardFromPopup(clickedCardSprite);
    }

    // --- OPERACJE NA KAFELKACH UI ---

    public void AddCardToHand(Sprite cardImage)
    {
        if (cardTemplatePrefab == null || cardImage == null) return;

        if (cardsInHand.ContainsKey(cardImage))
        {
            GameObject handCardObj = cardsInHand[cardImage];
            if (handCardObj != null)
            {
                cardCounts[cardImage]++;
                TextMeshProUGUI counterText = handCardObj.GetComponentInChildren<TextMeshProUGUI>();
                if (counterText != null) counterText.text = "x" + cardCounts[cardImage];
                if (!handCardObj.activeSelf) handCardObj.SetActive(true);
            }
        }
        else
        {
            CreateNewCardInHand(cardImage);
        }
    }

    private void CreateNewCardInHand(Sprite cardImage)
    {
        GameObject newCard = Instantiate(cardTemplatePrefab, transform);
        newCard.SetActive(true);

        Image uiImage = newCard.GetComponent<Image>();
        if (uiImage != null) uiImage.sprite = cardImage;

        TextMeshProUGUI counterText = newCard.GetComponentInChildren<TextMeshProUGUI>();
        if (counterText != null)
        {
            counterText.text = "x1";
            counterText.color = Color.black;
        }

        cardsInHand.Add(cardImage, newCard);
        cardCounts.Add(cardImage, 1);

        Button cardButton = newCard.GetComponent<Button>();
        if (cardButton == null) cardButton = newCard.AddComponent<Button>();

        cardButton.onClick.AddListener(() => OnHandCardClicked(cardImage));
    }

    public void SpendCard(Sprite cardImage)
    {
        if (!cardCounts.ContainsKey(cardImage) || cardImage == null) return;
        cardCounts[cardImage]--;

        GameObject handCardObj = cardsInHand[cardImage];
        if (handCardObj == null) return;

        if (cardCounts[cardImage] > 0)
        {
            TextMeshProUGUI counterText = handCardObj.GetComponentInChildren<TextMeshProUGUI>();
            if (counterText != null) counterText.text = "x" + cardCounts[cardImage];
        }
        else
        {
            handCardObj.SetActive(false);
            cardCounts[cardImage] = 0;
        }
    }

    public void AddCardToPopupVisuals(Sprite cardImage)
    {
        if (spentCardsContainer == null || cardImage == null) return;

        if (cardsInPopupContainer.ContainsKey(cardImage))
        {
            GameObject popupCardObj = cardsInPopupContainer[cardImage];
            if (popupCardObj != null)
            {
                cardCountsInPopupContainer[cardImage]++;
                if (!popupCardObj.activeSelf) popupCardObj.SetActive(true);

                Image uiImage = popupCardObj.GetComponent<Image>();
                if (uiImage != null) uiImage.sprite = cardImage;

                TextMeshProUGUI counterText = popupCardObj.GetComponentInChildren<TextMeshProUGUI>();
                if (counterText != null) counterText.text = "x" + cardCountsInPopupContainer[cardImage];
            }
        }
        else
        {
            GameObject newPopupCard = Instantiate(cardTemplatePrefab, spentCardsContainer);
            newPopupCard.SetActive(true);

            Image uiImage = newPopupCard.GetComponent<Image>();
            if (uiImage != null) uiImage.sprite = cardImage;

            TextMeshProUGUI counterText = newPopupCard.GetComponentInChildren<TextMeshProUGUI>();
            if (counterText != null)
            {
                counterText.text = "x1";
                counterText.color = Color.white;
            }

            Button cardButton = newPopupCard.GetComponent<Button>();
            if (cardButton == null) cardButton = newPopupCard.AddComponent<Button>();

            cardButton.interactable = true;
            cardButton.onClick.AddListener(() => OnPopupCardClicked(cardImage));

            cardsInPopupContainer.Add(cardImage, newPopupCard);
            cardCountsInPopupContainer.Add(cardImage, 1);
        }
    }

    public void RemoveCardFromPopupVisuals(Sprite cardImage)
    {
        if (!cardCountsInPopupContainer.ContainsKey(cardImage) || cardCountsInPopupContainer[cardImage] <= 0) return;

        cardCountsInPopupContainer[cardImage]--;
        GameObject popupCardObj = cardsInPopupContainer[cardImage];
        if (popupCardObj == null) return;

        if (cardCountsInPopupContainer[cardImage] > 0)
        {
            TextMeshProUGUI counterText = popupCardObj.GetComponentInChildren<TextMeshProUGUI>();
            if (counterText != null) counterText.text = "x" + cardCountsInPopupContainer[cardImage];
        }
        else
        {
            popupCardObj.SetActive(false);
            cardCountsInPopupContainer[cardImage] = 0;
        }
    }
    public void ClearPopupVisuals()
    {
        foreach (GameObject card in spawnedTunnelCards) Destroy(card);
        spawnedTunnelCards.Clear();
        spentCardsTrackList.Clear();
        if (spentCardsContainer != null)
        {
            foreach (Transform child in spentCardsContainer) Destroy(child.gameObject);
        }
        cardsInPopupContainer.Clear();
        cardCountsInPopupContainer.Clear();
    }
}

//// TEST

//using UnityEngine;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine.UI;

//public class PlayerHandUIController : MonoBehaviour
//{
//    public static PlayerHandUIController instance;

//    [Header("Szablony UI")]
//    public GameObject cardTemplatePrefab;

//    [Header("TEST: Pula kart do losowania tunelu")]
//    public List<Sprite> deckSpritesPool = new List<Sprite>();

//    [Header("TEST: Grafika nowej karty na stole po klikniêciu")]
//    public Sprite nextCardSprite;
//    public Button tableCardButton;

//    [Header("JOKER")]
//    [Tooltip("Plik graficzny Lokomotywy, którego u¿ywasz w grze")]
//    public Sprite locomotiveSprite;

//    [Header("--- SEKCJA OKIENKA POPUP ---")]
//    public GameObject routePopupPanel;
//    public Button closePopupButton;
//    public Button confirmPopupButton;

//    [Header("--- SEKCJA TUNELU ---")]
//    public Transform tunnelCardsContainer;
//    public TextMeshProUGUI popupInstructionText;
//    [Tooltip("Kontener z Horizontal Layout Group na karty, które gracz wy³o¿y³ z d³oni")]
//    public Transform spentCardsContainer;

//    private Dictionary<Sprite, GameObject> cardsInHand = new Dictionary<Sprite, GameObject>();
//    private Dictionary<Sprite, int> cardCounts = new Dictionary<Sprite, int>();

//    // S³owniki do obs³ugi grupowania kart wewn¹trz okienka popup
//    private Dictionary<Sprite, GameObject> cardsInPopupContainer = new Dictionary<Sprite, GameObject>();
//    private Dictionary<Sprite, int> cardCountsInPopupContainer = new Dictionary<Sprite, int>();

//    // Zmienne logiczne tunelu i trasy
//    private bool isBuildPopupOpen = false;
//    private bool isTunnelRoute = false;
//    private bool isTunnelWaitingForExtraCards = false;

//    private Sprite chosenRouteColor;
//    private int baseCardsSpent = 0;
//    private int extraCardsRequired = 0;
//    private int extraCardsSpent = 0;
//    private List<GameObject> spawnedTunnelCards = new List<GameObject>();

//    // Referencja do aktualnie klikniêtej trasy na mapie
//    private RouteClickDetector currentTestRoute;

//    private StationClickDetector currentTestStation; // Zapamiêtuje klikniêt¹ stacjê
//    private int requiredStationCardCount = 0;        // Ile kart wymaga ta stacja (1, 2 lub 3)
//    private bool isBuildingStation = false;          // Czy aktualnie budujemy stacjê, czy trasê?

//    private Vector2 confirmButtonDefaultPosition;

//    void Awake()
//    {
//        if (instance == null) instance = this;
//        else Destroy(gameObject);
//    }

//    void Start()
//    {
//        if (tableCardButton != null) tableCardButton.onClick.AddListener(HandleTestClick);
//        if (closePopupButton != null) closePopupButton.onClick.AddListener(CancelRouteBuild);
//        if (confirmPopupButton != null) confirmPopupButton.onClick.AddListener(ProcessConfirmButton);
//        if (routePopupPanel != null) routePopupPanel.SetActive(false);

//        // Zapamiêtujemy startow¹ pozycjê przycisku Confirm w uk³adzie UI
//        if (confirmPopupButton != null)
//        {
//            confirmButtonDefaultPosition = confirmPopupButton.GetComponent<RectTransform>().anchoredPosition;
//        }
//    }

//    // Funkcja wywo³ywana z poziomu RouteClickDetector po klikniêciu trasy na mapie
//    public void OpenRoutePopup(bool isTunnel, bool isStation = false, int stationCost = 0)
//    {
//        if (routePopupPanel == null) return;

//        isBuildPopupOpen = true;
//        isTunnelRoute = isTunnel;
//        isBuildingStation = isStation;
//        requiredStationCardCount = stationCost;
//        isTunnelWaitingForExtraCards = false;

//        baseCardsSpent = 0;
//        extraCardsRequired = 0;
//        extraCardsSpent = 0;
//        chosenRouteColor = null;

//        ClearTunnelUI();

//        // Upewniamy siê, ¿e przyciski i kontenery tunelu s¹ schowane, jeœli budujemy zwyk³¹ stacjê
//        if (tunnelCardsContainer != null) tunnelCardsContainer.gameObject.SetActive(!isStation && isTunnel);
//        if (closePopupButton != null) closePopupButton.gameObject.SetActive(true);

//        if (confirmPopupButton != null)
//        {
//            confirmPopupButton.GetComponent<RectTransform>().anchoredPosition = confirmButtonDefaultPosition;
//        }

//        routePopupPanel.SetActive(true);

//        // Ustawiamy odpowiedni tekst instrukcji dla gracza
//        if (popupInstructionText != null)
//        {
//            if (isStation)
//            {
//                popupInstructionText.text = $"Building a Station: Lay down {stationCost} card(s) of the same color and click Confirm";
//            }
//            else
//            {
//                popupInstructionText.text = isTunnel
//                    ? "Building a Tunnel: Select the basic cards from your hand and click Confirm"
//                    : "Select cards from your hand and click Confirm";
//            }
//        }
//    }

//    // Funkcja do zapamiêtania stacji (analogicznie do trasy)
//    public void SetCurrentActiveStationTest(StationClickDetector station)
//    {
//        currentTestStation = station;
//    }

//    public void SetCurrentActiveRouteTest(RouteClickDetector route)
//    {
//        currentTestRoute = route;
//    }

//    private void ProcessConfirmButton()
//    {
//        if (!isTunnelRoute)
//        {
//            FinalizeRoute();
//        }
//        else
//        {
//            if (!isTunnelWaitingForExtraCards)
//            {
//                StartTunnelDrawPhase();
//            }
//            else
//            {
//                if (extraCardsSpent >= extraCardsRequired) FinalizeRoute();
//                else if (popupInstructionText != null)
//                    popupInstructionText.text = $"You need to pay an additional {extraCardsRequired - extraCardsSpent} card(s)";
//            }
//        }
//    }

//    private void StartTunnelDrawPhase()
//    {
//        if (baseCardsSpent == 0 || chosenRouteColor == null)
//        {
//            if (popupInstructionText != null) popupInstructionText.text = "You must first play at least one basic card";
//            return;
//        }

//        isTunnelWaitingForExtraCards = true;
//        ClearTunnelUI();

//        for (int i = 0; i < 3; i++)
//        {
//            if (deckSpritesPool.Count == 0) break;
//            Sprite drawnSprite = deckSpritesPool[Random.Range(0, deckSpritesPool.Count)];

//            GameObject cardObj = Instantiate(cardTemplatePrefab, tunnelCardsContainer);
//            cardObj.SetActive(true);

//            if (cardObj.GetComponent<Image>() != null) cardObj.GetComponent<Image>().sprite = drawnSprite;
//            if (cardObj.GetComponent<Button>() != null) cardObj.GetComponent<Button>().interactable = false;

//            spawnedTunnelCards.Add(cardObj);

//            bool isLocomotive = (locomotiveSprite != null && drawnSprite == locomotiveSprite);

//            if (drawnSprite == chosenRouteColor || isLocomotive)
//            {
//                extraCardsRequired++;
//            }
//        }

//        // LOGIKA CENTROWANIA I WIDOCZNOŒCI PRZYCISKÓW:
//        if (extraCardsRequired > 0)
//        {
//            if (popupInstructionText != null)
//                popupInstructionText.text = $"{extraCardsRequired} matching cards have been drawn! Select the additional cards and click confirm";

//            if (closePopupButton != null) closePopupButton.gameObject.SetActive(true);

//            // Dop³ata jest wymagana -> przycisk Confirm wraca na swoj¹ standardow¹ pozycjê z boku
//            if (confirmPopupButton != null)
//            {
//                confirmPopupButton.GetComponent<RectTransform>().anchoredPosition = confirmButtonDefaultPosition;
//            }
//        }
//        else
//        {
//            if (popupInstructionText != null)
//                popupInstructionText.text = "Success! 0 matching cards. Click Confirm to complete the tunnel";

//            if (closePopupButton != null) closePopupButton.gameObject.SetActive(false);

//            // Szczêœliwy traf (0 dop³at) -> przesuwamy przycisk Confirm idealnie na œrodek (X = 0)
//            if (confirmPopupButton != null)
//            {
//                RectTransform rect = confirmPopupButton.GetComponent<RectTransform>();
//                rect.anchoredPosition = new Vector2(0f, rect.anchoredPosition.y);
//            }
//        }
//    }

//    private void FinalizeRoute()
//    {
//        if (isBuildingStation)
//        {
//            Debug.Log("[Sukces] Stacja pomyœlnie wybudowana!");
//            if (currentTestStation != null)
//            {
//                currentTestStation.BuildStationVisuals();
//            }
//        }
//        else
//        {
//            Debug.Log("[Sukces] Trasa/Tunel pomyœlnie wybudowana!");
//            if (currentTestRoute != null)
//            {
//                currentTestRoute.BuildRouteVisuals();
//            }
//        }

//        isBuildPopupOpen = false;
//        isBuildingStation = false;
//        routePopupPanel.SetActive(false);
//        ClearTunnelUI();
//    }

//    // LISTA PAMIÊCI KOLEJNOŒCI WY£O¯ONYCH KART
//    private List<Sprite> spentCardsTrackList = new List<Sprite>();

//    private void OnHandCardClicked(Sprite clickedCardSprite)
//    {
//        if (!isBuildPopupOpen) return;

//        bool isLocomotive = (locomotiveSprite != null && clickedCardSprite == locomotiveSprite);

//        if (chosenRouteColor == null && !isTunnelWaitingForExtraCards && !isLocomotive)
//        {
//            chosenRouteColor = clickedCardSprite;
//        }

//        // --- FAZA 1: BUDOWA PODSTAWY ---
//        if (!isTunnelWaitingForExtraCards)
//        {
//            if (clickedCardSprite == chosenRouteColor || isLocomotive || chosenRouteColor == null)
//            {
//                if (chosenRouteColor == null && !isLocomotive)
//                {
//                    chosenRouteColor = clickedCardSprite;
//                }

//                baseCardsSpent++;
//                spentCardsTrackList.Add(clickedCardSprite);
//                SpendCard(clickedCardSprite);

//                // MODYFIKACJA: Wywo³ujemy grupowane dodawanie do okienka popup
//                AddCardToPopupVisuals(clickedCardSprite);

//                if (popupInstructionText != null) popupInstructionText.text = $"{baseCardsSpent} base cards have been laid out";
//            }
//            else
//            {
//                Debug.LogWarning($"[Budowanie] Ta karta ({clickedCardSprite.name}) nie pasuje do koloru trasy!");
//            }
//        }
//        // --- FAZA 2: DOP£ATA DO TUNELU ---
//        else
//        {
//            if (extraCardsSpent < extraCardsRequired && (clickedCardSprite == chosenRouteColor || isLocomotive))
//            {
//                extraCardsSpent++;
//                spentCardsTrackList.Add(clickedCardSprite);
//                SpendCard(clickedCardSprite);

//                // Wywo³ujemy grupowane dodawanie do okienka popup!
//                AddCardToPopupVisuals(clickedCardSprite);

//                if (popupInstructionText != null)
//                    popupInstructionText.text = $"{extraCardsSpent}/{extraCardsRequired} additional cards were paid";
//            }
//            else
//            {
//                Debug.LogWarning("[Budowanie] Dop³ata musi byæ w kolorze trasy lub lokomotyw¹!");
//            }
//        }
//    }

//    // Grupowanie kafelków i liczników wewn¹trz CardsContainer w okienku popup
//    private void AddCardToPopupVisuals(Sprite cardImage)
//    {
//        if (spentCardsContainer == null || cardImage == null) return;

//        // Sytuacja A: Ten typ karty ju¿ jest w popupie
//        if (cardsInPopupContainer.ContainsKey(cardImage))
//        {
//            if (cardsInPopupContainer[cardImage] != null)
//            {
//                cardCountsInPopupContainer[cardImage]++;

//                // Jeœli kafelek by³ ukryty, w³¹czamy go i upewniamy siê, ¿e ma poprawn¹ grafikê
//                if (!cardsInPopupContainer[cardImage].activeSelf)
//                {
//                    cardsInPopupContainer[cardImage].SetActive(true);
//                }

//                // Wymuszamy, aby kafelek pokazywa³ dok³adnie tê kartê, któr¹ kliknêliœmy
//                Image uiImage = cardsInPopupContainer[cardImage].GetComponent<Image>();
//                if (uiImage != null)
//                {
//                    uiImage.sprite = cardImage;
//                }

//                TextMeshProUGUI counterText = cardsInPopupContainer[cardImage].GetComponentInChildren<TextMeshProUGUI>();
//                if (counterText != null)
//                {
//                    counterText.text = "x" + cardCountsInPopupContainer[cardImage];
//                }
//            }
//        }
//        // Sytuacja B: Pierwszy raz wyk³adamy tê kartê do popupu
//        else
//        {
//            GameObject newPopupCard = Instantiate(cardTemplatePrefab, spentCardsContainer);
//            newPopupCard.SetActive(true);

//            Image uiImage = newPopupCard.GetComponent<Image>();
//            if (uiImage != null) uiImage.sprite = cardImage;

//            TextMeshProUGUI counterText = newPopupCard.GetComponentInChildren<TextMeshProUGUI>();
//            if (counterText != null)
//            {
//                counterText.text = "x1";
//                counterText.color = Color.white;
//            }

//            // Zamiast wy³¹czaæ przycisk, zostawiamy go aktywnego i przypisujemy mu klikniêcie
//            Button cardButton = newPopupCard.GetComponent<Button>() ?? newPopupCard.AddComponent<Button>();
//            cardButton.interactable = true;
//            cardButton.onClick.AddListener(() => OnPopupCardClicked(cardImage));

//            cardsInPopupContainer.Add(cardImage, newPopupCard);
//            cardCountsInPopupContainer.Add(cardImage, 1);
//        }
//    }

//    // Ta funkcja wywo³uje siê, gdy gracz kliknie kartê na œrodku okienka (chce j¹ zabraæ z powrotem)
//    private void OnPopupCardClicked(Sprite clickedCardSprite)
//    {
//        // Nie pozwalamy na wycofanie kart, jeœli gra przesz³a ju¿ do losowania tunelu (faza dop³at)
//        if (isTunnelWaitingForExtraCards) return;

//        if (!cardCountsInPopupContainer.ContainsKey(clickedCardSprite) || cardCountsInPopupContainer[clickedCardSprite] <= 0) return;

//        // 1. Zmniejszamy licznik w pamiêci okienka popup
//        cardCountsInPopupContainer[clickedCardSprite]--;

//        // Zmniejszamy te¿ ogólny licznik bazowych kart wy³o¿onych na trasê
//        baseCardsSpent--;

//        // 2. Aktualizujemy wygl¹d w okienku popup
//        if (cardCountsInPopupContainer[clickedCardSprite] > 0)
//        {
//            TextMeshProUGUI counterText = cardsInPopupContainer[clickedCardSprite].GetComponentInChildren<TextMeshProUGUI>();
//            if (counterText != null)
//            {
//                counterText.text = "x" + cardCountsInPopupContainer[clickedCardSprite];
//            }
//        }
//        else
//        {
//            // Jeœli licznik spad³ do 0, ukrywamy ca³y kafelek w okienku popup
//            cardsInPopupContainer[clickedCardSprite].SetActive(false);
//        }

//        // 3. Oddajemy kartê spowrotem do d³oni gracza
//        AddCardToHand(clickedCardSprite);

//        // 4. Usuwamy te kartê z listy pamiêci trasy (szukamy pierwszej od koñca i usuwamy)
//        int indexToRemove = spentCardsTrackList.LastIndexOf(clickedCardSprite);
//        if (indexToRemove != -1)
//        {
//            spentCardsTrackList.RemoveAt(indexToRemove);
//        }

//        // 5. Resetujemy kolor wiod¹cy trasy, jeœli gracz wycofa³ ju¿ absolutnie wszystkie karty podstawowe
//        if (baseCardsSpent == 0)
//        {
//            chosenRouteColor = null;
//            if (popupInstructionText != null)
//                popupInstructionText.text = isTunnelRoute
//                    ? "Choose cards to build a tunnel"
//                    : "Choose cards to build a tunnel";
//        }
//        else
//        {
//            if (popupInstructionText != null)
//                popupInstructionText.text = $"{baseCardsSpent} base cards have been laid out";
//        }

//        Debug.Log($"[Cofniêcie] Gracz zabra³ kartê {clickedCardSprite.name} z powrotem do d³oni.");
//    }

//    public void CancelRouteBuild()
//    {
//        if (!isBuildPopupOpen) return;

//        int returnedCount = 0;
//        // Oddajemy dok³adnie te same karty, które zosta³y wrzucone do listy
//        foreach (Sprite cardToReturn in spentCardsTrackList)
//        {
//            if (cardToReturn != null)
//            {
//                AddCardToHand(cardToReturn);
//                returnedCount++;
//            }
//        }

//        Debug.Log($"[Zwrot] Pomyœlnie zwrócono {returnedCount} fizycznych kart do d³oni.");

//        isBuildPopupOpen = false;
//        routePopupPanel.SetActive(false);
//        ClearTunnelUI();
//    }

//    // Czyszczenie naszej nowej listy testowej
//    private void ClearTunnelUI()
//    {
//        foreach (GameObject card in spawnedTunnelCards) Destroy(card);
//        spawnedTunnelCards.Clear();

//        spentCardsTrackList.Clear();

//        if (spentCardsContainer != null)
//        {
//            foreach (Transform child in spentCardsContainer) Destroy(child.gameObject);
//        }

//        // Czyszczenie s³owników popupu przy resetowaniu okna
//        cardsInPopupContainer.Clear();
//        cardCountsInPopupContainer.Clear();
//    }

//    private void HandleTestClick()
//    {
//        Image cardImage = tableCardButton.GetComponent<Image>();
//        if (cardImage != null && cardImage.sprite != null)
//        {
//            AddCardToHand(cardImage.sprite);
//            if (nextCardSprite != null) cardImage.sprite = nextCardSprite;
//        }
//    }

//    public void AddCardToHand(Sprite cardImage)
//    {
//        if (cardTemplatePrefab == null || cardImage == null) return;

//        // Sytuacja 1: Karta ju¿ istnieje w pamiêci d³oni
//        if (cardsInHand.ContainsKey(cardImage))
//        {
//            // Bezpiecznie sprawdzamy, czy obiekt nie zosta³ wczeœniej fizycznie zniszczony
//            if (cardsInHand[cardImage] != null)
//            {
//                cardCounts[cardImage]++;

//                // Pobieramy komponent tekstowy bezpoœrednio z zapisanego obiektu karty
//                TextMeshProUGUI counterText = cardsInHand[cardImage].GetComponentInChildren<TextMeshProUGUI>();
//                if (counterText != null)
//                {
//                    counterText.text = "x" + cardCounts[cardImage];
//                }

//                // Jeœli kafelek by³ ukryty na ekranie (licznik = 0), w³¹czamy go z powrotem
//                if (!cardsInHand[cardImage].activeSelf)
//                {
//                    cardsInHand[cardImage].SetActive(true);
//                }
//            }
//            else
//            {
//                // Na wypadek gdyby stary obiekt zosta³ usuniêty przez Destroy z poprzednich testów, oczyszczamy s³ownik i tworzymy go na nowo poni¿ej
//                cardsInHand.Remove(cardImage);
//                cardCounts.Remove(cardImage);
//                CreateNewCardInHand(cardImage);
//            }
//        }
//        // Sytuacja 2: Pierwszy raz dodajemy tê kartê do rêki
//        else
//        {
//            CreateNewCardInHand(cardImage);
//        }
//    }

//    // Funkcja pomocnicza tworz¹ca nowy kafelek w d³oni od zera
//    private void CreateNewCardInHand(Sprite cardImage)
//    {
//        GameObject newCard = Instantiate(cardTemplatePrefab, transform);
//        newCard.SetActive(true);

//        Image uiImage = newCard.GetComponent<Image>();
//        if (uiImage != null) uiImage.sprite = cardImage;

//        TextMeshProUGUI counterText = newCard.GetComponentInChildren<TextMeshProUGUI>();
//        if (counterText != null)
//        {
//            counterText.text = "x1";
//            counterText.color = Color.black;
//        }

//        // ZAPISUJEMY CA£Y OBIEKT KARTY DO S£OWNIKA (Zamiast samego komponentu tekstowego)
//        cardsInHand.Add(cardImage, newCard);
//        cardCounts.Add(cardImage, 1);

//        Button cardButton = newCard.GetComponent<Button>() ?? newCard.AddComponent<Button>();
//        cardButton.onClick.AddListener(() => OnHandCardClicked(cardImage));
//    }

//    public void SpendCard(Sprite cardImage)
//    {
//        if (!cardCounts.ContainsKey(cardImage) || cardImage == null) return;

//        cardCounts[cardImage]--;

//        if (cardCounts[cardImage] > 0)
//        {
//            if (cardsInHand[cardImage] != null)
//            {
//                TextMeshProUGUI counterText = cardsInHand[cardImage].GetComponentInChildren<TextMeshProUGUI>();
//                if (counterText != null)
//                {
//                    counterText.text = "x" + cardCounts[cardImage];
//                }
//            }
//        }
//        else
//        {
//            // BEZPIECZNE UKRYWANIE: Odwo³ujemy siê bezpoœrednio do zapisanego obiektu, 
//            // bez u¿ywania awaryjnego GetComponentInParent
//            if (cardsInHand[cardImage] != null)
//            {
//                cardsInHand[cardImage].SetActive(false);
//            }
//            cardCounts[cardImage] = 0;
//        }
//    }
//}