using UnityEngine;
using UnityEngine.InputSystem;

public class CleaningManager : MonoBehaviour
{
    /// <summary>
    /// The material (with associated ShaderGraph) of the object to be cleaned.
    /// </summary>
    public Material objectMaterial;

    [Header("Global cleaning")]
    /// <summary>
    /// The ratio of how much global dirt is on the object. 1 - completely dirty, 0 - completely clean.
    /// </summary>
    [Range(0, 1)]
    public float currentDirt = 1.0f;

    /// <summary>
    /// The speed of the global cleaning process.
    /// </summary>
    [Range(0, 5)]
    public float cleaningSpeed = 0.5f;

    [Header("Brush washing")]
    /// <summary>
    /// The initial masking texture (e.g. all white) prepared to be brushed with black color.
    /// </summary>
    public Texture2D baseMask;

    /// <summary>
    /// The radius of the round brush for washing.
    /// </summary>
    [Range(1, 30)]
    public int brushSize = 10;

    private Texture2D runtimeMask;
    private Animator objectAnimator;
    private bool wasTurned = false;
    private Vector2? lastPixelUV = null;
    private float interpolationStepSize;

    private void Start()
    {
        objectAnimator = GetComponentInParent<Animator>();

        runtimeMask = new Texture2D(baseMask.width, baseMask.height);
        runtimeMask.SetPixels(baseMask.GetPixels());
        runtimeMask.Apply();

        interpolationStepSize = brushSize / runtimeMask.width * 0.5f;   //half a brush radius per drawing area is considered optimal for UV interpolation

        objectMaterial.SetFloat("_DirtIntensity", Mathf.Sqrt(currentDirt));
        objectMaterial.SetTexture("_MaskTexture", runtimeMask);
    }

    void Update()
    {
        if (!Pointer.current.press.isPressed)
        {
            lastPixelUV = null;
            return; //reset last pixel UV, because input is not pressed
        }
        
        Ray ray = Camera.main.ScreenPointToRay(Pointer.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hitInfo) || (hitInfo.collider.gameObject != this.gameObject))
        {
            lastPixelUV = null;
            return; //reset last pixel UV, because ray is out of the collider
        }

        DecreaseDirt();

        Vector2 pixelUV = hitInfo.textureCoord;

        if (lastPixelUV.HasValue)
        {
            float distInterpolation = Vector2.Distance(lastPixelUV.Value, pixelUV);
            int stepsInterpolation = Mathf.Max(1, Mathf.CeilToInt(distInterpolation / interpolationStepSize));  //prevent to be 0, because of dividing by zero later

            for (int i = 0; i <= stepsInterpolation; i++)
            {
                Vector2 pix = Vector2.Lerp(lastPixelUV.Value, pixelUV, (float)i / stepsInterpolation);

                Debug.Log("lastPixelUV = hasValue, Brush Lerp pixelUV " + pix);

                BrushWashing(pix);
            }
        }
        else
        {
            Debug.Log("lastPixelUV = noValue, Brush pixelUV = " + pixelUV);

            BrushWashing(pixelUV);
        }

        lastPixelUV = pixelUV;
        runtimeMask.Apply();    //optimize drawing into the masking texture just once per frame
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
            currentDirt -= Time.deltaTime * (cleaningSpeed / 10);
            currentDirt = Mathf.Clamp01(currentDirt);

            objectMaterial.SetFloat("_DirtIntensity", Mathf.Sqrt(currentDirt));
        }
        else
        {
            // TRIGGER SUCCESS ANIMÁCIE
            Debug.Log("VYCISTENE!");
        }
    }

    void BrushWashing(Vector2 pixelUV)
    {
        //multiply relative distance of hit UV coordinates of the texture with its dimensions
        pixelUV.x *= runtimeMask.width;
        pixelUV.y *= runtimeMask.height;

        //draw all the brush pixels on the masking texture
        for (int x = -brushSize; x < brushSize; x++)
        {
            for (int y = -brushSize; y < brushSize; y++)
            {
                if ((x * x) + (y * y) <= (brushSize * brushSize)) //make brush round, not square
                {
                    //clamp within masking texture boundaries
                    int drawX = Mathf.Clamp((int)pixelUV.x + x, 0, runtimeMask.width - 1);
                    int drawY = Mathf.Clamp((int)pixelUV.y + y, 0, runtimeMask.height - 1);
                    runtimeMask.SetPixel(drawX, drawY, Color.black);
                }
            }
        }
    }
}