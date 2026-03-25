using UnityEngine;
using UnityEngine.InputSystem;

public class WaterManager : MonoBehaviour
{
    public Transform hosepipeTip;
    public LineRenderer waterStream;
    public ParticleSystem splashParticles;
    public int segmentCount = 12; // Koæko bodov m· maù obl˙k
    public float waterVelocity = 5f; // R˝chlosù vody z hadice
    public float streamDistance = 1.4f;
    private Material waterMaterial;
    private float currentOffset;

    void Update()
    {
        if (Pointer.current.press.isPressed)
        {
            UpdateHoseStream();
        }
        else
        {
            waterStream.positionCount = 0;
            splashParticles.Stop();
        }
    }

    void StopWater()
    {
        waterStream.enabled = false;
        //splashParticles.Stop();
    }

    void UpdateHoseStream()
    {
        // 1. ZistÌme, kam mieri hr·Ë prstom (Raycast z kamery)
        Ray ray = Camera.main.ScreenPointToRay(Pointer.current.position.ReadValue());
        Vector3 targetPos;

        if (Physics.Raycast(ray, out RaycastHit cameraHit))
        {
            targetPos = cameraHit.point; // Bod na modely, kam chceme striekaù
        }
        else
        {
            targetPos = ray.origin + ray.direction * streamDistance; // Ak nemieri na niË, striekaj do diaæky
        }

        targetPos.y += 0.3f;

        Vector3 direction = targetPos - hosepipeTip.position;
        hosepipeTip.rotation = Quaternion.LookRotation(direction, Vector3.up);
        hosepipeTip.Rotate(0, -90, 0);

        splashParticles.transform.position = targetPos;
        splashParticles.transform.forward = -hosepipeTip.forward;
        

        // 2. VypoËÌtame smer od trysky k cieæu
        Vector3 startPos = waterStream.transform.position;
        Vector3 dir = (targetPos - startPos).normalized;

        // 3. DosadÌme do fyziky (zv˝ö velocity, ak je pr˙d prÌliö "zv‰dnut˝")
        Vector3 startVelocity = dir * waterVelocity;

        currentOffset -= Time.deltaTime * waterVelocity;
        waterStream.material.mainTextureOffset = new Vector2(currentOffset, 0);

        bool hasHitSomething = false;
        Vector3 hitPoint = targetPos;
        Vector3 hitPointNormal = Vector3.up;

        waterStream.positionCount = segmentCount;
        for (int i = 0; i < segmentCount; i++)
        {
            float t = i * 0.04f; // »asov˝ krok
            Vector3 p = startPos + startVelocity * t + 0.5f * Physics.gravity * t * t;
            waterStream.SetPosition(i, p);

            // Detekcia kolÌzie pre um˝vanie (Linecast medzi segmentmi)
            if (i > 0)
            {
                if (Physics.Linecast(waterStream.GetPosition(i - 1), p, out RaycastHit washHit))
                {
                    waterStream.positionCount = i + 1;
                    waterStream.SetPosition(i, washHit.point);

                    hasHitSomething = true;
                    hitPoint = washHit.point;
                    hitPointNormal = washHit.normal;

                    // TU prebieha samotnÈ Ëistenie (v mieste dopadu vody)
                    break;
                }
            }
        }

        if (hasHitSomething)
        {
            // Presunieme Ëastice presne tam, kde Linecast narazil
            splashParticles.transform.position = hitPoint;

            // OtoËÌme ich "od povrchu" (voliteænÈ, mÙûeö nechaù aj pÙvodnÈ)
            // splashParticles.transform.forward = washHit.normal; 

            splashParticles.transform.forward = hitPointNormal;

            if (!splashParticles.isPlaying) splashParticles.Play();
        }
        else
        {
            // Ak cel˝ pr˙d vody preletel bez kolÌzie, zastavÌme Ëastice
            if (splashParticles.isPlaying) splashParticles.Stop();
        }
    }

}
