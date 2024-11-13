using System;
using UnityEngine;

public class WizardServer : PlayerServer, ICharacter
{
    private const int MAX_SPELLS = 4;

    public Character characterClass { get; set; }
    public sbyte hp { get; set; }
    public sbyte maxHp { get; set; }
    public float moveSpeed { get; set; }
    public SpellName[] spells { get; set; }

    [SerializeField] private WizardClient wizardClient;

    #region Network Lifecycle
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        InitializeWizardCharacter();
    }
    #endregion

    private void InitializeWizardCharacter()
    {
        SetCharacterData(CharacterSpecifications.GetCharacter(Character.Wizard));
        InitializePlayerOnServer(this);
        // 클라이언트측 플레이어도 초기화 실행
        if (spells == null) return;
        wizardClient.InitializeWizardClientRPC(spells);
    }

    #region ICharacter 구현 
    public ICharacter GetCharacterData() => this;
    public void SetCharacterData(ICharacter character)
    {
        characterClass = character.characterClass;
        hp = character.hp;
        maxHp = character.maxHp;
        moveSpeed = character.moveSpeed;
        spells = new SpellName[MAX_SPELLS];
        Array.Copy(character.spells, spells, Math.Min(character.spells.Length, MAX_SPELLS));
    }
    public ulong GetClientID() => OwnerClientId;
    #endregion

}
