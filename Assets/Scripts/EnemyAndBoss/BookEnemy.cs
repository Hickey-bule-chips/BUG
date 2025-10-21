using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 书本怪核心脚本
/// </summary>
public class BookEnemy : MonoBehaviour
{
    [Header("数据设置")]
    [SerializeField] private BookEnemyData enemyData;
    
    [Header("组件引用")]
    private Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private CircleCollider2D triggerCollider; // 用于检测玩家接触的触发器
    private Animator animator;
    private AudioSource audioSource;
    
    [Header("状态变量")]
    private float currentHealth;
    private Transform player;
    private bool isFollowingPlayer = false;
    private bool isDead = false;
    private Vector2 currentDirection;
    private float lastContactDamageTime = 0f;
    private float collisionCooldown = 0.5f; // 碰撞冷却时间
    private float lastCollisionTime = 0f; // 上次碰撞时间
    
    [Header("对象池设置")]
    private string poolName = "";
    private bool useObjectPool = false;
    
    // 事件
    public System.Action<BookEnemy> OnEnemyDeath;
    
    void Awake()
    {
        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        // 如果没有音频源，添加一个
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void Start()
    {
        // 初始化数据
        InitializeEnemy();
        
        // 查找玩家
        FindPlayer();
    }
    
    protected virtual void Update()
    {
        if (isDead) return;
        
        // 检查玩家是否死亡
        if (player != null)
        {
            PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
            if (playerStatus != null && playerStatus.IsDead())
            {
                // 玩家死亡，停止追随
                isFollowingPlayer = false;
                return;
            }
        }
        
        // 更新追随逻辑
        UpdateFollowLogic();
        
        // 更新移动
        UpdateMovement();
    }
    
    /// <summary>
    /// 初始化敌人数据
    /// </summary>
    private void InitializeEnemy()
    {
        if (enemyData == null)
        {
            Debug.LogError("BookEnemy: 未设置敌人数据！");
            return;
        }
        
        // 设置血量
        currentHealth = enemyData.maxHealth;
        
        // 设置精灵
        if (spriteRenderer != null && enemyData.enemySprite != null)
        {
            spriteRenderer.sprite = enemyData.enemySprite;
        }
        
        // 设置主碰撞器（用于物理阻挡）
        if (circleCollider != null)
        {
            circleCollider.radius = enemyData.colliderRadius;
            circleCollider.isTrigger = false; // 物理碰撞，用于阻挡
        }
        
        // 创建触发器碰撞器（用于检测玩家接触）
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        
        if (triggerCollider != null)
        {
            triggerCollider.radius = enemyData.colliderRadius * 1.1f; // 稍微大一点，确保能检测到接触
            triggerCollider.isTrigger = true; // 触发器，用于检测玩家
        }
        
        // 设置刚体
        if (rb != null)
        {
            rb.gravityScale = 0f; // 书本怪不受重力影响
        }
        
        Debug.Log($"BookEnemy初始化完成: {enemyData.enemyName}, 血量: {currentHealth}");
    }
    
    /// <summary>
    /// 查找玩家
    /// </summary>
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("BookEnemy: 未找到玩家！");
        }
    }
    
    /// <summary>
    /// 更新追随逻辑
    /// </summary>
    private void UpdateFollowLogic()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 检查是否应该开始追随
        if (!isFollowingPlayer && distanceToPlayer <= enemyData.followDistance)
        {
            isFollowingPlayer = true;
            Debug.Log($"{enemyData.enemyName} 开始追随玩家");
        }
        // 检查是否应该停止追随
        else if (isFollowingPlayer && distanceToPlayer > enemyData.stopFollowDistance)
        {
            isFollowingPlayer = false;
            Debug.Log($"{enemyData.enemyName} 停止追随玩家");
        }
    }
    
    /// <summary>
    /// 更新移动
    /// </summary>
    private void UpdateMovement()
    {
        if (rb == null) return;
        
        // 检查是否在碰撞冷却时间内
        bool isInCollisionCooldown = Time.time - lastCollisionTime < collisionCooldown;
        
        if (isFollowingPlayer && player != null && !isInCollisionCooldown)
        {
            // 计算朝向玩家的方向
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            currentDirection = directionToPlayer;
            
            // 移动向玩家
            rb.velocity = directionToPlayer * enemyData.moveSpeed;
            
            // 翻转精灵
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = directionToPlayer.x < 0;
            }
        }
        else if (isInCollisionCooldown)
        {
            // 在碰撞冷却时间内，使用当前方向移动（避免方向被覆盖）
            rb.velocity = currentDirection * enemyData.moveSpeed;
            
            // 翻转精灵
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = currentDirection.x < 0;
            }
        }
        else
        {
            // 停止移动
            rb.velocity = Vector2.zero;
        }
    }
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"{enemyData.enemyName} 受到 {damage} 点伤害，剩余血量: {currentHealth}");
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 死亡处理
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log($"{enemyData.enemyName} 死亡！");
        
        // 停止移动
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        // 播放死亡音效
        if (audioSource != null && enemyData.deathSound != null)
        {
            audioSource.PlayOneShot(enemyData.deathSound);
        }
        
        // 播放死亡特效
        if (enemyData.deathEffect != null)
        {
            Instantiate(enemyData.deathEffect, transform.position, Quaternion.identity);
        }
        
        // 触发死亡事件
        OnEnemyDeath?.Invoke(this);
        
        // 返回对象池或销毁
        if (useObjectPool && !string.IsNullOrEmpty(poolName))
        {
            // 延迟返回对象池，让音效播放完毕
            StartCoroutine(ReturnToPoolAfterDelay(1f));
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 延迟返回对象池
    /// </summary>
    private IEnumerator ReturnToPoolAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Despawn(poolName, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 设置对象池信息
    /// </summary>
    public void SetPoolInfo(string poolName, bool useObjectPool)
    {
        this.poolName = poolName;
        this.useObjectPool = useObjectPool;
    }
    
    /// <summary>
    /// 设置敌人数据
    /// </summary>
    public void SetEnemyData(BookEnemyData data)
    {
        enemyData = data;
        InitializeEnemy();
    }
    
    /// <summary>
    /// 重置敌人状态（用于对象池）
    /// </summary>
    public virtual void ResetEnemy()
    {
        currentHealth = enemyData != null ? enemyData.maxHealth : 50f;
        isDead = false;
        isFollowingPlayer = false;
        currentDirection = Vector2.zero;
        lastContactDamageTime = 0f;
        
        // 重置刚体
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        // 重新查找玩家
        FindPlayer();
    }
    
    /// <summary>
    /// 获取当前血量
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    /// <summary>
    /// 获取最大血量
    /// </summary>
    public float GetMaxHealth()
    {
        return enemyData != null ? enemyData.maxHealth : 50f;
    }
    
    /// <summary>
    /// 检查是否死亡
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
    
    /// <summary>
    /// 触发器检测 - 处理与玩家的接触（使用触发器碰撞器）
    /// </summary>
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        
        // 检查是否接触到玩家
        if (other.CompareTag("Player"))
        {
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null && !playerStatus.IsDead())
            {
                // 对玩家造成接触伤害
                DealContactDamage(playerStatus);
            }
        }
    }
    
    /// <summary>
    /// 碰撞检测 - 处理与玩家的持续接触
    /// </summary>
    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        if (isDead) return;
        
        // 检查是否持续接触玩家
        if (other.CompareTag("Player"))
        {
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null && !playerStatus.IsDead())
            {
                // 对玩家造成持续接触伤害
                DealContactDamage(playerStatus);
            }
        }
    }
    
    /// <summary>
    /// 对玩家造成接触伤害
    /// </summary>
    protected virtual void DealContactDamage(PlayerStatus playerStatus)
    {
        if (enemyData == null) return;
        
        // 检查伤害间隔
        if (Time.time - lastContactDamageTime >= enemyData.contactDamageInterval)
        {
            playerStatus.TakeDamage(enemyData.damage);
            lastContactDamageTime = Time.time;
            
            Debug.Log($"{enemyData.enemyName} 对玩家造成 {enemyData.damage} 点接触伤害");
        }
    }
    
    /// <summary>
    /// 物理碰撞检测 - 处理与墙体和地面的碰撞（使用主碰撞器）
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查是否撞到墙体、地面或障碍墙
        if (collision.gameObject.CompareTag("Wall") || 
            collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
            collision.gameObject.layer == LayerMask.NameToLayer("ObstacleWall"))
        {
            // 反转移动方向
            currentDirection = -currentDirection;
            
            // 更新碰撞时间，开始碰撞冷却
            lastCollisionTime = Time.time;
            
            Debug.Log($"{enemyData.enemyName} 撞到障碍物，反转方向: {collision.gameObject.name}");
        }
    }
    
    /// <summary>
    /// 持续碰撞检测 - 处理卡在障碍物中的情况
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 检查是否撞到墙体、地面或障碍墙
        if (collision.gameObject.CompareTag("Wall") || 
            collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
            collision.gameObject.layer == LayerMask.NameToLayer("ObstacleWall"))
        {
            // 如果已经过了碰撞冷却时间，再次反转方向
            if (Time.time - lastCollisionTime >= collisionCooldown)
            {
                currentDirection = -currentDirection;
                lastCollisionTime = Time.time;
                Debug.Log($"{enemyData.enemyName} 持续碰撞，再次反转方向: {collision.gameObject.name}");
            }
        }
    }
}
