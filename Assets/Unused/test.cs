using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessV2Debugger : MonoBehaviour
{
    public PostProcessVolume vol;

    void Start()
    {
        if (vol == null)
        {
            Debug.LogWarning("PostProcessV2Debugger: ��� PostProcessVolume �ϵ� inspector �ϡ�");
            return;
        }

        vol.enabled = true;
        vol.weight = 1f;
        Debug.Log($"PostProcessVolume enabled={vol.enabled}, weight={vol.weight}, layer={LayerMask.LayerToName(vol.gameObject.layer)}");

        if (vol.profile != null)
        {
            if (vol.profile.TryGetSettings<Bloom>(out var bloom))
            {
                bloom.enabled.value = true;
                
                bloom.intensity.value = 10f;
                Debug.Log("PostProcessV2Debugger: ���� bloom intensity=10");
            }
            else
            {
                Debug.Log("PostProcessV2Debugger: Profile ��û�� Bloom ���û�������ͬ��");
            }
        }
        else
        {
            Debug.LogWarning("PostProcessV2Debugger: volume.profile Ϊ null��");
        }
    }
}
