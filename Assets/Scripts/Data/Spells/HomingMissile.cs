using Unity.Netcode;
using UnityEngine;

public class HomingMissile : NetworkBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private GameObject _target;
    [SerializeField] private ulong _shooterClientID;
    [SerializeField] private bool _startHoming = false;

    [Header("MOVEMENT")]
    [SerializeField] private float _speed = 0;
    [SerializeField] private float _rotateSpeed = 95;

    [Header("PREDICTION")]
    [SerializeField] private float _maxDistancePredict = 8;
    [SerializeField] private float _minDistancePredict = 1;
    [SerializeField] private float _maxTimePrediction = 2;
    [SerializeField] private Vector3 _standardPrediction, _deviatedPrediction;

    [Header("DEVIATION")]
    [SerializeField] private float _deviationAmount = 50;
    [SerializeField] private float _deviationSpeed = 2;

    [Header("AI가 피격됐을 시 타겟으로 설정될 마법을 소유한 플레이어 오브젝트.")]
    public GameObject spellOwnerObject;

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
            // 일정 범위 안에 있는 게임 오브젝트를 탐지하여 _target으로 설정합니다.
            DetectAndSetTarget();         
        }
    }

    public void StartHoming()
    {
        _startHoming = true;
    }

    public void SetOwner(ulong shooterClientID, GameObject spellOwnerObject)
    {
        if (!IsServer) return;
        _shooterClientID = shooterClientID;
        this.spellOwnerObject = spellOwnerObject;
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    public void SetMaxHomingRange(float newValue)
    {
        _maxDistancePredict = newValue;
    }

    public float GetMaxHomingRange()
    {
        return _maxDistancePredict;
    }

    private void PredictMovement(float leadTimePercentage)
    {
        var predictionTime = Mathf.Lerp(0, _maxTimePrediction, leadTimePercentage);

        _standardPrediction = _target.GetComponent<Rigidbody>().position + _target.GetComponent<Rigidbody>().velocity * predictionTime;
    }

    private void DetectAndSetTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _maxDistancePredict);
        foreach (var collider in colliders)
        {
            //Debug.Log($"호밍 디텍티드! 호밍미사일:{gameObject.layer}, 디텍티드:{collider.gameObject.layer}");

            // 자신을 제외한 Player나 AI를 타겟으로 삼습니다.
            if (collider.gameObject == spellOwnerObject) continue;

            if (collider.CompareTag("Player") || collider.CompareTag("AI"))
            {
                _target = collider.gameObject;
                break; // 첫 번째로 발견된 오브젝트만 타겟으로 설정합니다.
            }
        }
    }

    private void RotateRocket()
    {
        var heading = _deviatedPrediction - transform.position;
        var rotation = Quaternion.LookRotation(heading);

        // 위나 아래로는 이동할 필요가 없기 때문에 Y축 회전만 하도록 만듭니다.  
        rotation = new Quaternion(0, rotation.y, 0, rotation.w);

        _rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, _rotateSpeed * Time.deltaTime));
    }

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
