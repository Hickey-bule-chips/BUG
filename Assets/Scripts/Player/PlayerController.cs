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
    private Camera mainCamera; // 主摄像机
    
    // 状态变量
    private float horizontalInput;
    private bool isGrounded;
    private int jumpCount;
    
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
        mainCamera = Camera.main;
        
        // 如果没有设置地面检测点，创建一个
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.parent = transform;
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }

    void Update()
    {
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
        
        // 更新动画
        UpdateAnimation();
    }
    
    void FixedUpdate()
    {
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
        // 计算实际移动方向（如果反向则取反）
        float actualInput = isReversed ? -horizontalInput : horizontalInput;
        
        // 应用水平移动
        rb.velocity = new Vector2(actualInput * moveSpeed, rb.velocity.y);
        
        // 角色翻转（根据实际移动方向）
        if (actualInput > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (actualInput < 0)
        {
            spriteRenderer.flipX = true;
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
    
    private void UpdateAnimation()
    {
        // 如果有Animator组件，更新动画参数
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VelocityY", rb.velocity.y);
            animator.SetBool("IsReversed", isReversed); // 可用于反向时的特殊动画效果
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
        
        // 鼠标左键 - 发射笔芯
        if (Input.GetMouseButton(0))
        {
            Vector2 direction = GetMouseDirection();
            if (direction != Vector2.zero)
            {
                playerAttack.FirePenCore(direction);
            }
        }
        
        // 鼠标右键 - 发射墨水
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 direction = GetMouseDirection();
            if (direction != Vector2.zero)
            {
                playerAttack.FireInk(direction);
            }
        }
    }
    
    /// <summary>
    /// 获取鼠标方向（从玩家到鼠标位置的归一化向量）
    /// </summary>
    private Vector2 GetMouseDirection()
    {
        if (mainCamera == null)
            return Vector2.zero;
        
        // 获取鼠标在世界坐标中的位置
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        
        // 计算从玩家到鼠标的方向向量
        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        
        return direction;
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
        if (Application.isPlaying && mainCamera != null)
        {
            Vector2 mouseDir = GetMouseDirection();
            if (mouseDir != Vector2.zero)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)(mouseDir * 2f));
            }
        }
    }
}
