using Unity.Netcode.Components;
/// <summary>
/// 현재는 사용하지 않습니다.
/// 소유자 권한(서버권한 X 서버권한은 그냥 NetworkAnimator 쓰면 됩니다) 애니메이션 싱크를 맞추기 위한 스크립트 입니다.
/// https://docs-multiplayer.unity3d.com/netcode/current/components/networkanimator/#owner-authoritative-mode
/// </summary>
public class OwnerNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}