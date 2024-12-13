using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;

/// <summary>
/// ���� ������ �����ϴ� �������̽��Դϴ�.
/// �� �������̽��� ��Ƽ�÷��� ����, ��ġ����ŷ ���, �ܺ� ���� ���ڿ� ��
/// ���� ���� �ٽ� ������ ���� ������ �����մϴ�.
/// </summary>
public interface IServerInfoProvider
{
    /// <summary>
    /// ��Ƽ�÷��� ���� �ν��Ͻ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>IMultiplayService �ν��Ͻ�</returns>
    public IMultiplayService GetMultiplayService();

    /// <summary>
    /// ���� ��ġ����ŷ ����� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>MatchmakingResults ��ü</returns>
    public MatchmakingResults GetMatchmakerPayload();

    /// <summary>
    /// ������ �ܺ� ���� ���ڿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>IP:Port ������ ���� ���ڿ�</returns>
    public string GetExternalConnectionString();
}