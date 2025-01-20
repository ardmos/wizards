using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// AI의 타겟팅 시스템을 관리하는 클래스입니다.
/// </summary>
public class AITargetingSystem
{
    #region Constants & Fields
    // 최대 감지 가능한 콜라이더 수
    private const byte MAX_DETECTION_COUNT = 10;

    // 최대 감지 거리
    private float maxDetectionDistance;
    // AI의 Transform 컴포넌트
    private Transform aiTransform;
    // AI 플레이어 자신의 ITargetable 인터페이스
    private ITargetable aiPlayer;
    // 감지된 콜라이더를 저장할 배열
    private Collider[] detectionResults;
    // 캐시된 타겟들을 저장할 리스트
    private List<ITargetable> cachedTargets;
    #endregion

    #region Constructor
    /// <summary>
    /// AITargetingSystem의 생성자입니다.
    /// </summary>
    /// <param name="maxDetectionDistance">최대 감지 거리</param>
    /// <param name="aiTransform">AI의 Transform 컴포넌트</param>
    /// <param name="aiPlayer">AI 플레이어 자신의 ITargetable 인터페이스</param>
    public AITargetingSystem(float maxDetectionDistance, Transform aiTransform, ITargetable aiPlayer)
    {
        this.maxDetectionDistance = maxDetectionDistance;
        this.aiTransform = aiTransform;
        this.aiPlayer = aiPlayer;
        detectionResults = new Collider[MAX_DETECTION_COUNT];
        cachedTargets = new List<ITargetable>();
    }
    #endregion

    #region Target Detection
    /// <summary>
    /// 주변에서 타겟을 감지하고 HP가 가장 낮은 타겟을 반환합니다.
    /// </summary>
    /// <typeparam name="T">ITargetable을 구현한 타입</typeparam>
    /// <returns>감지된 타겟 중 HP가 가장 낮은 타겟의 GameObject, 없으면 null</returns>
    public GameObject DetectTarget<T>() where T : ITargetable
    {
        try
        {
            var targets = GetNearbyTargets<T>();
 /*           Logger.Log($"targets.Count(): {targets.Count()}");
            foreach (var target in targets)
            {
                Logger.Log($"target: {target}");
            }*/
           
            return targets.Count() > 0 ? GetLowestHPTarget(targets) : null;
        }
        catch (Exception e)
        {
            Logger.LogError($"{nameof(DetectTarget)} 에서 에러가 발생했습니다 : {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 근처의 타겟들을 반환합니다.
    /// 자주 호출되는 메서드라 성능 최적화를 위해 LINQ대신 직접 순회 방식으로 메서드를 구현했습니다.
    /// </summary>
    /// <typeparam name="T">ITargetable을 구현한 타입</typeparam>
    /// <returns>근처에서 감지된 타겟들의 컬렉션</returns>
    private IEnumerable<T> GetNearbyTargets<T>() where T : ITargetable
    {
        cachedTargets.Clear();

        int aiLayer = 1 << LayerMask.NameToLayer("AI");
        int playerLayer = 1 << LayerMask.NameToLayer("Player");
        int layerMask = aiLayer | playerLayer; // AI와 Player 레이어 모두 검출
        int hitCount = Physics.OverlapSphereNonAlloc(aiTransform.position, maxDetectionDistance, detectionResults, layerMask);

        if (hitCount > detectionResults.Length)
            Logger.LogWarning($"검출된 콜라이더 수({hitCount})가 검색 결과 배열 크기({detectionResults.Length})를 초과했습니다. 일부 검색 결과가 무시됩니다.");

        for (int i = 0; i < hitCount; i++)
        {
            if (detectionResults[i].TryGetComponent<T>(out T target))
            {
                if (ReferenceEquals(target, aiPlayer)) continue;
                cachedTargets.Add(target);
            }
        }
        return (IEnumerable<T>)cachedTargets;
    }

    /// <summary>
    /// HP가 가장 낮은 타겟을 반환합니다.
    /// 자주 호출되는 메서드라 성능 최적화를 위해 LINQ대신 직접 순회 방식으로 메서드를 구현했습니다.
    /// </summary>
    /// <typeparam name="T">ITargetable을 구현한 타입</typeparam>
    /// <param name="targets">타겟들의 컬렉션</param>
    /// <returns>HP가 가장 낮은 타겟의 GameObject</returns>
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
    #endregion
}