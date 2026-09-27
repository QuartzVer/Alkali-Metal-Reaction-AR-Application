using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragDropable : MonoBehaviour
{
    [Header("Chemical Properties")]
    [SerializeField] private string metalIdentity; // Set to Lithium, Sodium, Potassium, or Water

    [Header("Visual & Audio FX Assets")]
    [SerializeField] private ParticleSystem continuousReactionParticles; // Looping fizz/smoke for Li/Na
    [SerializeField] private ParticleSystem instantExplosionParticles;  // Big burst for K or Na end-pop
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip reactionSound; // Fizz or Hiss
    [SerializeField] private AudioClip explosionSound; // Pop or Explosion

    [Header("Input Action Configurations")]
    [SerializeField] private InputAction press;
    [SerializeField] private InputAction screenPos;

    private Vector2 curScreenPos;
    private Camera mainCam;
    private Rigidbody myRigidbody;
    private bool isDragging;
    private float initialZDistance;
    private bool hasReacted = false; // Prevents double-triggering

    // Public getter so other objects can verify identity elements
    public string Identity => metalIdentity;

    private Vector3 WorldPos
    {
        get
        {
            Vector3 screenPoint3D = new Vector3(curScreenPos.x, curScreenPos.y, initialZDistance);
            return mainCam.ScreenToWorldPoint(screenPoint3D);
        }
    }

    private bool IsClickedOn
    {
        get
        {
            Ray ray = mainCam.ScreenPointToRay(curScreenPos);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.transform == transform && gameObject.activeInHierarchy;
            }
            return false;
        }
    }

    private void Awake()
    {
        mainCam = Camera.main;
        myRigidbody = GetComponent<Rigidbody>();

        screenPos.Enable();
        press.Enable();

        screenPos.performed += context => curScreenPos = context.ReadValue<Vector2>();
        press.performed += _ => { if (!isDragging && IsClickedOn && !hasReacted) StartCoroutine(DragRoutine()); };
        press.canceled += _ => isDragging = false;
    }

    private void Start()
    {
        if (metalIdentity == "Water") return;

        // Check cross-scene static memory records
        if (AlkaliManager.LastScannedMetal != metalIdentity)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    private void OnDestroy()
    {
        screenPos.Disable();
        press.Disable();
    }

    private IEnumerator DragRoutine()
    {
        isDragging = true;

        initialZDistance = mainCam.WorldToScreenPoint(transform.position).z;
        Vector3 offset = transform.position - WorldPos;

        // Keep kinematic true so gravity doesn't pull it down while dragging
        if (myRigidbody != null) myRigidbody.isKinematic = true;

        while (isDragging)
        {
            // This forces Unity's physics engine and child objects to stay perfectly synced while dragging
            if (myRigidbody != null)
            {
                myRigidbody.MovePosition(WorldPos + offset);
            }
            else
            {
                transform.position = WorldPos + offset;
            }

            yield return null;
        }

        if (myRigidbody != null) myRigidbody.isKinematic = true;
    }



    private void OnTriggerEnter(Collider other)
    {
        if (hasReacted) return;

        DragDropable touchedObject = other.GetComponent<DragDropable>();
        if (touchedObject == null || touchedObject.hasReacted) return;

        // Verify if a Metal object and a Water object are meeting
        bool isMetalAndWater = (this.metalIdentity == "Water" && touchedObject.Identity != "Water") ||
                               (this.metalIdentity != "Water" && touchedObject.Identity == "Water");

        if (isMetalAndWater)
        {
            // Lock out further interactions instantly
            this.isDragging = false;
            touchedObject.isDragging = false;
            this.hasReacted = true;
            touchedObject.hasReacted = true;

            ExecuteContactReaction(this, touchedObject);
        }
    }

    private void ExecuteContactReaction(DragDropable first, DragDropable second)
    {
        Debug.Log("Mutual contact detected! Initiating reaction cleanup sequence.");

        DragDropable metalObj = (first.Identity == "Water") ? second : first;
        DragDropable waterObj = (first.Identity == "Water") ? first : second;

        // Clean up water instantly
        Destroy(waterObj.gameObject);

        // Hand off reaction sequence control directly to the metal object's local script logic
        metalObj.StartCoroutine(metalObj.RunChemicalSimulationSequence());
    }

    private IEnumerator RunChemicalSimulationSequence()
    {
        Vector3 originalScale = transform.localScale;

        if (metalIdentity == "Lithium")
        {
            // LITHIUM: Fizzes gently, completely dissolves into the water over time
            if (continuousReactionParticles != null) continuousReactionParticles.Play();
            if (audioSource != null && reactionSound != null) audioSource.PlayOneShot(reactionSound);

            float duration = 4.0f;
            float elapsed = 0f;

            // CHANGED: Target scale is now zero instead of 0.7f
            Vector3 targetScale = Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float pct = elapsed / duration;

                // Smoothly dissolve the metal chunk size down to zero
                transform.localScale = Vector3.Lerp(originalScale, targetScale, pct);

                yield return null;
            }

            if (continuousReactionParticles != null) continuousReactionParticles.Stop();

            // ADDED: Make sure it's fully invisible and destroyed
            if (GetComponent<Renderer>() != null) GetComponent<Renderer>().enabled = false;
            Destroy(gameObject);
        }

        else if (metalIdentity == "Sodium")
        {
            // Moderate reaction speed
            if (continuousReactionParticles != null) continuousReactionParticles.Play();
            if (audioSource != null && reactionSound != null) audioSource.PlayOneShot(reactionSound);

            float duration = 2.5f; // Lasts longer, fizzles around
            float elapsed = 0f;
            Vector3 startPos = transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float pct = elapsed / duration;

                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, pct);

                // Moderate zig-zag movement
                Vector3 dashOffset = new Vector3(Mathf.Sin(Time.time * 20f) * 4.0f, Mathf.Cos(Time.time * 15f) * 1.5f, 0);
                transform.position = startPos + dashOffset;

                yield return null;
            }

            if (continuousReactionParticles != null) continuousReactionParticles.Stop();
            if (GetComponent<Renderer>() != null) GetComponent<Renderer>().enabled = false;

            if (instantExplosionParticles != null) instantExplosionParticles.Play();
            if (audioSource != null && explosionSound != null) audioSource.PlayOneShot(explosionSound);

            yield return new WaitForSeconds(1.5f);
            Destroy(gameObject);
        }
        else if (metalIdentity == "Potassium")
        {
      
            if (instantExplosionParticles != null) instantExplosionParticles.Play(); // Purple flames engage
            if (audioSource != null && reactionSound != null) audioSource.PlayOneShot(reactionSound);

            float duration = 1.0f; // it burns out and detonates TWICE as fast as sodium!
            float elapsed = 0f;
            Vector3 startPos = transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float pct = elapsed / duration;

                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, pct);

                Vector3 fireMoveOffset = new Vector3(Mathf.Sin(Time.time * 25f) * 3.5f, Mathf.Cos(Time.time * 18f) * 1.5f, 0);
                transform.position = startPos + fireMoveOffset;

                yield return null;
            }

            if (instantExplosionParticles != null) instantExplosionParticles.Stop();
            if (GetComponent<Renderer>() != null) GetComponent<Renderer>().enabled = false;

            if (audioSource != null && explosionSound != null) audioSource.PlayOneShot(explosionSound);

            yield return new WaitForSeconds(1.5f);
            Destroy(gameObject);
        }


    }

    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
    }
}
