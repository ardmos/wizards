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
        // 스스로 게임오버 당한 경우, 게임 내 모든 플레이어들에게 점수를 줍니다. 
        if (clientWhoAttacked == OwnerClientId)
        {
            foreach (PlayerInGameData playerInGameData in CurrentPlayerDataManager.Instance.GetCurrentPlayers())
            {
                CurrentPlayerDataManager.Instance.AddPlayerScore(playerInGameData.clientId, DEFAULT_SCORE);
            }
        }
        // 일반적인 경우 상대 플레이어 DEFAULT_SCORE(300)스코어 획득
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
