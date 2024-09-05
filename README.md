# 실시간 멀티플레이어 대전 게임 프로젝트 - Wizards and Knights

![image](https://github.com/user-attachments/assets/08233f94-d136-4ed6-82df-a3a3d4b60e11)

## 프로젝트 개요
이 프로젝트는 Unity 엔진을 사용하여 개발한 멀티플레이어 게임입니다. Unity Gaming Services (UGS)를 활용하여 서버-클라이언트 구조를 구현하였습니다.

## 주요 기능 및 기술
- **네트워크 통신**: Unity Gaming Services 프레임워크를 활용하여 원격 프로시저 호출(Remote Procedure Calls, RPC) 방식으로 클라이언트와 서버 간의 통신을 구현하였습니다.
- **서버 인프라**: Unity의 Game Server Hosting 서비스를 통해 서버 인프라를 개발 및 관리하고 있습니다.
- **매칭 시스템**: Unity의 Matchmaker 서비스를 사용하여 플레이어 매칭 기능을 구현하였습니다.

## 사용 기술
- **게임 엔진**: Unity 2022.3.10f1
- **네트워킹**: Unity Gaming Services (UGS)
- **서버 호스팅**: Unity Game Server Hosting
- **매치메이킹**: Unity Matchmaker
- **프로그래밍 언어**: C#

## 구현 상세
1. **RPC 통신**
   - 클라이언트와 서버 간의 효율적인 데이터 교환을 위해 RPC 방식을 채택하였습니다.
   - 게임 상태 동기화, 플레이어 액션 전달 등에 활용됩니다.

2. **서버 인프라**
   - Unity Game Server Hosting을 통해 안정적이고 확장 가능한 서버 환경을 구축하였습니다.
   - 서버 자동 스케일링, 로드 밸런싱 등의 기능을 활용하여 효율적인 리소스 관리를 실현하였습니다.

3. **매치메이킹**
   - Unity Matchmaker를 사용하여 플레이어들을 적절히 매칭시키는 시스템을 구현하였습니다.
   - 플레이어 스킬 레벨, 지역 등을 고려한 매칭 알고리즘을 적용하였습니다.

4. **캐릭터 컨트롤러**
   - 유한 상태 기계(FSM)를 사용한 캐릭터 상태 관리

5. **전투 시스템**
   - 히트박스 기반의 정확한 충돌 감지

6. **네트워크 동기화**
   - 서버 권한 모델 구현

7. **최적화**
   - 오브젝트 풀링을 통한 메모리 관리
   - 코드 최적화(GC최소화, MonoBehaviour사용 최소화)
   - 라이트맵 사용
   - 사운드 최적화(압축률 조정), 이미지 리소스 최적화(압축률 조정), 쉐이더 최적화
   - 위 작업들을 통해 Unity Profiler기준 CPU Usage값을 1/8로 감소시켰습니다.

## 향후 개발 계획
- 추가적인 게임 모드 구현
- 캐릭터 추가
- 캐릭터 커스터마이징 기능 추가
- 성능 최적화 및 클라이언트 예측 기능 구현을 통한 네트워크 지연 개선
- 크로스 플랫폼 지원 확대

## 연락처
elis109777@gmail.com

## 이미지
https://sly-charger-b36.notion.site/Wizards-And-Knights-3D-73bc605a10204b95ad7e8381379b2085?pvs=74

## 구글 플레이스토어
https://play.google.com/store/apps/details?id=com.PuppyStepsGames.WizardsAndKnights&pcampaignid=web_share
