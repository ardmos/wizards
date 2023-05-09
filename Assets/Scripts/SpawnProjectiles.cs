using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 발사체들을 생성하는 스크립트
/// 1. Update()에서 Player.cs의 공격상태를 매 프레임 체크
/// 2. 공격상태 참이면 VFX 프리팹 생성(각각 알맞는 Projectile.cs가 붙어있음) & 카메라 흔들기(CameraShake.cs) 실행
/// 
/// 
/// 이 부분에서, 각 스킬들 실행할 때, 커맨드 패턴 적용할 필요가 있음.  수정 필요
/// </summary>

public class SpawnProjectiles : MonoBehaviour
{
    //private const string FIRE_BALL_1 = "FireBall1";

    [SerializeField] private Player player;
    public GameObject muzzle;
    public GameObject mainCamera;
    public bool cameraShake;
    public float timeToFire;

    // 스킬들. 각 발사체 VFX들 프리팹 리스트 저장 
    public List<GameObject> vfxProjectilePrefabs = new List<GameObject>();
    // 발사하기로 선택된 프리팹
    private GameObject vfxProjectileToSpawn;

    private void Update()
    {
/*        switch (skillName)
        {
            case FIRE_BALL_1:
                


                break;
            default:
                Debug.LogError("스킬목록에서 스킬을 찾지 못했습니다.");
                return;
        }*/


        if (player.IsAttack1()) {
            // 테스트용. 무조건 Attack1 파이어볼.   커맨드패턴 사용하게끔 수정할 때, 추후 플레이어가 줍는 스킬들로 동적으로 스킬 벨트가 세팅될 수 있게끔 신경써서 수정하기. 
            vfxProjectileToSpawn = vfxProjectilePrefabs[0]; // 테스트용 Attack1이 있는 0번 인덱스.
            // 연사율(쿨다운) 체크 연사율 체크 로직도 수정 검토하기. CodeMonkey 참고. 
            if (Time.time >= timeToFire)
            {
                //Debug.Log("발사 !!! Time.time:" + Time.time + ", timeToFire:" + timeToFire);
                timeToFire = Time.time + 1f / vfxProjectileToSpawn.GetComponent<ProjectileMoveScript>().fireRate;
                SpawnVFXProjectile(vfxProjectileToSpawn);  
                ShakeMainCamera();
            }
            //else Debug.Log("쿨다운 입니다 ... Time.time:"+ Time.time +", timeToFire:" + timeToFire);
        }
    }

    private void SpawnVFXProjectile(GameObject skillPrefObj)
    {      
        if (muzzle != null)
        {
            GameObject projectilePrefab;
            // 포구에 발사체 위치시키기
            projectilePrefab = Instantiate(skillPrefObj, muzzle.transform.position, Quaternion.identity);
            // 플레이어가 보고있는 방향과 발사체가 바라보는 방향 일치시키기. 직진이나 유도 등 이동은 발사체가 알아서 함.
            projectilePrefab.transform.localRotation = player.transform.localRotation;
        }   
    }

    private void ShakeMainCamera()
    {
        var cameraShakeScript = mainCamera.GetComponent<CameraShake>();
        if (cameraShake && cameraShakeScript != null)
        {
            cameraShakeScript.ShakeCamera();
            cameraShake = !cameraShake;
        }
            

        
    }
}
