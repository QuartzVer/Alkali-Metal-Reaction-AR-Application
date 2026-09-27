using UnityEngine;
using UnityEngine.SceneManagement;

public class AlkaliManager : MonoBehaviour
{
    // The memory bridge variable to remember the selection across different scenes
    public static string LastScannedMetal = "None";

    [System.Serializable]
    public class MetalUIConfig
    {
        public string metalName; // Tag to distinguish elements (e.g. "Lithium", "Sodium", "Potassium")
        public GameObject wholeUIPage;
        public GameObject associatedModel;
    }

    [Header("Element Setups")]
    public MetalUIConfig lithiumSetup;
    public MetalUIConfig sodiumSetup;
    public MetalUIConfig potassiumSetup;

    private MetalUIConfig currentActiveSetup;

    void Start()
    {
        HideSetup(lithiumSetup);
        HideSetup(sodiumSetup);
        HideSetup(potassiumSetup);
    }

    public void FoundLithium() { lithiumSetup.metalName = "Lithium"; SwitchToElement(lithiumSetup); }
    public void FoundSodium() { sodiumSetup.metalName = "Sodium"; SwitchToElement(sodiumSetup); }
    public void FoundPotassium() { potassiumSetup.metalName = "Potassium"; SwitchToElement(potassiumSetup); }

    private void SwitchToElement(MetalUIConfig incomingSetup)
    {
        if (currentActiveSetup != null)
            HideSetup(currentActiveSetup);

        currentActiveSetup = incomingSetup;
        currentActiveSetup.wholeUIPage.SetActive(true);
        currentActiveSetup.associatedModel.SetActive(true);

        // Save the tracked metal identity to memory
        LastScannedMetal = incomingSetup.metalName;

        currentActiveSetup.associatedModel.transform.localRotation = Quaternion.identity;
    }

    private void HideSetup(MetalUIConfig setup)
    {
        if (setup.wholeUIPage != null) setup.wholeUIPage.SetActive(false);
        if (setup.associatedModel != null) setup.associatedModel.SetActive(false);
    }

    // Call this function when the user clicks the Perform Reaction button!
    public void OnClickPerform()
    {
        if (LastScannedMetal != "None")
        {
            // Assuming your React scene is at Index 2 in your Build Settings window
            SceneManager.LoadSceneAsync(2);
        }
        else
        {
            Debug.LogWarning("No metal marker has been scanned yet!");
        }
    }

    public void OnClickScanMarker()
    {
        SceneManager.LoadSceneAsync(0); // Main Menu
    }
}
