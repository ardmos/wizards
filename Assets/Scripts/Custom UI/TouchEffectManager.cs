using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TouchEffectManager : MonoBehaviour
{
    [SerializeField] private GameObject uiPrefab;  // Image 프리팹
    [SerializeField] private Canvas canvas;  // Canvas 오브젝트

    public int poolSize;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Start()
    {
        InitializePool();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 마우스 클릭 위치를 화면 좌표로 가져옴
            Vector3 screenPosition = Input.mousePosition;

            // 화면 좌표를 월드 좌표로 변환 (캔버스의 렌더 모드에 따라)
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvas.transform as RectTransform, screenPosition, canvas.worldCamera, out Vector3 worldPosition);

            // UI 오브젝트 생성 및 배치
            //GameObject newUIObject = Instantiate(uiPrefab, canvas.transform);
            GameObject newUIObject = GetObject();
            if (newUIObject == null) return;

            RectTransform rectTransform = newUIObject.GetComponent<RectTransform>();
            rectTransform.position = worldPosition;
            rectTransform.localScale = Vector3.zero;  // 처음에 크기를 0으로 설정

            StartCoroutine(ScaleAndDestroyCoroutine(newUIObject));
        }
    }

    private IEnumerator ScaleAndDestroyCoroutine(GameObject obj)
    {
        //Debug.Log("ScaleAndDestroyCoroutine");
        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            //Debug.Log(elapsedTime);
            float scale = Mathf.Clamp01(elapsedTime / duration);
            rectTransform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
        while (elapsedTime > 0)
        {
            elapsedTime -= Time.deltaTime;
            //Debug.Log(elapsedTime);
            float scale = Mathf.Clamp01(elapsedTime / duration);
            rectTransform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
        // 오브젝트 파괴
        ReturnObject(obj);
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(uiPrefab, canvas.transform);
            obj.SetActive(false);
            //obj.transform.parent = canvas.transform;
            //obj.AddComponent<RectTransform>();
            pool.Enqueue(obj);
        }
    }

    public GameObject GetObject()
    {
        if (pool.Count == 0)
        {
            Debug.LogWarning("Pool is empty. Returning null.");
            return null;
        }

        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        //Debug.Log($"GetObject()! pool count:{pool.Count}");
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
        //Debug.Log($"ReturnObject()! pool count:{pool.Count}");
    }
}
