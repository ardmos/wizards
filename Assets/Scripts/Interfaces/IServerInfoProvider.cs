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
    IMultiplayService GetMultiplayService();

    /// <summary>
    /// ���� ��ġ����ŷ ����� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>MatchmakingResults ��ü</returns>
    MatchmakingResults GetMatchmakerPayload();

    /// <summary>
    /// ������ �ܺ� ���� ���ڿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>IP:Port ������ ���� ���ڿ�</returns>
    string GetExternalConnectionString();
}