using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    [Header("Mesh Related")]
    public float activeTime = 3f;
    public float meshRefreshRate = 0.1f;
    public float meshDestroyDelay = 3f;
    public Transform positionToSpawn;

    [Header("Shader Related")]
    public Material material;
    public string shaderVarRef;
    public float shaderVarRate = 0.1f;
    public float shaderVarRefreshRate = 0.05f;

    [SerializeField] private bool isTrailActive;
    [SerializeField] private SkinnedMeshRenderer[] skinnedMeshRenderers;

    // Update is called once per frame
    void Update()
    {
        if (!isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));
        }
    }

    private IEnumerator ActivateTrail(float timeActive)
    {
        while (timeActive > 0)
        {
            timeActive -= meshRefreshRate;

            for (int i=0; i<skinnedMeshRenderers.Length; i++)
            {
                // 추후 오브젝트 풀링으로 전환해야합니다.
                GameObject gameObject = new GameObject();
                gameObject.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer meshRenderer =  gameObject.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh, meshFilter);

                meshFilter.mesh = mesh;
                meshRenderer.material = material;

                StartCoroutine(AnimateMaterialFloat(meshRenderer.material, 0, shaderVarRate, shaderVarRefreshRate));

                Destroy(gameObject, meshDestroyDelay);
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        isTrailActive = false;
    }

    private IEnumerator AnimateMaterialFloat(Material material, float goal, float rate, float refreshRate)
    {
        float valueToAnimate = material.GetFloat(shaderVarRef);

        while (valueToAnimate > goal)
        {
            valueToAnimate -= rate;
            material.SetFloat(shaderVarRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }

}
