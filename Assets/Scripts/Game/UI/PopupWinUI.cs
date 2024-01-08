using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ���Ӿ� �¸� �÷��̾�� ����Ǵ� UI
/// 1. ���ӵ��� ���� ��Ʋ�н� ����ġ(�� �Ǵ� 10) �����̴��� �߿��ֱ�.(�����̴��� �������°��� ���̰Բ� ����)
/// 2. ���� ������ ����ֱ�. (�ϳ��� �ִϸ��̼��� �����ָ� ����)
/// 3. 'Claim' Ŭ���� ��� ��ǰ ���� & �κ�� �̵�
/// 4. 'Claim 2��' Ŭ���� ������ ���
///     1. ���� ��û ���н� ���󺹱�
///     2. ���� ��û �Ϸ�� ��� ��ǰ 2�� ����(��Ʋ�н� ����) & �κ�� �̵�
///     
/// �˾����� �����ϴ� �ִϸ��̼� ���
/// Step1. Victory ���� ���� �߾� ���� ���� ���
/// Step2. Step1 ���� Detail���� x�� scale�� ��� �ִϸ��̼�
/// Step3. Step2 ���� ��Ʋ�н� �����̴� �� �������� �ִϸ��̼� & ���� �����۵� ������� ���׶��� ����
/// </summary>
public class PopupWinUI : MonoBehaviour
{
    private enum AnimationState {
        Step1,
        Step2,
        Step3
    }

    private AnimationState animationState;

    [SerializeField] private Slider sliderBattlePath;
    [SerializeField] private GameObject detailArea;
    [SerializeField] private GameObject btnClaim;
    [SerializeField] private GameObject btnClaim2x;
    [SerializeField] private GameObject ImgEffect;
    [SerializeField] private GameObject ItemTemplateGray;
    [SerializeField] private GameObject ItemTemplateBlue;
    [SerializeField] private GameObject ItemTemplateYellow;

    // Start is called before the first frame update
    void Start()
    {
        animationState = AnimationState.Step1;
        StartAnimation(animationState);
    }

    private void StartAnimation(AnimationState animationState)
    {
        switch (animationState)
        {
            case AnimationState.Step1:
                // Step1. Victory ���� ���� �߾� ���� ���� ���

                
                break; 
            case AnimationState.Step2:
                // Step2. Detail���� x�� scale�� ��� �ִϸ��̼�
                break;
            case AnimationState.Step3:
                // Step3. ��Ʋ�н� �����̴� �� �������� �ִϸ��̼� & ���� �����۵� ������� ���׶��� ���� & �ɰ��� ����
                break;
        }
    }
}
