using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class LightController : MonoBehaviour
{
    [Header("References (UI Images)")]
    public Image light;
    public Image player;
    public GameController controller;

    [Header("Rotation settings")]
    public float startAngle = -45f;
    public float endAngle = 45f;
    public float rotateSpeed = 90f; // 度 / 秒
    public float waitTime = 1f;     // 到端点的停留时间（秒）

    // 内部状态
    Rigidbody2D rb;
    float targetAngle;
    bool isPaused = false;
    bool skippedInitialPause = false;

    void Start()
    {
        if (light == null)
        {
            Debug.LogError("Light Image 未设置 (light)。请在 Inspector 指定。");
            enabled = false;
            return;
        }
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("需要 Rigidbody2D 组件。");
            enabled = false;
            return;
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        SetRotationImmediate(startAngle);
        targetAngle = endAngle;
    }
    void FixedUpdate()
    {
        if (controller.fail||controller.succeed||isPaused) return;
        if (light == null) return;
        float current = light.rectTransform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(current, targetAngle, rotateSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);
        light.rectTransform.localEulerAngles = new Vector3(0f, 0f, newAngle);
        if (Mathf.Abs(Mathf.DeltaAngle(newAngle, targetAngle)) < 0.01f)
        {
            StartCoroutine(PauseThenSwap());
        }
    }

    IEnumerator PauseThenSwap()
    {
        isPaused = true;
        yield return new WaitForSeconds(waitTime);
        isPaused = false;
        SwapTargetImmediate();
    }
    void SwapTargetImmediate()
    {
        targetAngle = Mathf.Approximately(targetAngle, endAngle) ? startAngle : endAngle;
    }

    void SetRotationImmediate(float angle)
    {
        light.rectTransform.localEulerAngles = new Vector3(0f, 0f, angle);
        if (rb != null)
            rb.rotation = angle;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (player != null && other.gameObject == player.gameObject && controller.succeed == false)
        {
            controller.fail = true;
        }
    }
    public void Reset()
    {
        SetRotationImmediate(startAngle);
        targetAngle = endAngle;
    }
#if UNITY_EDITOR
    void OnValidate()
    {
        // 确保 rotateSpeed 非负
        if (rotateSpeed < 0f) rotateSpeed = Mathf.Abs(rotateSpeed);
        if (light != null && Application.isPlaying == false)
        {
            light.rectTransform.localEulerAngles = new Vector3(0f, 0f, startAngle);
        }
    }
#endif
}
