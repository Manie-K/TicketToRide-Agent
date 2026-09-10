using CoreEngine;
using CoreEngine.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicTest : MonoBehaviour
{
    // Prosta funkcja "gracza": zawsze wybiera pierwszą dostępną opcję (indeks 0)
    private int DummyChoice(IEnumerable choices)
    {
        return 0;
    }

    void Start()
    {
        var players = new List<Player>
        {
            new Player(DummyChoice, PlayerColor.Red),
            new Player(DummyChoice, PlayerColor.Blue)
        };

        var manager = new GameManager(players, GameMode.Live);

        Debug.Log("Test działa! Utworzono GameManagera z " + manager.Players.Count + " graczami.");
    }
}