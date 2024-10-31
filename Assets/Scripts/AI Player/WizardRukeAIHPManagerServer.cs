using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class WizardRukeAIHPManagerServer : NetworkBehaviour
{
    PlayerInGameData playerData;
    public WizardRukeAIClient wizardRukeAIClient;
    public WizardRukeAIServer wizardRukeAIServer;
    public WizardRukeAIBattleSystemServer wizardRukeAIBattleSystemServer;
    public PlayerAnimator playerAnimator;

    public void InitPlayerHP(ICharacter character)
    {
        playerData = CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(wizardRukeAIServer.AIClientId);
        playerData.hp = character.hp;
        playerData.maxHp = character.maxHp;
        CurrentPlayerDataManager.Instance.SetPlayerDataByClientId(wizardRukeAIServer.AIClientId, playerData);

        wizardRukeAIClient.SetHPClientRPC(playerData.hp, playerData.maxHp);
    }

    /// <summary>
    /// 서버에서 동작합니다
    /// </summary>
    /// <param name="healingValue"></param>
    public void ApplyHeal(sbyte healingValue)
    {
        // 힐 적용하는중
        //playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(wizardRukeAIServer.AIClientId);
        sbyte newHP = (sbyte)(playerData.hp + healingValue);
        if (newHP > playerData.maxHp) newHP = playerData.maxHp;

        // 변경된 HP값 서버에 저장
        playerData.hp = newHP;
        CurrentPlayerDataManager.Instance.SetPlayerDataByClientId(wizardRukeAIServer.AIClientId, playerData);

        // 각 Client 플레이어의 HP바 UI 업데이트 ClientRPC       
        wizardRukeAIClient.SetHPClientRPC(playerData.hp, playerData.maxHp);
    }

    /// <summary>
    /// 서버에서 호출해야하는 메서드. 서버에서 동작합니다.
    /// </summary>
    public void TakingDamage(sbyte damage, ulong clientWhoAttacked)
    {
        // GamePlaying중이 아니면 전부 리턴. 게임이 끝나면 무적처리 되도록.
        if (!MultiplayerGameManager.Instance.IsGamePlaying()) return;
        if (wizardRukeAIServer.gameState != PlayerGameState.Playing) return;

        // 피격 사운드 효과 실행 ClientRPC

        // 피격 대미지 숫자 표시 실행. 각 Client Damage Text Popup UI 업데이트 지시 
        wizardRukeAIClient.ShowDamageTextPopupClientRPC(damage);

        // 요청한 플레이어 현재 HP값 가져오기 
        //playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(wizardRukeAIServer.AIClientId);
        sbyte newPlayerHP = playerData.hp;

        // HP보다 Damage가 클 경우 GameOver
        if (newPlayerHP <= damage)
        {
            // HP 0
            newPlayerHP = 0;
            playerData.playerGameState = PlayerGameState.GameOver; // 아래에서 HP값을 저장할 때 새로 SetPlayerData를 하기 때문에...! 일단 여기서 한 번더 게임오버 스테이트를 저장해주고 있는데, GameOver()에서 이미 게임오버처리를 해주고 있다. 일단은 동작하지만 수정 필요.

            // 게임오버 처리
            wizardRukeAIServer.GameOver(clientWhoAttacked);
        }
        else
        {
            // HP 감소 계산
            newPlayerHP -= (sbyte)damage;

            // 피격 애니메이션 실행 Server
            playerAnimator.UpdatePlayerAnimationOnServer(PlayerMoveAnimState.Hit);
        }

        // 변경된 HP값 서버에 저장
        playerData.hp = newPlayerHP;
        CurrentPlayerDataManager.Instance.SetPlayerDataByClientId(wizardRukeAIServer.AIClientId, playerData);

        // 각 Client 플레이어의 HP바 UI 업데이트 ClientRPC       
        wizardRukeAIClient.SetHPClientRPC(playerData.hp, playerData.maxHp);

        // 각 Client의 쉐이더 피격 이펙트 실행 ClientRPC
        wizardRukeAIClient.ActivateHitByAttackEffectClientRPC();
    }

    // 피격처리.
    public void TakingDamageWithCameraShake(sbyte damage, ulong clientIDWhoAttacked, GameObject clientObjectWhoAttacked)
    {
        // 피격 처리 총괄.
        TakingDamage(damage, clientIDWhoAttacked);

        // 공격자가 인식범위 안에 있으면 타겟으로 설정. 
        if (Vector3.Distance(transform.position, clientObjectWhoAttacked.transform.position) <= wizardRukeAIServer.maxDistanceDetect)
            wizardRukeAIServer.target = clientObjectWhoAttacked;

        // 방어스킬 발동
        wizardRukeAIBattleSystemServer.Defence();

        // 공격자가 Player라면 카메라 쉐이크 
        if (clientObjectWhoAttacked.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            playerClient.ActivateHitCameraShakeClientRPC();
        }
    }

    public void StartToTakeDotDamage(sbyte damagePerSecond, float duration, ulong attackerClientId)
    {
        StartCoroutine(TakeDamageOverTime(damagePerSecond, duration, attackerClientId));
    }

    // 도트 대미지를 받는 Coroutine (ex. Fire Type 마법의 점화)
    public IEnumerator TakeDamageOverTime(sbyte damagePerSecond, float duration, ulong clientWhoAttacked)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            // 1초 대기
            yield return new WaitForSeconds(1);

            TakingDamage(damagePerSecond, clientWhoAttacked);

            elapsed += 1;
        }
    }

    public sbyte GetHP()
    {
        return playerData.hp;
    }
}
