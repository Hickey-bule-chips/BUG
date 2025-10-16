using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("跳跃设置")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private int maxJumpCount = 2; // 二段跳（1=单跳，2=二段跳）
    
    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("反向操作设置")]
    [SerializeField] private float minTimeBeforeReverse = 5f; // 开始移动后多久触发反向
    [SerializeField] private float maxTimeBeforeReverse = 10f;
    [SerializeField] private float minReverseDuration = 5f; // 反向持续时间
    [SerializeField] private float maxReverseDuration = 15f;
    
    [Header("组件引用")]
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerAttack playerAttack; // 攻击组件
    
    // 状态变量
    private float horizontalInput;
    private bool isGrounded;
    private int jumpCount;
    
    // 攻击状态变量
    private bool isAttackingInk = false;
    private float inkAttackDuration = 0.5f; // 墨水攻击动画持续时间
    private float inkAttackTimer = 0f;
    
    // 死亡状态变量
    private bool isDead = false; // 是否已死亡
    
    // 笔芯攻击控制变量
    private bool isPenCoreAttackActive = false; // 是否正在播放笔芯攻击动画
    private bool canFirePenCore = false; // 是否可以发射笔芯
    private float lastFireTime = 0f; // 上次发射时间
    private float fireCooldown = 0.1f; // 发射冷却时间（防止过于频繁）
    
    // 反向操作变量
    private bool isReversed = false; // 当前是否反向
    private bool hasStartedMoving = false; // 是否已经开始移动
    private float reverseTimer = 0f; // 反向触发倒计时
    private float reverseDurationTimer = 0f; // 反向持续时间倒计时
    private float timeUntilReverse; // 随机生成的触发反向的时间
    private float reverseDuration; // 随机生成的反向持续时间
    
    void Start()
    {
        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerAttack = GetComponent<PlayerAttack>();
        
        // 如果没有设置地面检测点，创建一个
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.parent = transform;
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
        
        // 订阅死亡事件
        PlayerStatus playerStatus = GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            playerStatus.OnPlayerDeath += OnPlayerDeath;
        }
    }

    void Update()
    {
        // 如果玩家已死亡，不处理任何输入
        if (isDead) return;
        
        // 获取输入
        GetInput();
        
        // 处理反向操作计时
        HandleReverseControl();
        
        // 地面检测
        CheckGround();
        
        // 处理跳跃
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
        
        // 处理攻击（鼠标输入）
        HandleMouseAttack();
        
        // 更新攻击状态计时器
        UpdateAttackStates();
        
        // 更新动画
        UpdateAnimation();
    }
    
    void FixedUpdate()
    {
        // 如果玩家已死亡，不处理移动
        if (isDead) return;
        
        // 处理移动
        Move();
    }
    
    private void GetInput()
    {
        // 获取水平输入
        horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // 检测玩家是否开始移动
        if (!hasStartedMoving && Mathf.Abs(horizontalInput) > 0.01f)
        {
            hasStartedMoving = true;
            // 生成随机时间：什么时候触发反向
            timeUntilReverse = Random.Range(minTimeBeforeReverse, maxTimeBeforeReverse);
            reverseTimer = 0f;
            Debug.Log($"玩家开始移动！将在 {timeUntilReverse:F1} 秒后反向操作");
        }
    }
    
    private void Move()
    {
        // 检查必要组件
        if (rb == null)
            return;
        
        // 计算实际移动方向（如果反向则取反）
        float actualInput = isReversed ? -horizontalInput : horizontalInput;
        
        // 应用水平移动
        rb.velocity = new Vector2(actualInput * moveSpeed, rb.velocity.y);
        
        // 角色翻转（根据实际移动方向）
        if (spriteRenderer != null)
        {
            if (actualInput > 0)
            {
                spriteRenderer.flipX = false;
            }
            else if (actualInput < 0)
            {
                spriteRenderer.flipX = true;
            }
        }
    }
    
    private void HandleReverseControl()
    {
        // 如果还没开始移动，不处理
        if (!hasStartedMoving)
            return;
        
        // 如果还没有触发反向
        if (!isReversed)
        {
            reverseTimer += Time.deltaTime;
            
            // 检查是否到了触发反向的时间
            if (reverseTimer >= timeUntilReverse)
            {
                // 触发反向操作
                isReversed = true;
                reverseDuration = Random.Range(minReverseDuration, maxReverseDuration);
                reverseDurationTimer = 0f;
                Debug.Log($"<color=red>操作反向！持续时间: {reverseDuration:F1} 秒</color>");
            }
        }
        else
        {
            // 反向操作进行中，计时
            reverseDurationTimer += Time.deltaTime;
            
            // 检查反向时间是否结束
            if (reverseDurationTimer >= reverseDuration)
            {
                // 恢复正常操作
                isReversed = false;
                hasStartedMoving = false; // 重置，准备下一轮
                Debug.Log("<color=green>操作恢复正常！</color>");
            }
        }
    }
    
    private void Jump()
    {
        // 检查必要组件
        if (rb == null)
            return;
        
        // 如果在地面上，重置跳跃次数
        if (isGrounded)
        {
            jumpCount = 0;
        }
        
        // 如果还有跳跃次数
        if (jumpCount < maxJumpCount)
        {
            // 重置垂直速度再跳跃，使每次跳跃高度一致
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpCount++;
        }
    }
    
    private void CheckGround()
    {
        // 使用圆形检测判断是否在地面上
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // 如果在地面上，重置跳跃次数
        if (isGrounded)
        {
            jumpCount = 0;
        }
    }
    
    /// <summary>
    /// 更新攻击状态计时器
    /// </summary>
    private void UpdateAttackStates()
    {
        // 更新墨水攻击状态计时器
        if (isAttackingInk)
        {
            inkAttackTimer -= Time.deltaTime;
            if (inkAttackTimer <= 0)
            {
                isAttackingInk = false;
            }
        }
        
        // 检测笔芯攻击动画状态
        CheckPenCoreAttackAnimation();
    }
    
    /// <summary>
    /// 检测笔芯攻击动画状态，控制发射时机
    /// </summary>
    private void CheckPenCoreAttackAnimation()
    {
        if (animator == null) return;
        
        // 检查是否正在播放笔芯攻击动画
        bool currentlyAttacking = animator.GetBool("IsAttackingPenCore");
        
        // 如果开始攻击动画
        if (currentlyAttacking && !isPenCoreAttackActive)
        {
            isPenCoreAttackActive = true;
            canFirePenCore = false;
        }
        // 如果停止攻击动画
        else if (!currentlyAttacking && isPenCoreAttackActive)
        {
            isPenCoreAttackActive = false;
            canFirePenCore = false;
        }
        
        // 如果正在播放攻击动画
        if (isPenCoreAttackActive)
        {
            // 获取当前动画的播放进度
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // 检查是否播放到指定进度（50%的进度）
            if (stateInfo.IsName("PenCoreAttack") && stateInfo.normalizedTime >= 0.5f)
            {
                canFirePenCore = true;
            }
        }
    }
    
    /// <summary>
    /// 动画事件：允许发射笔芯（在动画的指定帧调用）
    /// </summary>
    public void AllowPenCoreFire()
    {
        canFirePenCore = true;
    }
    
    /// <summary>
    /// 动画事件：重置笔芯发射状态（在动画结束时调用）
    /// </summary>
    public void ResetPenCoreFire()
    {
        canFirePenCore = false;
    }
    
    private void UpdateAnimation()
    {
        // 如果有Animator组件，更新动画参数
        if (animator != null)
        {
            // 如果死亡，只更新死亡状态
            if (isDead)
            {
                animator.SetBool("IsDead", true);
                return;
            }
            
            animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
            animator.SetBool("IsGrounded", isGrounded);
            
            // 检查rb是否为空再设置VelocityY
            if (rb != null)
            {
                animator.SetFloat("VelocityY", rb.velocity.y);
            }
            
            animator.SetBool("IsReversed", isReversed); // 可用于反向时的特殊动画效果
            
            // 攻击动画状态
            animator.SetBool("IsAttackingPenCore", Input.GetMouseButton(0)); // 左键按住射击笔芯攻击状态
            animator.SetBool("IsAttackingInk", isAttackingInk); // 右键射击墨水攻击状态
        }
    }
    
    /// <summary>
    /// 处理鼠标攻击输入
    /// </summary>
    private void HandleMouseAttack()
    {
        // 检查是否有攻击组件
        if (playerAttack == null)
            return;
        
        // 鼠标左键 - 发射笔芯（按住持续发射，但需要等待动画时机）
        if (Input.GetMouseButton(0))
        {
            // 检查是否可以发射（动画时机 + 冷却时间）
            if (canFirePenCore && Time.time - lastFireTime >= fireCooldown)
            {
                Vector2 direction = playerAttack.GetMouseDirection();
                if (direction != Vector2.zero)
                {
                    playerAttack.FirePenCore(direction);
                    lastFireTime = Time.time; // 更新上次发射时间
                }
            }
        }
        
        // 鼠标右键 - 发射墨水
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 direction = playerAttack.GetMouseDirection();
            if (direction != Vector2.zero)
            {
                playerAttack.FireInk(direction);
                // 触发墨水攻击动画状态
                isAttackingInk = true;
                inkAttackTimer = inkAttackDuration;
            }
        }
    }
    
    
    // 在编辑器中显示地面检测范围
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        // 显示鼠标方向（仅在运行时）
        if (Application.isPlaying && playerAttack != null)
        {
            Vector2 mouseDir = playerAttack.GetMouseDirection();
            if (mouseDir != Vector2.zero)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)(mouseDir * 2f));
            }
        }
    }
    
    /// <summary>
    /// 玩家死亡处理
    /// </summary>
    private void OnPlayerDeath()
    {
        isDead = true;
        Debug.Log("PlayerController: 玩家死亡，停止所有输入处理");
        
        // 停止移动
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        // 播放死亡动画
        PlayDeathAnimation();
    }
    
    /// <summary>
    /// 播放死亡动画
    /// </summary>
    private void PlayDeathAnimation()
    {
        if (animator != null)
        {
            // 设置死亡动画参数
            animator.SetBool("IsDead", true);
            animator.SetTrigger("Death");
            
            Debug.Log("播放死亡动画");
        }
    }
    
    /// <summary>
    /// 获取玩家是否死亡
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
    
    void OnDestroy()
    {
        // 取消订阅事件
        PlayerStatus playerStatus = GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            playerStatus.OnPlayerDeath -= OnPlayerDeath;
        }
    }
}
