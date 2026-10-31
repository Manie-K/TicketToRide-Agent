using UnityEngine;
using System.Collections.Generic;

public class TurnShowcaseTester : MonoBehaviour
{
    [Header("Ustawienia testu tury:")]
    [Tooltip("Ilu graczy ma braæ udzia³ w tym teœcie? (np. 3)")]
    public int testTotalPlayers = 3;

    [Tooltip("Co ile sekund ma zmieniaæ siê tura?")]
    public float timeBetweenTurns = 5f;

    // Przyk³adowe imiona do wyœwietlenia na banerze
    private List<string> sampleNames = new List<string>() { "Kot", "Pies", "Królik", "Papuga", "W¹¿" };
    private int currentTestPlayer = 1;

    void Start()
    {
        if (TurnUIController.instance == null) return;

        testTotalPlayers = Mathf.Clamp(testTotalPlayers, 2, 5);

        // Przekazujemy liczbê graczy ORAZ ca³¹ listê imion na start!
        TurnUIController.instance.InitializePlayerList(testTotalPlayers, sampleNames);

        InvokeRepeating("SimulateNextTurn", 1f, timeBetweenTurns);
    }

    void SimulateNextTurn()
    {
        // Pobieramy ³adne imiê z listy dla aktualnego numeru gracza
        string currentName = sampleNames[currentTestPlayer - 1];

        // Wysy³amy sygna³ do Waszego g³ównego kontrolera UI
        TurnUIController.instance.SetActivePlayerTurn(currentTestPlayer, currentName);

        Debug.Log($"[TEST] Prze³¹czono na gracza numer {currentTestPlayer}: {currentName}");

        // Przechodzimy do kolejnego gracza. Jeœli dojdziemy do koñca (np. 3), wracamy do pierwszego (1)
        currentTestPlayer++;
        if (currentTestPlayer > testTotalPlayers)
        {
            currentTestPlayer = 1;
        }
    }
}
