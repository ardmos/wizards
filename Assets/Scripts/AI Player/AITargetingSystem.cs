using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AITargetingSystem
{
    private const byte MAX_DETECTION_COUNT = 10;

    private float maxDetectionDistance;
    private Transform aiTransform;
    private Collider[] detectionResults;
    private List<ITargetable> cachedTargets;

    public AITargetingSystem(float maxDetectionDistance, Transform aiTransform)
    {
        this.maxDetectionDistance = maxDetectionDistance;
        this.aiTransform = aiTransform;
        detectionResults = new Collider[MAX_DETECTION_COUNT];
        cachedTargets = new List<ITargetable>();
    }

    public GameObject DetectTarget<T>() where T : ITargetable
    {
        try
        {
            var targets = GetNearbyTargets<T>();
            return targets.Count() > 0 ? GetLowestHPTarget(targets) : null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in DetectTarget: {e.Message}");
            return null;
        }
    }

    private IEnumerable<T> GetNearbyTargets<T>() where T : ITargetable
    {
        cachedTargets.Clear();
        // 근처 범위 탐색
        int hitCount = Physics.OverlapSphereNonAlloc(aiTransform.position, maxDetectionDistance, detectionResults);

        if (hitCount > detectionResults.Length)
        {
            Debug.LogWarning($"검출된 콜라이더 수({hitCount})가 배열 크기({detectionResults.Length})를 초과했습니다. 일부 결과가 무시됩니다.");
        }

        for (int i = 0; i < hitCount; i++)
        {
            var target = detectionResults[i].GetComponent<T>();
            // 자신을 제외한 AI를 검색
            if (target != null && !ReferenceEquals(target, this))
            {
                cachedTargets.Add(target);
            }
        }

        return (IEnumerable<T>)cachedTargets;
    }

    private GameObject GetLowestHPTarget<T>(IEnumerable<T> targets) where T : ITargetable
    {
        return targets
            .OrderBy(target => target.GetHP())
            .FirstOrDefault()
            ?.GetGameObject();
    }

    public GameObject DetectTarget()
    {
        GameObject result = null;

        // 근처 범위 탐색
        Collider[] colliders = Physics.OverlapSphere(aiTransform.position, maxDetectionDistance);
        List<PlayerServer> players = new List<PlayerServer>();
        List<WizardRukeAIServer> aiPlayers = new List<WizardRukeAIServer>();

        foreach (var collider in colliders)
        {
            if (collider.CompareTag(Tags.Player))
            {
                if (collider.TryGetComponent<PlayerServer>(out PlayerServer player))
                {
                    players.Add(player);
                }
            }
            // 자신을 제외한 AI를 검색
            else if (collider.gameObject != aiTransform.gameObject && collider.CompareTag(Tags.AI))
            {
                if (collider.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayer))
                {
                    aiPlayers.Add(aiPlayer);
                }
            }
        }

        // 검색된 타겟이 없는 경우. 
        if (players.Count == 0 && aiPlayers.Count == 0)
        {
            return null;
        }

        // HP가 가장 낮은 플레이어를 타겟으로 설정
        if (players.Count > 0)
        {
            List<PlayerServer> sortedPlayers = players.OrderBy(player => player.GetPlayerHP()).ToList();
            result = sortedPlayers[0].gameObject;
        }
        if (aiPlayers.Count > 0)
        {
            List<WizardRukeAIServer> sortedAIPlayer = aiPlayers.OrderBy(aiPlayer => aiPlayer.GetHP()).ToList();
            result = sortedAIPlayer[0].gameObject;
        }

        return result;
    }
}
