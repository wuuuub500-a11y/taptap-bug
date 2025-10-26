using System.Collections;
using UnityEngine;

public class ScreenShaker : MonoBehaviour
{
    [Tooltip("����� Transform����Ϊ�ս��Զ�ʹ�� Camera.main.transform��")]
    public Transform cameraTransform;

    [Tooltip("�Ƿ�ʹ�÷�����ʱ�䣨unscaled time����Ĭ��Ϊ true")]
    public bool useUnscaledTime = true;

    private Vector3 originalLocalPos;
    private Coroutine shakeCoroutine;
    private float noiseSeedX;
    private float noiseSeedY;

    void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null)
            originalLocalPos = cameraTransform.localPosition;

        //�������
        noiseSeedX = Random.Range(0f, 1000f);
        noiseSeedY = Random.Range(0f, 1000f);
    }

    void OnDisable()
    {
        if (cameraTransform != null)
            cameraTransform.localPosition = originalLocalPos;
    }
    public void Shake(float amplitude, float frequency, float duration)
    {
        if (cameraTransform == null)
        {
            Debug.LogWarning("ScreenShaker: cameraTransform δ���ã�Ҳû���ҵ� Camera.main��");
            return;
        }

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            cameraTransform.localPosition = originalLocalPos;
            shakeCoroutine = null;
        }

        originalLocalPos = cameraTransform.localPosition;

        noiseSeedX = Random.Range(0f, 1000f);
        noiseSeedY = Random.Range(0f, 1000f);

        shakeCoroutine = StartCoroutine(ShakeCoroutine_Perlin(amplitude, frequency, duration));
    }

    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        if (cameraTransform != null)
            cameraTransform.localPosition = originalLocalPos;
    }

    private IEnumerator ShakeCoroutine_Perlin(float amplitude, float frequency, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += dt;
            float t = Mathf.Clamp01(elapsed / duration);
            float damper = Mathf.SmoothStep(1f, 0f, t); // �� 1 �� 0 ƽ��˥��
            float timeInput = elapsed * frequency;
            float nx = (Mathf.PerlinNoise(noiseSeedX, timeInput) - 0.5f) * 2f; // -1..1
            float ny = (Mathf.PerlinNoise(noiseSeedY, timeInput) - 0.5f) * 2f; // -1..1
            Vector3 offset = new Vector3(nx, ny, 0f) * amplitude * damper;
            cameraTransform.localPosition = originalLocalPos + offset;
            yield return null;
        }
        // ����ʱ��λ
        cameraTransform.localPosition = originalLocalPos;
        shakeCoroutine = null;
    }
}
