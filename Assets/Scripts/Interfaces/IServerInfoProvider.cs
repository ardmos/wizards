using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;

/// <summary>
/// 서버 정보를 제공하는 인터페이스입니다.
/// 이 인터페이스는 멀티플레이 서비스, 매치메이킹 결과, 외부 연결 문자열 등
/// 서버 관련 핵심 정보에 대한 접근을 제공합니다.
/// </summary>
public interface IServerInfoProvider
{
    /// <summary>
    /// 멀티플레이 서비스 인스턴스를 반환합니다.
    /// </summary>
    /// <returns>IMultiplayService 인스턴스</returns>
    IMultiplayService GetMultiplayService();

    /// <summary>
    /// 현재 매치메이킹 결과를 반환합니다.
    /// </summary>
    /// <returns>MatchmakingResults 객체</returns>
    MatchmakingResults GetMatchmakerPayload();

    /// <summary>
    /// 서버의 외부 연결 문자열을 반환합니다.
    /// </summary>
    /// <returns>IP:Port 형식의 연결 문자열</returns>
    string GetExternalConnectionString();
}