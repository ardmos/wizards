using Unity.Netcode;

public class Scroll : NetworkBehaviour
{
    public Item.ItemName itemName;

    public virtual Spell ApplyScrollEffect(Spell targetSpell)
    {

        return targetSpell;
    }
}