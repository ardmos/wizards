using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightBuzzMeshTrail : MonoBehaviour
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
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer_Body;
    [SerializeField] private MeshFilter[] meshFilters;

    // 오브젝트 풀링
    [SerializeField] private ObjectPool objectPool_Body;
    [SerializeField] private ObjectPool objectPool_Helmet;
    [SerializeField] private ObjectPool objectPool_Sword;
    [SerializeField] private ObjectPool objectPool_Shield;

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
        //Debug.Log("Start");
        while (timeActive > 0)
        {
            timeActive -= meshRefreshRate;
            //Debug.Log($"timeActive: {timeActive}");

            // 오브젝트 풀링 사용
            GameObject gameObject = objectPool_Body.GetObject();
            //GameObject gameObject = new GameObject();
            if (gameObject == null)
            {
                Debug.Log("Failed to get object from object pool. Returning.");
                yield break;
            }
            // 위치와 회전 설정
            gameObject.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);


            MeshRenderer meshRenderer;
            MeshFilter meshFilter;
            if (!gameObject.GetComponent<MeshRenderer>() || !gameObject.GetComponent<MeshFilter>())
            {
                // Mesh 생성
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshFilter = gameObject.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                skinnedMeshRenderer_Body.BakeMesh(mesh, meshFilter);
                meshFilter.mesh = mesh;
                meshRenderer.material = material;
            }
            else
            {
                // 이미 컴포넌트가 있는 경우 존재하는 컴포넌트를 가져오기만 한다
                meshRenderer = gameObject.GetComponent<MeshRenderer>();
            }

            // Material 및 애니메이션 설정
            StartCoroutine(AnimateMaterialFloat(meshRenderer.material, 0, shaderVarRate, shaderVarRefreshRate));

            // 일정시간 이후 오브젝트풀에 오브젝트 반환
            // 반환 전에 alpha값 원상복구
            meshRenderer.material.SetFloat(shaderVarRef, 1f);
            StartCoroutine(ReturnObjectDelayed_Body(gameObject, meshDestroyDelay));
            //Destroy(gameObject, meshDestroyDelay);

            // Body를 제외한 나머지 부분들 구현(SkinnedMeshRenderer가 아님)
            for (int i = 0; i < meshFilters.Length; i++)
            {
                // 오브젝트 풀링 사용
                GameObject gameObjectM = null;
                if (i == 0)
                {
                    gameObjectM = objectPool_Helmet.GetObject();
                }
                else if (i == 1)
                {
                    gameObjectM = objectPool_Sword.GetObject();
                }
                else if (i == 2)
                {
                    gameObjectM = objectPool_Shield.GetObject();
                }
                
                //GameObject gameObject = new GameObject();
                if (gameObjectM == null)
                {
                    Debug.Log("Failed to get object from object pool. Returning.");
                    yield break;
                }
                // 위치와 회전 설정
                gameObjectM.transform.SetPositionAndRotation(meshFilters[i].transform.position, meshFilters[i].transform.rotation);


                MeshRenderer meshRendererM;
                MeshFilter meshFilterM;
                if (!gameObjectM.GetComponent<MeshRenderer>() || !gameObjectM.GetComponent<MeshFilter>())
                {
                    // Mesh 생성
                    meshRendererM = gameObjectM.AddComponent<MeshRenderer>();
                    meshFilterM = gameObjectM.AddComponent<MeshFilter>();

                    meshFilterM.mesh = meshFilters[i].mesh;
                    meshRendererM.material = material;
                }
                else
                {
                    // 이미 컴포넌트가 있는 경우 존재하는 컴포넌트를 가져오기만 한다
                    meshRendererM = gameObjectM.GetComponent<MeshRenderer>();
                }

                // Material 및 애니메이션 설정
                StartCoroutine(AnimateMaterialFloat(meshRendererM.material, 0, shaderVarRate, shaderVarRefreshRate));

                // 일정시간 이후 오브젝트풀에 오브젝트 반환
                // 반환 전에 alpha값 원상복구
                meshRendererM.material.SetFloat(shaderVarRef, 1f);
                if (i == 0)
                {
                    StartCoroutine(ReturnObjectDelayed_Helmet(gameObjectM, meshDestroyDelay));
                }
                else if (i == 1)
                {

                    StartCoroutine(ReturnObjectDelayed_Sword(gameObjectM, meshDestroyDelay));                 
                }
                else if (i == 2)
                {

                    StartCoroutine(ReturnObjectDelayed_Shield(gameObjectM, meshDestroyDelay));
                }
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

    private IEnumerator ReturnObjectDelayed_Body(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        objectPool_Body.ReturnObject(obj);
    }

    private IEnumerator ReturnObjectDelayed_Helmet(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        objectPool_Helmet.ReturnObject(obj);
    }
    private IEnumerator ReturnObjectDelayed_Sword(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        objectPool_Sword.ReturnObject(obj);
    }
    private IEnumerator ReturnObjectDelayed_Shield(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        objectPool_Shield.ReturnObject(obj);
    }
}
