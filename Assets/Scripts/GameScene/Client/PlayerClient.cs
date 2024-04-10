using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class PlayerClient : NetworkBehaviour
{
    public static PlayerClient Instance {  get; private set; }

    private GameInput gameInput;

    private void Awake()
    {
        gameInput = GetComponent<GameInput>();
    }

    [ClientRpc]
    private void InitializePlayerClientRPC(SpellName[] ownedSpellNameList)
    {
        Debug.Log($"OwnerClientId {OwnerClientId} Player InitializePlayerClientRPC");

        // ī�޶� ��ġ �ʱ�ȭ. �����ڸ� ����ٴϵ��� �� 
        virtualCameraObj.SetActive(IsOwner);
        // �ڲ� isKinematic�� ������ �߰��� �ڵ�. Rigidbody network���� ��� �Ѵ� �� ����.
        GetComponent<Rigidbody>().isKinematic = false;
        // �÷��̾� �г��� ����
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        userNameUIController.Setup(playerData.playerName.ToString(), IsOwner);
        //Debug.Log($"player Name :{playerData.playerName.ToString()}");

        if (!IsOwner) return;

        Instance = this;

        // ���� ������ GamePad UI�� �ݿ�          
        //Debug.Log($"ownedSpellNameList.Length : {ownedSpellNameList.Length}");
        FindObjectOfType<GamePadUIController>().UpdateSpellUI(ownedSpellNameList);

        // Input Action �̺�Ʈ ����
        gameInput.OnAttack1Started += GameInput_OnAttack1Started;
        gameInput.OnAttack2Started += GameInput_OnAttack2Started;
        gameInput.OnAttack3Started += GameInput_OnAttack3Started;
        gameInput.OnAttack1Ended += GameInput_OnAttack1Ended;
        gameInput.OnAttack2Ended += GameInput_OnAttack2Ended;
        gameInput.OnAttack3Ended += GameInput_OnAttack3Ended;
        gameInput.OnDefenceStarted += GameInput_OnDefenceStarted;
        gameInput.OnDefenceEnded += GameInput_OnDefenceEnded;
    }

    private void OnDisable()
    {
        // ���� ����
        gameInput.OnAttack1Started -= GameInput_OnAttack1Started;
        gameInput.OnAttack2Started -= GameInput_OnAttack2Started;
        gameInput.OnAttack3Started -= GameInput_OnAttack3Started;
        gameInput.OnAttack1Ended -= GameInput_OnAttack1Ended;
        gameInput.OnAttack2Ended -= GameInput_OnAttack2Ended;
        gameInput.OnAttack3Ended -= GameInput_OnAttack3Ended;
        gameInput.OnDefenceStarted -= GameInput_OnDefenceStarted;
        gameInput.OnDefenceEnded -= GameInput_OnDefenceEnded;
    }

    protected abstract void GameInput_OnAttack1Started(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack2Started(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack3Started(object sender, EventArgs e);
    protected abstract void GameInput_OnDefenceStarted(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack1Ended(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack2Ended(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack3Ended(object sender, EventArgs e);
    protected abstract void GameInput_OnDefenceEnded(object sender, EventArgs e);

    [ClientRpc]
    public void SetHPClientRPC(sbyte hp, sbyte maxHP)
    {
        hPBarUIController.SetHP(hp, maxHP);
    }

    [ClientRpc]
    public void ShowDamagePopupClientRPC(byte damageAmount)
    {
        damageTextUIController.CreateTextObject(damageAmount);
    }

    /// <summary>
    /// ���� ������ ���۽�Ű�� �޼ҵ�.
    /// 1. �÷��̾� ���� �Ұ�
    /// 2. ���ӿ��� �˾� ����
    /// 3. �˾� BGM ���
    /// </summary>
    [ClientRpc]
    public void SetPlayerGameOverClientRPC()
    {
        //Debug.Log($"I'm player {OwnerClientId}");
        // �� ���ӿ��� ĳ������ �����ڰ� �ƴϸ� ����. RPC�� ������ üũ �� �� ����� ��. 
        if (!IsOwner) return;

        OnPlayerGameOver.Invoke(this, EventArgs.Empty);
        // Popup �����ֱ�
        GameSceneUIManager.Instance.popupGameOverUIController.Show();
        // BGM ���
        SoundManager.Instance.PlayLosePopupSound();
    }

    [ClientRpc]
    public void SetPlayerGameWinClientRPC()
    {
        // �� �¸� ĳ������ �����ڰ� �ƴϸ� ����.
        if (!IsOwner) return;

        OnPlayerWin.Invoke(this, EventArgs.Empty);
        // Popup �����ֱ�
        GameSceneUIManager.Instance.popupWinUIController.Show();
        // BGM ���
        SoundManager.Instance.PlayWinPopupSound();
    }

    // PlayerClient�� PlayerServer�� �ڵ� �з���. ���� �з��ϰ� �������̽� ������� �ϱ�. Knight����.
    // 1. WIzard, Knight�� �˸°� abstract �޼��� ���� �����ϱ�
    // 2. GetScore, GetSpellController �� �ʿ伺 Ȯ�� �� ��ġor�����ϱ�
    // 3. 
}
