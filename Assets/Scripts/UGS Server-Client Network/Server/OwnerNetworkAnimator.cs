using Unity.Netcode.Components;
/// <summary>
/// ����� ������� �ʽ��ϴ�.
/// ������ ����(�������� X ���������� �׳� NetworkAnimator ���� �˴ϴ�) �ִϸ��̼� ��ũ�� ���߱� ���� ��ũ��Ʈ �Դϴ�.
/// https://docs-multiplayer.unity3d.com/netcode/current/components/networkanimator/#owner-authoritative-mode
/// </summary>
public class OwnerNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}