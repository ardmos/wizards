using Unity.Netcode;
using UnityEngine;

public class WizardRukeAIGameOverManagerServer : NetworkBehaviour
{
    private const int DEFAULT_SCORE = 300;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider aiCollider;
    [SerializeField] private WizardRukeAIItemDropManagerServer itemDropManager;
    private WizardRukeAIServer aiServer;
    private WizardRukeAIClient aiClient;

    public void InitAIGameOverManager(WizardRukeAIServer aiServer, WizardRukeAIClient aiClient)
    {
        this.aiServer = aiServer;
        this.aiClient = aiClient;
    }

    public void HandleGameOver(ulong clientWhoAttacked)
    {
        if (!ValidationCheck()) return;

        rb.isKinematic = true;
        aiCollider.enabled = false;
        tag = Tags.GameOver;

        aiServer.GetMovementManager().StopMove();
        aiClient.OffPlayerUIClientRPC();
        aiServer.GetPlayerAnimator().UpdatePlayerAnimationOnServer(PlayerMoveAnimState.GameOver);
        itemDropManager.DropItem(aiServer.transform.position);
        MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(aiServer.GetAIClientId(), clientWhoAttacked);

        GiveScore(clientWhoAttacked);
    }

    private void GiveScore(ulong clientWhoAttacked)
    {
        // ������ ���ӿ��� ���� ���, ���� �� ��� �÷��̾�鿡�� ������ �ݴϴ�. 
        if (clientWhoAttacked == OwnerClientId)
        {
            foreach (PlayerInGameData playerInGameData in CurrentPlayerDataManager.Instance.GetCurrentPlayers())
            {
                CurrentPlayerDataManager.Instance.AddPlayerScore(playerInGameData.clientId, DEFAULT_SCORE);
            }
        }
        // �Ϲ����� ��� ��� �÷��̾� DEFAULT_SCORE(300)���ھ� ȹ��
        else
        {
            CurrentPlayerDataManager.Instance.AddPlayerScore(clientWhoAttacked, DEFAULT_SCORE);
        }
    }

    private bool ValidationCheck()
    {
        bool checkResault = true;
        if (rb == null) checkResault = false;
        if (aiCollider == null) checkResault = false;
        if (aiServer == null) checkResault = false;
        if (aiClient == null) checkResault = false;
        if (aiServer.GetMovementManager() == null) checkResault = false;
        if (itemDropManager == null) checkResault = false;
        if (MultiplayerGameManager.Instance == null) checkResault = false;
        if (CurrentPlayerDataManager.Instance == null) checkResault = false;

        return checkResault;
    }
}
