using UnityEditor;
using UnityEngine;
using CoreEngine.Game;
using CoreEngine.UnityData;

namespace CoreEngine.Editor
{
    public static class MapDataGenerator
    {
        private const string CitiesFolder =
            "Assets/Scripts/GameData/Cities";

        private const string RoutesFolder =
            "Assets/Scripts/GameData/Routes";


        // ============================================================
        // CITIES
        // ============================================================

        [MenuItem("Ticket to Ride/Generate City Data")]
        public static void GenerateCities()
        {
            MapDefinition.Build();

            if (!AssetDatabase.IsValidFolder(CitiesFolder))
            {
                Debug.LogError(
                    $"Folder nie istnieje: {CitiesFolder}"
                );
                return;
            }

            int created = 0;
            int updated = 0;

            foreach (var city in MapDefinition.Cities.Values)
            {
                string assetPath =
                    $"{CitiesFolder}/{city.Name}.asset";

                CityData cityData =
                    AssetDatabase.LoadAssetAtPath<CityData>(assetPath);

                if (cityData == null)
                {
                    cityData =
                        ScriptableObject.CreateInstance<CityData>();

                    cityData.CityName = city.Name;
                    cityData.BoardPosition = Vector2.zero;

                    AssetDatabase.CreateAsset(
                        cityData,
                        assetPath
                    );

                    created++;
                }
                else
                {
                    cityData.CityName = city.Name;

                    EditorUtility.SetDirty(cityData);

                    updated++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"City Data generation finished. " +
                $"Created: {created}, Updated: {updated}"
            );
        }


        // ============================================================
        // ROUTES
        // ============================================================

        [MenuItem("Ticket to Ride/Generate Route Data")]
        public static void GenerateRoutes()
        {
            MapDefinition.Build();

            if (!AssetDatabase.IsValidFolder(RoutesFolder))
            {
                Debug.LogError(
                    $"Folder nie istnieje: {RoutesFolder}"
                );
                return;
            }

            int created = 0;
            int updated = 0;

            for (int i = 0; i < MapDefinition.Routes.Count; i++)
            {
                var route = MapDefinition.Routes[i];

                string fileName =
                    $"Route_{i + 1:D3}_{route.Origin.Name}_{route.Destination.Name}.asset";

                string assetPath =
                    $"{RoutesFolder}/{fileName}";

                RouteData routeData =
                    AssetDatabase.LoadAssetAtPath<RouteData>(
                        assetPath
                    );

                if (routeData == null)
                {
                    routeData =
                        ScriptableObject.CreateInstance<RouteData>();

                    created++;
                }
                else
                {
                    updated++;
                }

                routeData.Origin =
                    LoadCityData(route.Origin.Name);

                routeData.Destination =
                    LoadCityData(route.Destination.Name);

                routeData.Length =
                    route.Length;

                routeData.Color =
                    route.Color;

                routeData.IsTunnel =
                    route.IsTunnel;

                routeData.LocomotivesNeeded =
                    route.LocomotivesNeeded;

                if (AssetDatabase.LoadAssetAtPath<RouteData>(
                        assetPath) == null)
                {
                    AssetDatabase.CreateAsset(
                        routeData,
                        assetPath
                    );
                }
                else
                {
                    EditorUtility.SetDirty(routeData);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Route Data generation finished. " +
                $"Created: {created}, Updated: {updated}"
            );
        }


        // ============================================================
        // HELPERS
        // ============================================================

        private static CityData LoadCityData(string cityName)
        {
            string assetPath =
                $"{CitiesFolder}/{cityName}.asset";

            CityData cityData =
                AssetDatabase.LoadAssetAtPath<CityData>(assetPath);

            if (cityData == null)
            {
                Debug.LogError(
                    $"Nie znaleziono CityData dla miasta: {cityName}"
                );
            }

            return cityData;
        }
    }
}