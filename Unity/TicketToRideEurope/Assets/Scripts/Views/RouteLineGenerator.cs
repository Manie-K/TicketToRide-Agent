using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using CoreEngine.Cards;
using System.Collections.Generic;
using TMPro;
using CoreEngine.UnityData;
using CoreEngine;

public class RouteLineGenerator : MonoBehaviour
{
    public Sprite squareSprite;
    public float segmentThickness = 0.15f;
    public float segmentGap = 0.05f;
    public float parallelOffset = 0.15f;
    public float borderThickness = 0.02f;
    public float tunnelBorderThickness = 0.06f;
    public string routesFolder = "Assets/Scripts/GameData/Routes";

    [ContextMenu("Generate Routes")]
    public void GenerateRoutes()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        RouteData[] routes = LoadAllRoutes();

        var groups = new Dictionary<string, List<RouteData>>();
        foreach (var route in routes)
        {
            if (route.Origin == null || route.Destination == null) continue;
            string key = string.CompareOrdinal(route.Origin.CityName, route.Destination.CityName) < 0
                ? route.Origin.CityName + "_" + route.Destination.CityName
                : route.Destination.CityName + "_" + route.Origin.CityName;
            if (!groups.ContainsKey(key)) groups[key] = new List<RouteData>();
            groups[key].Add(route);
        }

        foreach (var kvp in groups)
        {
            List<RouteData> group = kvp.Value;
            int total = group.Count;

            for (int g = 0; g < total; g++)
            {
                RouteData route = group[g];

                Vector2 start = route.Origin.BoardPosition;
                Vector2 end = route.Destination.BoardPosition;
                Vector2 direction = (end - start).normalized;
                Vector2 perpendicular = new Vector2(-direction.y, direction.x);

                float offsetAmount = (g - (total - 1) / 2f) * parallelOffset;
                start += perpendicular * offsetAmount;
                end += perpendicular * offsetAmount;

                float totalDistance = Vector2.Distance(start, end);
                int count = Mathf.Max(route.Length, 1);
                float segmentLength = (totalDistance / count) - segmentGap;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.Euler(0, 0, angle);
                Color color = GetColorFor(route.Color);
                float border = route.IsTunnel ? tunnelBorderThickness : borderThickness;

                GameObject routeParent = new GameObject("Route_" + route.Origin.CityName + "_" + route.Destination.CityName + "_" + g);
                routeParent.transform.SetParent(transform);

                for (int i = 0; i < count; i++)
                {
                    float centerDist = (totalDistance / count) * (i + 0.5f);
                    Vector2 pos = start + direction * centerDist;
                    Vector3 pos3D = new Vector3(pos.x, pos.y, 0f);

                    GameObject segmentHolder = new GameObject("Segment_" + i);
                    segmentHolder.transform.SetParent(routeParent.transform);
                    segmentHolder.transform.position = pos3D;
                    segmentHolder.transform.rotation = rotation;

                    GameObject borderObj = new GameObject("Border");
                    borderObj.transform.SetParent(segmentHolder.transform);
                    borderObj.transform.localPosition = Vector3.zero;
                    borderObj.transform.localRotation = Quaternion.identity;
                    var borderSr = borderObj.AddComponent<SpriteRenderer>();
                    borderSr.sprite = squareSprite;
                    borderSr.color = Color.black;
                    borderSr.sortingOrder = 4;
                    borderObj.transform.localScale = new Vector3(segmentLength + border * 2f, segmentThickness + border * 2f, 1f);

                    GameObject fillObj = new GameObject("Fill");
                    fillObj.transform.SetParent(segmentHolder.transform);
                    fillObj.transform.localPosition = Vector3.zero;
                    fillObj.transform.localRotation = Quaternion.identity;
                    var fillSr = fillObj.AddComponent<SpriteRenderer>();
                    fillSr.sprite = squareSprite;
                    fillSr.color = color;
                    fillSr.sortingOrder = 5;
                    fillObj.transform.localScale = new Vector3(segmentLength, segmentThickness, 1f);

                    if (i < route.LocomotivesNeeded)
                    {
                        GameObject labelObj = new GameObject("Label_L");
                        labelObj.transform.SetParent(routeParent.transform);
                        labelObj.transform.position = pos3D;
                        labelObj.transform.rotation = Quaternion.identity;

                        var tmp = labelObj.AddComponent<TextMeshPro>();
                        tmp.text = "L";
                        tmp.fontSize = 3;
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.color = Color.white;
                        tmp.sortingOrder = 6;
                    }
                }
            }
        }
    }

    private RouteData[] LoadAllRoutes()
    {
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:RouteData", new[] { routesFolder });
        var list = new List<RouteData>();
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var route = AssetDatabase.LoadAssetAtPath<RouteData>(path);
            if (route != null) list.Add(route);
        }
        return list.ToArray();
#else
        return new RouteData[0];
#endif
    }

    private Color GetColorFor(TrainColor color)
    {
        switch (color.ToString().ToLower())
        {
            case "red": return Color.red;
            case "blue": return Color.blue;
            case "green": return Color.green;
            case "yellow": return Color.yellow;
            case "orange": return new Color(1f, 0.5f, 0f);
            case "black": return Color.black;
            case "white": return Color.white;
            case "pink":
            case "purple": return Color.magenta;
            default: return Color.gray;
        }
    }
}