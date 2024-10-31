using Unity.Netcode.Components;
using UnityEngine;

/// Client 권한 방식으로 UGS NetworkTransform을 사용할 때 쓰는 스크립트 입니다.
/// 지금은 서버 권한 방식이라 사용하지 않습니다. 
namespace Unity.Multiplayer.Samples.Utilities.ClientAuthority
{
    /// <summary>
    /// Used for syncing a transform with client side changes. This includes host. Pure server as owner isn't supported by this. Please use NetworkTransform
    /// for transforms that'll always be owned by the server.
    /// </summary>
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {
        /// <summary>
        /// Used to determine who can write to this transform. Owner client only.
        /// This imposes state to the server. This is putting trust on your clients. Make sure no security-sensitive features use this transform.
        /// </summary>
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}