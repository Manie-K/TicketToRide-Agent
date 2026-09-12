using UnityEngine;

public class CoordinatePicker : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f;
            Debug.Log($"Position: new Vector2({worldPos.x:F2}f, {worldPos.y:F2}f)");
        }
    }
}