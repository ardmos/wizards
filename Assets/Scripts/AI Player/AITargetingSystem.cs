using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AITargetingSystem
{
    private float maxDetectionDistance;
    private Transform aiTransform;

    public AITargetingSystem(float maxDetectionDistance, Transform aiTransform)
    {
        this.maxDetectionDistance = maxDetectionDistance;
        this.aiTransform = aiTransform;
    }

    public GameObject DetectTarget()
    {
        GameObject resault = null;

        // 근처 범위 검색
        Collider[] colliders = Physics.OverlapSphere(aiTransform.position, maxDetectionDistance);
        List<PlayerServer> players = new List<PlayerServer>();
        List<WizardRukeAIServer> aiPlayers = new List<WizardRukeAIServer>();

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                if (collider.TryGetComponent<PlayerServer>(out PlayerServer player))
                {
                    players.Add(player);
                }
            }
            // 자신을 제외한 AI를 검색
            else if (collider.gameObject != aiTransform.gameObject && collider.CompareTag("AI"))
            {
                if (collider.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayer))
                {
                    aiPlayers.Add(aiPlayer);
                }
            }
        }

        // HP가 가장 낮은 플레이어를 타겟으로 설정
        if (players.Count > 0)
        {
            List<PlayerServer> sortedPlayers = players.OrderBy(player => player.GetPlayerHP()).ToList();
            resault = sortedPlayers[0].gameObject;
        }
        if (aiPlayers.Count > 0)
        {
            List<WizardRukeAIServer> sortedAIPlayer = aiPlayers.OrderBy(aiPlayer => aiPlayer.GetPlayerHP()).ToList();
            resault = sortedAIPlayer[0].gameObject;
        }

        // 검색된 타겟이 없는 경우. 
        if (players.Count == 0 && aiPlayers.Count == 0)
        {
            resault = null;
        }

        return resault;
    }
}
