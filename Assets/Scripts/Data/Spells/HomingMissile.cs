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

    [Header("AI�� �ǰݵ��� �� Ÿ������ ������ ������ ������ �÷��̾� ������Ʈ.")]
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
            // ���� ���� �ȿ� �ִ� ���� ������Ʈ�� Ž���Ͽ� _target���� �����մϴ�.
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
            //Debug.Log($"ȣ�� ����Ƽ��! ȣ�ֹ̻���:{gameObject.layer}, ����Ƽ��:{collider.gameObject.layer}");

            // �ڽ��� ������ Player�� AI�� Ÿ������ ����ϴ�.
            if (collider.gameObject == spellOwnerObject) continue;

            if (collider.CompareTag("Player") || collider.CompareTag("AI"))
            {
                _target = collider.gameObject;
                break; // ù ��°�� �߰ߵ� ������Ʈ�� Ÿ������ �����մϴ�.
            }
        }
    }

    private void RotateRocket()
    {
        var heading = _deviatedPrediction - transform.position;
        var rotation = Quaternion.LookRotation(heading);

        // ���� �Ʒ��δ� �̵��� �ʿ䰡 ���� ������ Y�� ȸ���� �ϵ��� ����ϴ�.  
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
