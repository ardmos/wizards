using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// AI�� Ÿ���� �ý����� �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class AITargetingSystem
{
    // �ִ� ���� ������ �ݶ��̴� ��
    private const byte MAX_DETECTION_COUNT = 10;

    // �ִ� ���� �Ÿ�
    private float maxDetectionDistance;
    // AI�� Transform ������Ʈ
    private Transform aiTransform;
    // AI �÷��̾� �ڽ��� ITargetable �������̽�
    private ITargetable aiPlayer;
    // ������ �ݶ��̴��� ������ �迭
    private Collider[] detectionResults;
    // ĳ�õ� Ÿ�ٵ��� ������ ����Ʈ
    private List<ITargetable> cachedTargets;

    /// <summary>
    /// AITargetingSystem�� �������Դϴ�.
    /// </summary>
    /// <param name="maxDetectionDistance">�ִ� ���� �Ÿ�</param>
    /// <param name="aiTransform">AI�� Transform ������Ʈ</param>
    /// <param name="aiPlayer">AI �÷��̾� �ڽ��� ITargetable �������̽�</param>
    public AITargetingSystem(float maxDetectionDistance, Transform aiTransform, ITargetable aiPlayer)
    {
        this.maxDetectionDistance = maxDetectionDistance;
        this.aiTransform = aiTransform;
        this.aiPlayer = aiPlayer;
        detectionResults = new Collider[MAX_DETECTION_COUNT];
        cachedTargets = new List<ITargetable>();
    }

    /// <summary>
    /// �ֺ����� Ÿ���� �����ϰ� HP�� ���� ���� Ÿ���� ��ȯ�մϴ�.
    /// </summary>
    /// <typeparam name="T">ITargetable�� ������ Ÿ��</typeparam>
    /// <returns>������ Ÿ�� �� HP�� ���� ���� Ÿ���� GameObject, ������ null</returns>
    public GameObject DetectTarget<T>() where T : ITargetable
    {
        try
        {
            var targets = GetNearbyTargets<T>();
            return targets.Count() > 0 ? GetLowestHPTarget(targets) : null;
        }
        catch (Exception e)
        {
            Logger.LogError($"{nameof(DetectTarget)} ���� ������ �߻��߽��ϴ� : {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// ��ó�� Ÿ�ٵ��� ��ȯ�մϴ�. 
    /// ���� ȣ��Ǵ� �޼���� ���� ����ȭ�� ���� LINQ��� ���� ��ȸ ������� �޼��带 �����߽��ϴ�.
    /// </summary>
    /// <typeparam name="T">ITargetable�� ������ Ÿ��</typeparam>
    /// <returns>��ó���� ������ Ÿ�ٵ��� �÷���</returns>
    private IEnumerable<T> GetNearbyTargets<T>() where T : ITargetable
    {
        cachedTargets.Clear();

        int hitCount = Physics.OverlapSphereNonAlloc(aiTransform.position, maxDetectionDistance, detectionResults);
        if (hitCount > detectionResults.Length) Logger.LogWarning($"����� �ݶ��̴� ��({hitCount})�� �˻� ��� �迭 ũ��({detectionResults.Length})�� �ʰ��߽��ϴ�. �Ϻ� �˻� ����� ���õ˴ϴ�.");

        for (int i = 0; i < hitCount; i++)
        {
            if(detectionResults[i].TryGetComponent<T>(out T target))
            {
                if (ReferenceEquals(target, aiPlayer)) continue;
                cachedTargets.Add(target);
            }
        }
        return (IEnumerable<T>)cachedTargets;
    }

    /// <summary>
    /// HP�� ���� ���� Ÿ���� ��ȯ�մϴ�. 
    /// ���� ȣ��Ǵ� �޼���� ���� ����ȭ�� ���� LINQ��� ���� ��ȸ ������� �޼��带 �����߽��ϴ�.
    /// </summary>
    /// <typeparam name="T">ITargetable�� ������ Ÿ��</typeparam>
    /// <param name="targets">Ÿ�ٵ��� �÷���</param>
    /// <returns>HP�� ���� ���� Ÿ���� GameObject</returns>
    private GameObject GetLowestHPTarget<T>(IEnumerable<T> targets) where T : ITargetable
    {
        ITargetable lowestHPTarget = cachedTargets[0];
        float lowestHP = lowestHPTarget.GetHP();

        for (int i = 1; i < cachedTargets.Count; i++)
        {
            float hp = cachedTargets[i].GetHP();
            if (hp < lowestHP)
            {
                lowestHP = hp;
                lowestHPTarget = cachedTargets[i];
            }
        }

        return lowestHPTarget.GetGameObject();
    }
}
