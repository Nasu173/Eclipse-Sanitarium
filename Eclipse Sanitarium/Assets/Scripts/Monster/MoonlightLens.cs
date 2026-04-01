using System.Collections;
using UnityEngine;

public class MoonlightLens : MonoBehaviour, IInteractable
{
    [Header("透镜设置")]
    [Tooltip("照射范围")]
    public float beamRange = 10f;

    [Tooltip("照射宽度")]
    public float beamWidth = 2f;

    [Tooltip("冷却时间")]
    public float cooldown = 3f;

    [Header("效果设置")]
    public bool causeStun = true;
    public bool causeSlow = true;

    [Header("视觉特效")]
    public GameObject beamEffect;
    public AudioClip beamSound;

    // 私有变量
    private bool _isOnCooldown = false;
    private AudioSource _audioSource;
    private LineRenderer _lineRenderer;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }

        if (beamEffect != null)
        {
            beamEffect.SetActive(false);
        }
    }

    void Update()
    {
        // 按住Shift可预览射线方向（仅编辑器）
        if (Application.isEditor && Input.GetKey(KeyCode.LeftShift))
        {
            Debug.DrawRay(transform.position, transform.forward * beamRange, Color.green);
        }
    }

    public void UseLens()
    {
        if (_isOnCooldown) return;

        StartCoroutine(FireBeam());
    }

    private IEnumerator FireBeam()
    {
        _isOnCooldown = true;

        // 激活光束特效
        if (beamEffect != null)
        {
            beamEffect.SetActive(true);
        }

        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = true;
        }

        // 播放音效
        if (beamSound != null)
        {
            _audioSource.PlayOneShot(beamSound);
        }

        // 执行照射
        PerformBeamHit();

        // 等待光束显示时间
        yield return new WaitForSeconds(0.5f);

        // 关闭特效
        if (beamEffect != null)
        {
            beamEffect.SetActive(false);
        }

        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }

        // 等待冷却
        yield return new WaitForSeconds(cooldown);
        _isOnCooldown = false;
    }

    private void PerformBeamHit()
    {
        // 从透镜位置向前发射射线
        Ray ray = new Ray(transform.position, transform.forward);

        // 使用SphereCast检测扇形区域内的物体
        RaycastHit[] hits = Physics.SphereCastAll(ray, beamWidth, beamRange);

        foreach (var hit in hits)
        {
            // 检查是否击中护士长（支持父物体查找）
            NurseChaser chaser = hit.collider.GetComponent<NurseChaser>();
            if (chaser == null)
            {
                chaser = hit.collider.GetComponentInParent<NurseChaser>();
            }

            if (chaser != null)
            {
                // 应用效果
                if (causeStun)
                {
                    chaser.OnHitByMoonlight();
                }
                else if (causeSlow)
                {
                    chaser.ApplySlow();
                }
            }
        }
    }

    #region IInteractable 接口实现

    public string GetInteractPrompt()
    {
        if (_isOnCooldown)
        {
            return "月光透镜（冷却中）";
        }
        return "[E] 激活月光透镜";
    }

    public void OnInteract()
    {
        UseLens();
    }

    public void ToggleHighlight(bool isHighlighted)
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = isHighlighted;
        }
    }

    #endregion
}