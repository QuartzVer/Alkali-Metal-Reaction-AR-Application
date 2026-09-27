using UnityEngine;
using UnityEngine.SceneManagement;

public class ExplanationManager : MonoBehaviour
{
    [System.Serializable]
    public class ExplanationUIConfig
    {
        public string metalName; // "Lithium", "Sodium", or "Potassium"
        public GameObject explanationPanel; // The Canvas group/panel containing text/graphics
    }

    [Header("Explanation Panels")]
    [SerializeField] private ExplanationUIConfig lithiumExplanation;
    [SerializeField] private ExplanationUIConfig sodiumExplanation;
    [SerializeField] private ExplanationUIConfig potassiumExplanation;

    void Start()
    {
        // Hide all explanation panels by default
        HidePanel(lithiumExplanation);
        HidePanel(sodiumExplanation);
        HidePanel(potassiumExplanation);

        // Fetch the current active metal from the static memory bridge
        string currentMetal = AlkaliManager.LastScannedMetal;

        // Activate only the corresponding panel
        if (currentMetal == "Lithium") ShowPanel(lithiumExplanation);
        else if (currentMetal == "Sodium") ShowPanel(sodiumExplanation);
        else if (currentMetal == "Potassium") ShowPanel(potassiumExplanation);
        else
        {
            Debug.LogWarning("No scanned metal recognized in the explanation scene!");
        }
    }

    private void ShowPanel(ExplanationUIConfig config)
    {
        if (config.explanationPanel != null) config.explanationPanel.SetActive(true);
    }

    private void HidePanel(ExplanationUIConfig config)
    {
        if (config.explanationPanel != null) config.explanationPanel.SetActive(false);
    }

    // Button function to return back to the main menu or scanning scene
    public void OnClickBackToScan()
    {
        SceneManager.LoadSceneAsync(1); // Return to AR Scan scene
    }
}
