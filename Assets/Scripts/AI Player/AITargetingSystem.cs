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

        // ��ó ���� �˻�
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
            // �ڽ��� ������ AI�� �˻�
            else if (collider.gameObject != aiTransform.gameObject && collider.CompareTag("AI"))
            {
                if (collider.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayer))
                {
                    aiPlayers.Add(aiPlayer);
                }
            }
        }

        // HP�� ���� ���� �÷��̾ Ÿ������ ����
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

        // �˻��� Ÿ���� ���� ���. 
        if (players.Count == 0 && aiPlayers.Count == 0)
        {
            resault = null;
        }

        return resault;
    }
}
