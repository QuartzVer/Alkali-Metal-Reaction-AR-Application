using UnityEngine;
using UnityEngine.SceneManagement;

public class ReactSceneUI : MonoBehaviour
{
    // Feature 1: Replay Button Function
    public void ReplayReaction()
    {
        // Reloads the active scene (Index 2), resetting the water and metal objects
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    // Feature 2: Next Button Function
    public void GoToExplanationScene()
    {
        // Loads the Explanation scene (Index 3)
        SceneManager.LoadSceneAsync(3);
    }
}
