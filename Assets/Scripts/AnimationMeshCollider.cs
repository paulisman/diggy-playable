using UnityEngine;

public class AnimationMeshCollider : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public MeshCollider meshCollider;
    private Mesh bakedMesh;

    void Start()
    {
        bakedMesh = new Mesh();
    }

    void Update()
    {
        if (skinnedMeshRenderer != null && meshCollider != null)
        {
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            meshCollider.sharedMesh = bakedMesh;
        }
    }
}
