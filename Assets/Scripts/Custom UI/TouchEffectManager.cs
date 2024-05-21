using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TouchEffectManager : MonoBehaviour
{
    [SerializeField] private GameObject uiPrefab;  // Image ������
    [SerializeField] private Canvas canvas;  // Canvas ������Ʈ

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
            // ���콺 Ŭ�� ��ġ�� ȭ�� ��ǥ�� ������
            Vector3 screenPosition = Input.mousePosition;

            // ȭ�� ��ǥ�� ���� ��ǥ�� ��ȯ (ĵ������ ���� ��忡 ����)
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvas.transform as RectTransform, screenPosition, canvas.worldCamera, out Vector3 worldPosition);

            // UI ������Ʈ ���� �� ��ġ
            //GameObject newUIObject = Instantiate(uiPrefab, canvas.transform);
            GameObject newUIObject = GetObject();
            if (newUIObject == null) return;

            RectTransform rectTransform = newUIObject.GetComponent<RectTransform>();
            rectTransform.position = worldPosition;
            rectTransform.localScale = Vector3.zero;  // ó���� ũ�⸦ 0���� ����

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
        // ������Ʈ �ı�
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
