using Unity.Netcode;
using UnityEngine;

public class HomingMissile : NetworkBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private PlayerServer _target;
    [SerializeField] private ulong _shooterClientID;
    /*    [SerializeField] private GameObject _explosionPrefab;
        [SerializeField] private GameObject _muzzlePrefab;
        [SerializeField] private sbyte damage = 1;*/
    [SerializeField] private bool _startHoming = false;

    [Header("MOVEMENT")]
    [SerializeField] private float _speed = 0;
    [SerializeField] private float _rotateSpeed = 95;

    [Header("PREDICTION")]
    [SerializeField] private float _maxDistancePredict = 5;
    [SerializeField] private float _minDistancePredict = 1;
    [SerializeField] private float _maxTimePrediction = 2;
    [SerializeField] private Vector3 _standardPrediction, _deviatedPrediction;

    [Header("DEVIATION")]
    [SerializeField] private float _deviationAmount = 50;
    [SerializeField] private float _deviationSpeed = 2;

    private void Awake()
    {
        _rb.isKinematic = false;
        //Destroy(gameObject, 5f);
    }

    private void FixedUpdate()
    {
        if (!_startHoming) return;

        _rb.velocity = transform.forward * _speed;
    
        if (_target)
        {
            float leadTimePercentage = Mathf.InverseLerp(_minDistancePredict, _maxDistancePredict, Vector3.Distance(transform.position, _target.transform.position));
            PredictMovement(leadTimePercentage);

            //AddDeviation(leadTimePercentage);
            _deviatedPrediction = _standardPrediction;

            RotateRocket();
        }   
        else
        {
            // ���� ���� �ȿ� �ִ� ���� ������Ʈ�� Ž���Ͽ� _target���� �����մϴ�.
            DetectAndSetTarget();         
        }
    }

    public void StartHoming()
    {
        _startHoming = true;
    }

    public void SetOwner(ulong shooterClientID)
    {
        _shooterClientID = shooterClientID;

        // �÷��̾� Layer ����
        switch (_shooterClientID)
        {
            case 0:
                gameObject.layer = LayerMask.NameToLayer("Player0");
                break;
            case 1:
                gameObject.layer = LayerMask.NameToLayer("Player1");
                break;
            case 2:
                gameObject.layer = LayerMask.NameToLayer("Player2");
                break;
            case 3:
                gameObject.layer = LayerMask.NameToLayer("Player3");
                break;
            default:
                break;
        }

        // �÷��̾� ���� Layer�� �浹üũ���� �����մϴ�
        Physics.IgnoreLayerCollision(gameObject.layer, gameObject.layer, true);
/*        Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Floor"), true);
        Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Item"), true);*/
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    /*    public GameObject GetMuzzleVFXPrefab()
        {
            return _muzzlePrefab;
        }*/

    private void PredictMovement(float leadTimePercentage)
    {
        var predictionTime = Mathf.Lerp(0, _maxTimePrediction, leadTimePercentage);

        _standardPrediction = _target.rb.position + _target.rb.velocity * predictionTime;
    }

/*    private void AddDeviation(float leadTimePercentage)
    {
        var deviation = new Vector3(Mathf.Cos(Time.time * _deviationSpeed), 0, 0);

        var predictionOffset = transform.TransformDirection(deviation) * _deviationAmount * leadTimePercentage;

        _deviatedPrediction = _standardPrediction + predictionOffset;
    }*/

    private void DetectAndSetTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _maxDistancePredict);
        foreach (var collider in colliders)
        {
            //Debug.Log($"ȣ�� ����Ƽ��! ȣ�ֹ̻���:{gameObject.layer}, ����Ƽ��:{collider.gameObject.layer}");
            // _target���� ������ ������Ʈ�� ������ ���⿡ �߰��մϴ�.
            // ���� ���, �±װ� "Player"�̰�, ��Ʈ��ũ �󿡼� ��ȿ�� ������Ʈ�� ��쿡�� _target���� ������ �� �ֽ��ϴ�.
            if (collider.CompareTag("Player") && collider.gameObject.layer != gameObject.layer)
            {
                _target = collider.GetComponent<PlayerServer>();
                break; // ù ��°�� �߰ߵ� ������Ʈ�� Ÿ������ �����մϴ�.
            }
        }
    }

    private void RotateRocket()
    {
        var heading = _deviatedPrediction - transform.position;
        //heading.y = 1f;
        //Debug.Log($"_deviatedPrediction{_deviatedPrediction}, transform.position{transform.position}, heading{heading}");
        var rotation = Quaternion.LookRotation(heading);
        //rotation = new Quaternion(rotation.x, 0, rotation.z, rotation.w);
        //Debug.Log($"ȸ�� rotation:{rotation}");
        _rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, _rotateSpeed * Time.deltaTime));
    }

/*    private void OnCollisionEnter(Collision collision)
    {
*//*        if (_explosionPrefab)
        {
            GameObject explosionVFX = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            explosionVFX.GetComponent<NetworkObject>().Spawn();
        }
        if (collision.transform.TryGetComponent<PlayerHPManagerServer>(out var ex)) ex.TakingDamage(damage, _shooter.OwnerClientId);

        Destroy(gameObject);*//*
    }*/

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, _standardPrediction);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(_standardPrediction, _deviatedPrediction);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _maxDistancePredict);
    }
}
