using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("笔芯攻击设置")]
    [SerializeField] private GameObject penCorePrefab; // 笔芯预制体
    [SerializeField] private float penCoreSpeed = 15f; // 笔芯速度
    [SerializeField] private float penCoreCooldown = 0.3f; // 笔芯冷却时间
    [SerializeField] private int penCoreDamage = 10; // 笔芯伤害
    
    [Header("墨水攻击设置")]
    [SerializeField] private GameObject inkPrefab; // 墨水预制体
    [SerializeField] private float inkSpeed = 12f; // 墨水速度
    [SerializeField] private float inkCooldown = 1f; // 墨水冷却时间
    [SerializeField] private GameObject inkPlatformPrefab; // 墨水平台预制体
    [SerializeField] private float platformDuration = 5f; // 平台存在时间
    
    [Header("发射设置")]
    [SerializeField] private Transform firePoint; // 发射点
    [SerializeField] private float firePointOffset = 0.5f; // 发射点偏移距离
    
    [Header("对象池设置")]
    [SerializeField] private bool useObjectPool = true; // 是否使用对象池
    [SerializeField] private int poolInitialSize = 20; // 对象池初始大小
    [SerializeField] private int poolMaxSize = 50; // 对象池最大大小
    
    // 冷却计时器
    private float penCoreTimer = 0f;
    private float inkTimer = 0f;
    
    // 组件引用
    private Camera mainCamera;
    
    // 对象池名称
    private const string PEN_CORE_POOL = "PenCorePool";
    private const string INK_POOL = "InkPool";
    
    void Start()
    {
        // 获取主摄像机
        mainCamera = Camera.main;
        
        // 如果没有设置发射点，创建一个
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.parent = transform;
            firePointObj.transform.localPosition = new Vector3(firePointOffset, 0, 0);
            firePoint = firePointObj.transform;
        }
        
        // 初始化对象池
        if (useObjectPool)
        {
            InitializeObjectPools();
        }
    }
    
    /// <summary>
    /// 初始化对象池
    /// </summary>
    private void InitializeObjectPools()
    {
        if (PoolManager.Instance != null)
        {
            // 创建笔芯对象池
            if (penCorePrefab != null)
            {
                PoolManager.Instance.CreatePool(PEN_CORE_POOL, penCorePrefab, poolInitialSize, poolMaxSize);
            }
            
            // 创建墨水对象池
            if (inkPrefab != null)
            {
                PoolManager.Instance.CreatePool(INK_POOL, inkPrefab, poolInitialSize, poolMaxSize);
            }
        }
        else
        {
            Debug.LogWarning("PoolManager 未找到！将使用 Instantiate/Destroy 方式。");
            useObjectPool = false;
        }
    }
    
    void Update()
    {
        // 更新冷却计时器
        if (penCoreTimer > 0)
            penCoreTimer -= Time.deltaTime;
        if (inkTimer > 0)
            inkTimer -= Time.deltaTime;
    }
    
    /// <summary>
    /// 发射笔芯攻击
    /// </summary>
    /// <param name="direction">发射方向（已归一化）</param>
    public void FirePenCore(Vector2 direction)
    {
        // 检查冷却
        if (penCoreTimer > 0)
            return;
        
        // 检查预制体
        if (penCorePrefab == null)
        {
            Debug.LogWarning("笔芯预制体未设置！");
            return;
        }
        
        // 获取笔芯对象（从对象池或实例化）
        GameObject penCore;
        if (useObjectPool && PoolManager.Instance != null)
        {
            // 从对象池获取
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            penCore = PoolManager.Instance.Spawn(PEN_CORE_POOL, firePoint.position, Quaternion.Euler(0, 0, angle));
        }
        else
        {
            // 直接实例化
            penCore = Instantiate(penCorePrefab, firePoint.position, Quaternion.identity);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            penCore.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        if (penCore == null)
            return;
        
        // 设置笔芯速度
        Rigidbody2D rb = penCore.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * penCoreSpeed;
        }
        
        // 设置笔芯伤害和对象池名称
        PenCoreProjectile penCoreScript = penCore.GetComponent<PenCoreProjectile>();
        if (penCoreScript != null)
        {
            penCoreScript.SetDamage(penCoreDamage);
            if (useObjectPool)
            {
                penCoreScript.SetPoolName(PEN_CORE_POOL);
            }
        }
        
        // 重置冷却
        penCoreTimer = penCoreCooldown;
    }
    
    /// <summary>
    /// 发射墨水
    /// </summary>
    /// <param name="direction">发射方向（已归一化）</param>
    public void FireInk(Vector2 direction)
    {
        // 检查冷却
        if (inkTimer > 0)
            return;
        
        // 检查预制体
        if (inkPrefab == null)
        {
            Debug.LogWarning("墨水预制体未设置！");
            return;
        }
        
        // 获取墨水对象（从对象池或实例化）
        GameObject ink;
        if (useObjectPool && PoolManager.Instance != null)
        {
            // 从对象池获取
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            ink = PoolManager.Instance.Spawn(INK_POOL, firePoint.position, Quaternion.Euler(0, 0, angle));
        }
        else
        {
            // 直接实例化
            ink = Instantiate(inkPrefab, firePoint.position, Quaternion.identity);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            ink.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        if (ink == null)
            return;
        
        // 设置墨水速度
        Rigidbody2D rb = ink.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * inkSpeed;
        }
        
        // 设置墨水的平台预制体、存在时间和对象池名称
        InkProjectile inkScript = ink.GetComponent<InkProjectile>();
        if (inkScript != null)
        {
            inkScript.SetPlatformPrefab(inkPlatformPrefab, platformDuration);
            if (useObjectPool)
            {
                inkScript.SetPoolName(INK_POOL);
            }
        }
        
        // 重置冷却
        inkTimer = inkCooldown;
        
        Debug.Log($"发射墨水！方向: {direction}");
    }
    
    /// <summary>
    /// 检查笔芯是否可以发射
    /// </summary>
    public bool CanFirePenCore()
    {
        return penCoreTimer <= 0 && penCorePrefab != null;
    }
    
    /// <summary>
    /// 检查墨水是否可以发射
    /// </summary>
    public bool CanFireInk()
    {
        return inkTimer <= 0 && inkPrefab != null;
    }
    
    /// <summary>
    /// 获取笔芯冷却进度 (0-1)
    /// </summary>
    public float GetPenCoreCooldownProgress()
    {
        return 1f - (penCoreTimer / penCoreCooldown);
    }
    
    /// <summary>
    /// 获取墨水冷却进度 (0-1)
    /// </summary>
    public float GetInkCooldownProgress()
    {
        return 1f - (inkTimer / inkCooldown);
    }
    
    /// <summary>
    /// 获取鼠标方向（从玩家到鼠标位置的归一化向量）
    /// </summary>
    public Vector2 GetMouseDirection()
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
    
    // 在编辑器中显示发射点
    private void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(firePoint.position, 0.15f);
            
            // 绘制一条指向鼠标的线（仅在运行时）
            if (Application.isPlaying && mainCamera != null)
            {
                Vector2 direction = GetMouseDirection();
                if (direction != Vector2.zero)
                {
                    Gizmos.DrawLine(firePoint.position, firePoint.position + (Vector3)(direction * 2f));
                }
            }
        }
    }
}

