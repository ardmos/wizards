using TMPro;
using UnityEngine;

public class DamageTextUI : MonoBehaviour
{
    private const float DISAPPEAR_TIME_MAX = 0.8f;

    [SerializeField] private TextMeshProUGUI txtDamage;
    private Color txtColor;
    private float disappearTime;
    private float randomizeIntensity = 50f;
    private Vector3 offset = new Vector3 (0, 80f, 0);
    private Vector3 moveVector;


    public void Setup(sbyte damageAmount)
    {
        txtColor = txtDamage.color;
        txtDamage.text = damageAmount.ToString();
        transform.localPosition += offset;
        transform.localPosition += new Vector3(Random.Range(-randomizeIntensity, randomizeIntensity), 0f, 0f);
        disappearTime = DISAPPEAR_TIME_MAX;
        moveVector = new Vector3(0.7f, 2f) * 60f;
    }

    private void Update()
    {
        // 이동 효과 구현 부분
        transform.localPosition += moveVector * Time.deltaTime;
        //Debug.Log($"transform.localPosition:{transform.localPosition}, moveVector:{moveVector}, Time.deltaTime:{Time.deltaTime}");
        moveVector -= moveVector * 8f * Time.deltaTime;

        // 크기 변화 효과 구현 부분
        if (disappearTime > DISAPPEAR_TIME_MAX * 0.5f)
        {
            float increaseScaleAmount = 1f;
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        }
        else
        {
            float decreaseScaleAmount = 1f;
            transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
        }

        // 사라지기 효과 구현 부분
        disappearTime -= Time.deltaTime;
        if (disappearTime < 0) 
        {
            // 사라지기 시작
            float disappearSpeed = 3f;
            txtColor.a -= disappearSpeed * Time.deltaTime;
            txtDamage.color = txtColor;
            if(txtColor.a < 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
