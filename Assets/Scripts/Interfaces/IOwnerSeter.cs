using UnityEngine;

public interface IOwnerSeter
{
    public void SetOwner(ulong shooterClientID, GameObject spellOwnerObject);
}
