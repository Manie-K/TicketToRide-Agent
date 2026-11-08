using UnityEngine;
using UnityEngine.SceneManagement; // Wymagane do przełączania scen!

public class MainMenuController : MonoBehaviour
{
    // Ta metoda uruchomi Twoją główną planszę gry
    public void StartGame()
    {
        Debug.Log("[MENU] Przechodzenie do planszy gry...");
        SceneManager.LoadScene("SampleScene"); // Wpisz dokładnie taką nazwę, jaką ma Twoja scena z mapą!
    }

    // Ta metoda wyłącza grę
    public void QuitGame()
    {
        Debug.Log("[MENU] Wychodzenie z gry...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}