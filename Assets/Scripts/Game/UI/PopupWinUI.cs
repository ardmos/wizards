using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 게임씬 승리 플레이어에게 노출되는 UI
/// 1. 게임동안 얻은 배틀패스 경험치(한 판당 10) 슬라이더에 뜨워주기.(슬라이더가 차오르는것이 보이게끔 구현)
/// 2. 얻은 아이템 띄워주기. (하나씩 애니메이션을 보여주며 생성)
/// 3. 'Claim' 클릭시 모든 상품 수령 & 로비씬 이동
/// 4. 'Claim 2배' 클릭시 광고영상 재생
///     1. 광고 시청 실패시 원상복귀
///     2. 광고 시청 완료시 모든 상품 2배 수령(배틀패스 포함) & 로비씬 이동
///     
/// 팝업에서 동작하는 애니메이션들 목록(정리용)
/// 1. Victory 문구 최초 중앙 등장 이후 상승
/// 2. 1번 이후 Detail영역 x축 scale값 상승 애니메이션
/// 3. 2번 이후 배틀패스 슬라이더 값 차오르는 애니메이션
/// 4. 2번 이후 얻은 아이템들 순서대로 또잉또잉 등장
/// </summary>
public class PopupWinUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
