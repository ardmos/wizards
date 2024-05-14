using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    [Header("Mesh Related")]
    public float activeTime = 0.1f;
    public float meshRefreshRate = 0.1f;
    public float meshDestroyDelay = 0.1f;
    public Transform positionToSpawn;

    [Header("Shader Related")]
    public Material material;
    public string shaderVarRef;
    public float shaderVarRate = 0.1f;
    public float shaderVarRefreshRate = 0.05f;

    [SerializeField] private bool activateTrail;
    [SerializeField] private bool isTrailActive;
    [SerializeField] private SkinnedMeshRenderer[] skinnedMeshRenderers;

    // ������Ʈ Ǯ��
    [SerializeField] private ObjectPool objectPool;

    // Update is called once per frame
    void Update()
    {
        if (activateTrail && !isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));
        }
    }

    public void ActivateTrail()
    {
        activateTrail = true;
    }

    private IEnumerator ActivateTrail(float timeActive)
    {
        // Debug.Log("Start");
        while (timeActive > 0)
        {
            timeActive -= meshRefreshRate;
            //Debug.Log($"timeActive: {timeActive}");

            for (int i=0; i<skinnedMeshRenderers.Length; i++)
            {
                // ������Ʈ Ǯ�� ���
                GameObject gameObject = objectPool.GetObject();
                //GameObject gameObject = new GameObject();
                if (gameObject == null)
                {
                    Debug.LogWarning("Failed to get object from object pool. Returning.");
                    yield break;
                }
                // ��ġ�� ȸ�� ����
                gameObject.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);


                MeshRenderer meshRenderer;
                MeshFilter meshFilter;
                if (!gameObject.GetComponent<MeshRenderer>() || !gameObject.GetComponent<MeshFilter>())
                {
                    // Mesh ����
                    meshRenderer = gameObject.AddComponent<MeshRenderer>();
                    meshFilter = gameObject.AddComponent<MeshFilter>();

                    Mesh mesh = new Mesh();
                    skinnedMeshRenderers[i].BakeMesh(mesh, meshFilter);
                    meshFilter.mesh = mesh;
                    meshRenderer.material = material;
                }
                else
                {
                    // �̹� ������Ʈ�� �ִ� ��� �����ϴ� ������Ʈ�� �������⸸ �Ѵ�
                    meshRenderer = gameObject.GetComponent<MeshRenderer>();
                }

                // Material �� �ִϸ��̼� ����
                StartCoroutine(AnimateMaterialFloat(meshRenderer.material, 0, shaderVarRate, shaderVarRefreshRate));

                // �����ð� ���� ������ƮǮ�� ������Ʈ ��ȯ
                // ��ȯ ���� alpha�� ���󺹱�
                meshRenderer.material.SetFloat(shaderVarRef, 1f);
                StartCoroutine(ReturnObjectDelayed(gameObject, meshDestroyDelay));
                //Destroy(gameObject, meshDestroyDelay);
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        isTrailActive = false;
        activateTrail = false;
        //Debug.Log("End");
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

    private IEnumerator ReturnObjectDelayed(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        objectPool.ReturnObject(obj);
    }

}
