using UnityEngine;
using UnityEngine.InputSystem;

public class CleaningManager : MonoBehaviour
{
    /// <summary>
    /// The material (with associated ShaderGraph) of the object to be cleaned.
    /// </summary>
    public Material objectMaterial;

    /// <summary>
    /// The ratio of how much dirt is on the object. 1 - completely dirty, 0 - completely clean.
    /// </summary>
    [Range(0, 1)]
    public float currentDirt = 1.0f;

    /// <summary>
    /// The speed of the cleaning process.
    /// </summary>
    public float cleaningSpeed = 0.5f;

    private Animator objectAnimator;
    private bool wasTurned = false;

    private void Start()
    {
        objectMaterial.SetFloat("_DirtIntensity", Mathf.Sqrt(currentDirt));
        objectAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                if (hitInfo.collider.gameObject == this.gameObject)
                {
                    DecreaseDirt();
                }
            }
        }
    }

    /// <summary>
    /// Decrease the amount of dirt from object material. Reference _DirtIntensity is float value in interval 0 - 1 in ShaderGraph.
    /// </summary>
    void DecreaseDirt()
    {
        if (currentDirt > 0)
        {
            if ((!wasTurned) && (currentDirt < 0.5))
            {
                wasTurned = true;
                objectAnimator.SetTrigger("doTurn");
            }
            currentDirt -= Time.deltaTime * cleaningSpeed;
            currentDirt = Mathf.Clamp01(currentDirt);

            objectMaterial.SetFloat("_DirtIntensity", Mathf.Sqrt(currentDirt));

            Debug.Log("Cistim!");
        }
        else
        {
            // TRIGGER SUCCESS ANIMÁCIE
            Debug.Log("VYCISTENE!");
        }
    }
}