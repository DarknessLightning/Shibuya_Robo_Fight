using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void ChangeScene(string sceneName)
    {
        if (sceneName == "")
        {
            Debug.Log("Play!");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    public void OpenPanel(GameObject panel = null)
    {
        if (panel == null)
        {
            Debug.Log("Panel Opened!");
            return;
        }
        panel.SetActive(true);
    }

    public void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
