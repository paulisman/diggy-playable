using UnityEngine;
using UnityEngine.InputSystem;

public class WaterManager : MonoBehaviour
{
    public Transform hosepipeTip;
    public LineRenderer waterStream;
    public ParticleSystem splashParticles;
    public AudioSource waterAudioSource;
    public int waterSegments = 12;
    public float waterVelocity = 5f;
    public float streamDistance = 1.4f;
    public float verticalCalibration = 0.3f;

    private float currentMaterialOffset;

    void Update()
    {
        if (Pointer.current.press.isPressed)
        {
            UpdateWaterStream();
            if (!waterAudioSource.isPlaying)
            {
                waterAudioSource.Play();
            }
        }
        else
        {
            waterStream.positionCount = 0;
            splashParticles.Stop();

            if (waterAudioSource.isPlaying)
            {
                waterAudioSource.Stop();
            }
        }
    }

    void UpdateWaterStream()
    {
        Ray ray = Camera.main.ScreenPointToRay(Pointer.current.position.ReadValue());
        Vector3 targetPos;

        if (Physics.Raycast(ray, out RaycastHit cameraHit))
        {
            targetPos = cameraHit.point;
        }
        else
        {
            targetPos = ray.origin + ray.direction * streamDistance;
        }

        targetPos.y += verticalCalibration;

        Vector3 direction = targetPos - hosepipeTip.position;
        hosepipeTip.rotation = Quaternion.LookRotation(direction, Vector3.up);
        hosepipeTip.Rotate(0, -90, 0);

        splashParticles.transform.position = targetPos;
        splashParticles.transform.forward = -hosepipeTip.forward;
        
        Vector3 startPos = waterStream.transform.position;
        Vector3 dir = (targetPos - startPos).normalized;
        Vector3 startVelocity = dir * waterVelocity;

        currentMaterialOffset -= Time.deltaTime * waterVelocity;
        waterStream.material.mainTextureOffset = new Vector2(currentMaterialOffset, 0);

        bool hasHitSomething = false;
        Vector3 hitPoint = targetPos;
        Vector3 hitPointNormal = Vector3.up;

        waterStream.positionCount = waterSegments;
        for (int i = 0; i < waterSegments; i++)
        {
            float t = i * 0.04f;
            Vector3 p = startPos + startVelocity * t + 0.5f * Physics.gravity * t * t;
            waterStream.SetPosition(i, p);

            if (i > 0)
            {
                if (Physics.Linecast(waterStream.GetPosition(i - 1), p, out RaycastHit washHit))
                {
                    waterStream.positionCount = i + 1;
                    waterStream.SetPosition(i, washHit.point);

                    hasHitSomething = true;
                    hitPoint = washHit.point;
                    hitPointNormal = washHit.normal;

                    break;
                }
            }
        }

        if (hasHitSomething)
        {
            splashParticles.transform.position = hitPoint;

            splashParticles.transform.forward = hitPointNormal;

            if (!splashParticles.isPlaying) splashParticles.Play();
        }
        else
        {
            if (splashParticles.isPlaying) splashParticles.Stop();
        }
    }
}
