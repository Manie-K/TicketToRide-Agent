using UnityEngine;
using TMPro;
using System.Collections.Generic; 

public class TurnUIController : MonoBehaviour
{
    public static TurnUIController instance;

    [Header("Górny Baner Tury:")]
    public TextMeshProUGUI turnBannerText;

    [Header("Lista Kafelków Graczy (Wszystkie 5 okienek):")]
    public PlayerUIItem[] allPlayersUI;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void InitializePlayerList(int totalPlayersInGame, List<string> playerNames)
    {
        for (int i = 0; i < allPlayersUI.Length; i++)
        {
            if (allPlayersUI[i] != null)
            {
                bool shouldBeVisible = (i < totalPlayersInGame);
                allPlayersUI[i].gameObject.SetActive(shouldBeVisible);

                if (shouldBeVisible && playerNames != null && i < playerNames.Count)
                {
                    allPlayersUI[i].SetPlayerName(playerNames[i]);
                }
            }
        }
    }

    public void SetActivePlayerTurn(int playerNumber, string playerCustomName)
    {
        if (turnBannerText != null)
        {
            turnBannerText.text = "Player's turn: " + playerCustomName;
        }

        for (int i = 0; i < allPlayersUI.Length; i++)
        {
            if (allPlayersUI[i] != null && allPlayersUI[i].gameObject.activeSelf)
            {
                bool isThisPlayerActive = (i == playerNumber - 1);
                allPlayersUI[i].SetActiveStatus(isThisPlayerActive);

                if (isThisPlayerActive)
                {
                    allPlayersUI[i].SetPlayerName(playerCustomName);
                }
            }
        }
    }

    public void UpdatePlayerCarriagesUI(int playerNumber, int currentCarriages)
    {
        int index = playerNumber - 1;
        if (index >= 0 && index < allPlayersUI.Length && allPlayersUI[index] != null)
        {
            allPlayersUI[index].SetPlayerCarriages(currentCarriages);
        }
    }
}
