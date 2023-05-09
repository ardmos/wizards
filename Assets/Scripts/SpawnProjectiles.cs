using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �߻�ü���� �����ϴ� ��ũ��Ʈ
/// 1. Update()���� Player.cs�� ���ݻ��¸� �� ������ üũ
/// 2. ���ݻ��� ���̸� VFX ������ ����(���� �˸´� Projectile.cs�� �پ�����) & ī�޶� ����(CameraShake.cs) ����
/// 
/// 
/// �� �κп���, �� ��ų�� ������ ��, Ŀ�ǵ� ���� ������ �ʿ䰡 ����.  ���� �ʿ�
/// </summary>

public class SpawnProjectiles : MonoBehaviour
{
    //private const string FIRE_BALL_1 = "FireBall1";

    [SerializeField] private Player player;
    public GameObject muzzle;
    public GameObject mainCamera;
    public bool cameraShake;
    public float timeToFire;

    // ��ų��. �� �߻�ü VFX�� ������ ����Ʈ ���� 
    public List<GameObject> vfxProjectilePrefabs = new List<GameObject>();
    // �߻��ϱ�� ���õ� ������
    private GameObject vfxProjectileToSpawn;

    private void Update()
    {
/*        switch (skillName)
        {
            case FIRE_BALL_1:
                


                break;
            default:
                Debug.LogError("��ų��Ͽ��� ��ų�� ã�� ���߽��ϴ�.");
                return;
        }*/


        if (player.IsAttack1()) {
            // �׽�Ʈ��. ������ Attack1 ���̾.   Ŀ�ǵ����� ����ϰԲ� ������ ��, ���� �÷��̾ �ݴ� ��ų��� �������� ��ų ��Ʈ�� ���õ� �� �ְԲ� �Ű�Ἥ �����ϱ�. 
            vfxProjectileToSpawn = vfxProjectilePrefabs[0]; // �׽�Ʈ�� Attack1�� �ִ� 0�� �ε���.
            // ������(��ٿ�) üũ ������ üũ ������ ���� �����ϱ�. CodeMonkey ����. 
            if (Time.time >= timeToFire)
            {
                //Debug.Log("�߻� !!! Time.time:" + Time.time + ", timeToFire:" + timeToFire);
                timeToFire = Time.time + 1f / vfxProjectileToSpawn.GetComponent<ProjectileMoveScript>().fireRate;
                SpawnVFXProjectile(vfxProjectileToSpawn);  
                ShakeMainCamera();
            }
            //else Debug.Log("��ٿ� �Դϴ� ... Time.time:"+ Time.time +", timeToFire:" + timeToFire);
        }
    }

    private void SpawnVFXProjectile(GameObject skillPrefObj)
    {      
        if (muzzle != null)
        {
            GameObject projectilePrefab;
            // ������ �߻�ü ��ġ��Ű��
            projectilePrefab = Instantiate(skillPrefObj, muzzle.transform.position, Quaternion.identity);
            // �÷��̾ �����ִ� ����� �߻�ü�� �ٶ󺸴� ���� ��ġ��Ű��. �����̳� ���� �� �̵��� �߻�ü�� �˾Ƽ� ��.
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
