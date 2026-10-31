using UnityEngine;
using TMPro; 
public class PlayerUIItem : MonoBehaviour
{
    [Header("Podepnij tutaj tekst z nazw¹ gracza:")]
    public TextMeshProUGUI playerNameText;

    [Header("Podepnij tutaj tekst z liczb¹ wagoników:")]
    public TextMeshProUGUI playerCarriagesText;

    private Vector3 targetScale = Vector3.one;
    public float animationSpeed = 8f;

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    public void SetPlayerName(string newName)
    {
        if (playerNameText != null)
        {
            playerNameText.text = newName;
        }
    }

    public void SetActiveStatus(bool isActive)
    {
        targetScale = isActive ? new Vector3(1.12f, 1.12f, 1f) : Vector3.one;
    }

    public void SetPlayerCarriages(int carriagesCount)
    {
        if (playerCarriagesText != null)
        {
            playerCarriagesText.text = "Carriages: " + carriagesCount;
        }
    }
}