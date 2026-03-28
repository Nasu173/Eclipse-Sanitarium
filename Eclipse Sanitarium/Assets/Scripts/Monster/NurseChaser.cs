using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NurseChaser : MonoBehaviour
{
    [Header("基础设置")]
    [Tooltip("追捕状态")]
    public bool isChasing = false;

    [Tooltip("追捕速度")]
    public float chaseSpeed = 3.5f;

    [Tooltip("正常速度（未追捕时）")]
    public float normalSpeed = 1.5f;

    [Header("追捕范围")]
    [Tooltip("开始追捕的距离")]
    public float chaseStartDistance = 10f;

    [Tooltip("放弃追捕的距离")]
    public float chaseStopDistance = 20f;

    [Tooltip("攻击距离")]
    public float attackDistance = 1.5f;

    [Header("停顿设置")]
    [Tooltip("追捕中的停顿时间（秒）")]
    public float pauseTimeMin = 1f;
    public float pauseTimeMax = 3f;

    [Tooltip("停顿间隔（秒）")]
    public float pauseInterval = 5f;

    [Header("被月光照射后的反应")]
    [Tooltip("被照射后停顿时间")]
    public float stunnedDuration = 3f;

    [Tooltip("被照射后的减速时间")]
    public float slowDuration = 2f;

    [Tooltip("被照射后的减速倍率")]
    [Range(0f, 1f)]
    public float slowMultiplier = 0.3f;

    [Header("音效")]
    [Tooltip("脚步声")]
    public AudioClip footstepSound;

    [Tooltip("发现玩家时的音效")]
    public AudioClip spotSound;

    [Tooltip("被照射时的音效")]
    public AudioClip stunnedSound;

    [Tooltip("攻击音效")]
    public AudioClip attackSound;

    // 私有变量
    private Transform _player;
    private NavMeshAgent _agent;
    private Animator _animator;
    private AudioSource _audioSource;
    private float _pauseTimer = 0f;
    private bool _isPaused = false;
    private bool _isStunned = false;
    private float _stunnedTimer = 0f;
    private float _slowTimer = 0f;
    private float _originalSpeed;
    private bool _hasSpotted = false;

    void Start()
    {
        // 获取组件
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 保存原始速度
        if (_agent != null)
        {
            _originalSpeed = _agent.speed;
            _agent.speed = normalSpeed;
        }

        // 初始化停顿计时器
        _pauseTimer = Random.Range(pauseInterval * 0.5f, pauseInterval);
    }

    void Update()
    {
        if (!isChasing || _player == null) return;

        // 处理被照射后的减速效果
        HandleSlowEffect();

        // 处理被照射后的眩晕效果
        HandleStunnedEffect();

        // 如果眩晕中，不进行追捕
        if (_isStunned) return;

        // 计算与玩家的距离
        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        // 检查是否应该停止追捕
        if (distanceToPlayer > chaseStopDistance)
        {
            StopChase();
            return;
        }

        // 检查是否应该开始追捕（如果还没开始）
        if (distanceToPlayer < chaseStartDistance && !_isPaused)
        {
            StartChase();
        }

        // 追捕逻辑
        if (!_isPaused && !_isStunned)
        {
            // 移动向玩家
            _agent.SetDestination(_player.position);

            // 检查是否在攻击范围内
            if (distanceToPlayer <= attackDistance)
            {
                Attack();
            }
        }
        else if (_isPaused && !_isStunned)
        {
            // 停顿状态，停止移动
            _agent.isStopped = true;

            // 更新停顿计时器
            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer <= 0)
            {
                _isPaused = false;
                _agent.isStopped = false;
                _pauseTimer = Random.Range(pauseInterval * 0.8f, pauseInterval * 1.2f);
            }
        }

        // 更新动画
        UpdateAnimation();
    }

    /// <summary>
    /// 开始追捕
    /// </summary>
    private void StartChase()
    {
        if (!_hasSpotted)
        {
            _hasSpotted = true;

            // 播放发现玩家的音效
            if (spotSound != null)
            {
                _audioSource.PlayOneShot(spotSound);
            }

            // 播放发现动画
            if (_animator != null)
            {
                _animator.SetTrigger("Spot");
            }
        }

        isChasing = true;
        _agent.speed = chaseSpeed;
        _agent.isStopped = false;

        Debug.Log("护士长开始追捕玩家");
    }

    /// <summary>
    /// 停止追捕
    /// </summary>
    public void StopChase()
    {
        isChasing = false;
        _hasSpotted = false;
        _agent.speed = normalSpeed;
        _agent.isStopped = true;

        // 重置状态
        _isPaused = false;
        _isStunned = false;
        _pauseTimer = Random.Range(pauseInterval * 0.5f, pauseInterval);

        Debug.Log("护士长停止追捕");
    }

    /// <summary>
    /// 攻击玩家
    /// </summary>
    private void Attack()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("Attack");
        }

        if (attackSound != null)
        {
            _audioSource.PlayOneShot(attackSound);
        }

        // 这里可以调用玩家受伤逻辑
        // PlayerHealth.Instance?.TakeDamage(1);

        // 攻击后短暂停顿
        _isPaused = true;
        _pauseTimer = 1f;

        Debug.Log("护士长攻击玩家");
    }

    /// <summary>
    /// 被月光透镜照射
    /// </summary>
    public void OnHitByMoonlight()
    {
        if (_isStunned) return;

        // 播放被照射音效
        if (stunnedSound != null)
        {
            _audioSource.PlayOneShot(stunnedSound);
        }

        // 播放被照射动画
        if (_animator != null)
        {
            _animator.SetTrigger("Stunned");
        }

        // 眩晕效果
        _isStunned = true;
        _stunnedTimer = stunnedDuration;

        // 停止移动
        _agent.isStopped = true;

        Debug.Log("护士长被月光照射，眩晕 " + stunnedDuration + " 秒");
    }

    /// <summary>
    /// 处理减速效果
    /// </summary>
    private void HandleSlowEffect()
    {
        if (_slowTimer > 0)
        {
            _slowTimer -= Time.deltaTime;
            if (_slowTimer <= 0)
            {
                // 恢复速度
                _agent.speed = chaseSpeed;
            }
        }
    }

    /// <summary>
    /// 处理眩晕效果
    /// </summary>
    private void HandleStunnedEffect()
    {
        if (_isStunned)
        {
            _stunnedTimer -= Time.deltaTime;
            if (_stunnedTimer <= 0)
            {
                _isStunned = false;
                _agent.isStopped = false;
                Debug.Log("护士长恢复行动");
            }
        }
    }

    /// <summary>
    /// 应用减速效果（由月光透镜调用）
    /// </summary>
    public void ApplySlow()
    {
        if (_isStunned) return;

        _agent.speed = chaseSpeed * slowMultiplier;
        _slowTimer = slowDuration;
    }

    /// <summary>
    /// 更新动画
    /// </summary>
    private void UpdateAnimation()
    {
        if (_animator == null) return;

        float speed = _agent.velocity.magnitude;
        _animator.SetFloat("Speed", speed);
        _animator.SetBool("IsChasing", isChasing);
        _animator.SetBool("IsStunned", _isStunned);
    }

    /// <summary>
    /// 设置追捕状态（供外部调用）
    /// </summary>
    public void SetChasing(bool chasing)
    {
        if (chasing)
        {
            StartChase();
        }
        else
        {
            StopChase();
        }
    }

    /// <summary>
    /// 重置追捕者（用于重新开始）
    /// </summary>
    public void ResetChaser()
    {
        StopChase();
        _isStunned = false;
        _isPaused = false;
        _stunnedTimer = 0;
        _slowTimer = 0;
        _hasSpotted = false;

        if (_agent != null)
        {
            _agent.speed = normalSpeed;
            _agent.isStopped = true;
        }
    }

    /// <summary>
    /// 获取当前是否在追捕中
    /// </summary>
    public bool IsChasing()
    {
        return isChasing;
    }

    /// <summary>
    /// 获取当前是否眩晕
    /// </summary>
    public bool IsStunned()
    {
        return _isStunned;
    }

    // 绘制Gizmos（调试用）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseStartDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseStopDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}