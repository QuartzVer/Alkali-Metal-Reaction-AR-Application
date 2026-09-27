using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//creating functions in unity

public class Menu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(0); // selecting which scene we want to load, important, put scene number or scene name with ""
    }

    public void ScanMarker()
    {
        SceneManager.LoadSceneAsync(1); // selecting which scene we want to load, important, put scene number or scene name with ""
    }
    public void OpenQuizScene()
    {
        SceneManager.LoadSceneAsync(4);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}