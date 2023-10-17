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
    [SerializeField] private GameObject muzzle;
    //public GameObject mainCamera;
    //public bool cameraShake;
    [SerializeField] private float timeToFire;

    // 스킬들. 각 발사체 VFX들 프리팹 리스트 저장 
    [SerializeField] private List<GameObject> vfxProjectilePrefabs = new List<GameObject>();
    // 발사하기로 선택된 프리팹
    [SerializeField] private GameObject vfxProjectileToSpawn;

    private void Update()
    {
        /*        switch (skillName)
                {
                    case FIRE_BALL_1:



                        break;
                    default:
                        Debug.LogError("???????????? ?????? ???? ??????????.");
                        return;
                }*/

        // Test Code : Spell Controller 기능구현 테스트중 주석처리
        //if (player.IsAttack1()) {
        //    // ????????. ?????? Attack1 ????????.   ?????????? ?????????? ?????? ??, ???? ?????????? ???? ???????? ???????? ???? ?????? ?????? ?? ?????? ???????? ????????. 
        //    vfxProjectileToSpawn = vfxProjectilePrefabs[0]; // ???????? Attack1?? ???? 0?? ??????.
        //    // ??????(??????) ???? ?????? ???? ?????? ???? ????????. CodeMonkey ????. 
        //    // ProjectileMoveScript?? ???? ????????
        //    if (Time.time >= timeToFire)
        //    {
        //        //Debug.Log("???? !!! Time.time:" + Time.time + ", timeToFire:" + timeToFire);
        //        timeToFire = Time.time + 1f / vfxProjectileToSpawn.GetComponent<ProjectileMoveScript>().fireRate;
        //        SpawnVFXProjectile(vfxProjectileToSpawn);  
        //        //ShakeMainCamera();  ?????? ?????? ???????? ???????? ?????????? 
        //    }
        //    //else Debug.Log("?????? ?????? ... Time.time:"+ Time.time +", timeToFire:" + timeToFire);
        //}
    }

    private void SpawnVFXProjectile(GameObject skillPrefObj)
    {      
        if (muzzle != null)
        {
            GameObject projectilePrefab;
            // 포구에 발사체 위치시키기
            projectilePrefab = Instantiate(skillPrefObj, muzzle.transform.position, Quaternion.identity);
            // 아래 회전, 발사는 각 마법의 Move스크립트에서 처리. 여기서는 머즐에 생성만 해줌.
            // 플레이어가 보고있는 방향과 발사체가 바라보는 방향 일치시키기. 직진이나 유도 등 이동은 발사체가 알아서 함.
            projectilePrefab.transform.localRotation = player.transform.localRotation;
            // 소환시에 Impulse 
            float speed = 35f;
            projectilePrefab.GetComponent<Rigidbody>().AddForce(transform.forward * speed, ForceMode.Impulse);
        }   
    }

/*    private void ShakeMainCamera()
    {
        var cameraShakeScript = mainCamera.GetComponent<CameraShake>();
        if (cameraShake && cameraShakeScript != null)
        {
            cameraShakeScript.ShakeCamera();
            cameraShake = !cameraShake;
        }              
    }*/
}
