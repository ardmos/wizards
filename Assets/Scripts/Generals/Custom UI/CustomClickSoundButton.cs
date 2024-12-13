using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI ��ư Ŭ�� ���� �߰��� ���� Ŀ���� ��ư ��ũ��Ʈ �Դϴ�.
/// </summary>
public class CustomClickSoundButton : Button
{
    public void AddClickListener(UnityAction call)
    {
        onClick.AddListener(()=> {
            SoundManager.Instance.PlayButtonClickSound();
            call(); 
        });
    }
}
