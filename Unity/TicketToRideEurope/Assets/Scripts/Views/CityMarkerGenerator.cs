using CoreEngine.UnityData;
using UnityEngine;

public class CityMarkerGenerator : MonoBehaviour
{
    public GameObject markerPrefab;
    public CityData[] cities;

    [ContextMenu("Generate Markers")]
    public void GenerateMarkers()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        foreach (var city in cities)
        {
            GameObject marker = Instantiate(markerPrefab, transform);
            marker.transform.position = new Vector3(city.BoardPosition.x, city.BoardPosition.y, 0f);
            marker.name = "City_" + city.CityName;
        }
    }
}