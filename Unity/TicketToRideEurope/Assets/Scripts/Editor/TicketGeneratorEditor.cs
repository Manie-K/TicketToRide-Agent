#if UNITY_EDITOR
using CoreEngine.Cards;
using CoreEngine.Game;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TicketGeneratorEditor
{
    [MenuItem("TicketToRide/Generuj Wszystkie Bilety do Folderu")]
    public static void GenerateTickets()
    {
        string folderPath = "Assets/Scripts/GameData/GeneratedTickets";

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Pobieramy zarówno zwykłe, jak i długie bilety razem!
        List<TicketCard> tickets = MapDefinition.BuildRegularTickets()
            .Concat(MapDefinition.BuildLongRouteTickets())
            .ToList();

        int count = 0;
        foreach (var ticket in tickets)
        {
            TicketCardData asset = ScriptableObject.CreateInstance<TicketCardData>();

            // Pobieramy poprawnie nazwy miast jako stringi
            asset.city1 = ticket.Origin.Name;
            asset.city2 = ticket.Destination.Name;
            asset.points = ticket.Points;

            // Tworzymy czytelną nazwę pliku
            string fileName = $"{ticket.Origin.Name}_to_{ticket.Destination.Name}_{ticket.Points}pts.asset";
            string fullPath = Path.Combine(folderPath, fileName);

            AssetDatabase.CreateAsset(asset, fullPath);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[GENERATOR] Pomyślnie wygenerowano łącznie {count} kart biletów w folderze: {folderPath}");
    }
}
#endif