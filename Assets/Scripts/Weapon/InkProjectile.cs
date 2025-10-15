using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkProjectile : MonoBehaviour
{
    [Header("墨水设置")]
    [SerializeField] private float lifetime = 3f; // 生命周期
    
    [Header("抛物线弹道设置")]
    [SerializeField] private bool useParabolicTrajectory = true; // 是否使用抛物线弹道
    [SerializeField] private float gravityScale = 1f; // 重力缩放（1 = 正常重力）
    
    [Header("效果")]
    [SerializeField] private GameObject splashEffectPrefab; // 飞溅特效
    
    private GameObject platformPrefab; // 平台预制体
    private float platformDuration; // 平台存在时间
    private bool hasHit = false; // 是否已击中
    private Rigidbody2D rb;
    private float timer = 0f;
    private string poolName = ""; // 对象池名称
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void OnEnable()
    {
        // 重置状态
        hasHit = false;
        timer = 0f;
        
        // 设置重力
        if (rb != null)
        {
            rb.gravityScale = useParabolicTrajectory ? gravityScale : 0f;
        }
    }
    
    private void Update()
    {
        timer += Time.deltaTime;
        
        // 超时归还到池
        if (timer >= lifetime)
        {
            // 如果墨水自然消失（没有击中任何物体），在当前位置生成平台
            if (!hasHit)
            {
                Debug.Log($"墨水自然消失，在位置 {transform.position} 生成平台");
                CreatePlatform(transform.position);
                SpawnSplashEffect();
            }
            
            ReturnToPool();
        }
    }
    
    /// <summary>
    /// 设置平台预制体和存在时间
    /// </summary>
    public void SetPlatformPrefab(GameObject prefab, float duration)
    {
        platformPrefab = prefab;
        platformDuration = duration;
    }
    
    /// <summary>
    /// 设置对象池名称
    /// </summary>
    public void SetPoolName(string name)
    {
        poolName = name;
    }
    
    /// <summary>
    /// 归还到对象池或销毁
    /// </summary>
    private void ReturnToPool()
    {
        if (!string.IsNullOrEmpty(poolName) && PoolManager.Instance != null)
        {
            PoolManager.Instance.Despawn(poolName, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 防止重复触发
        if (hasHit)
            return;
        
        Debug.Log($"墨水碰撞检测: {collision.name}, 标签: {collision.tag}");
        
        // 忽略玩家
        if (collision.CompareTag("Player"))
        {
            Debug.Log("忽略玩家碰撞");
            return;
        }
        
        // 击中任何物体都生成平台
        hasHit = true;
        
        Debug.Log($"墨水击中物体: {collision.name}, 位置: {collision.transform.position}");
        
        // 在墨水当前位置生成平台
        CreatePlatform(transform.position);
        
        // 生成飞溅特效
        SpawnSplashEffect();
        
        // 归还到对象池
        ReturnToPool();
    }
    
    private void CreatePlatform(Vector3 position)
    {
        Debug.Log($"在墨水消失位置 {position} 创建平台");
        
        if (platformPrefab == null)
        {
            Debug.LogError("墨水平台预制体未设置！请检查PlayerAttack组件中的Ink Platform Prefab设置");
            return;
        }
        
        // 实例化平台
        GameObject platform = Instantiate(platformPrefab, position, Quaternion.identity);
        
        // 将平台递归设置到 Ground 层，并确保碰撞器为非触发器，便于地面检测
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer >= 0)
        {
            SetLayerRecursively(platform, groundLayer);
        }
        SetCollidersToNonTrigger(platform);
        
        if (platform == null)
        {
            Debug.LogError("平台实例化失败！");
            return;
        }
        
        Debug.Log($"平台创建成功: {platform.name} 在位置 {position}");
        
        // 设置平台的存在时间
        InkPlatform inkPlatform = platform.GetComponent<InkPlatform>();
        if (inkPlatform != null)
        {
            inkPlatform.SetDuration(platformDuration);
        }
        else
        {
            Debug.LogWarning($"平台预制体 {platformPrefab.name} 没有InkPlatform脚本，将使用定时销毁");
            // 如果没有InkPlatform脚本，直接定时销毁
            Destroy(platform, platformDuration);
        }
        
        Debug.Log($"墨水平台创建完成，持续 {platformDuration} 秒");
    }
    
    /// <summary>
    /// 递归设置物体及其所有子节点的层
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;
        obj.layer = layer;
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            Transform child = obj.transform.GetChild(i);
            if (child != null)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
    
    /// <summary>
    /// 将所有 Collider2D 设为非触发器，确保可被 OverlapCircle 当作地面检测
    /// </summary>
    private void SetCollidersToNonTrigger(GameObject obj)
    {
        if (obj == null) return;
        var colliders = obj.GetComponentsInChildren<Collider2D>(true);
        foreach (var col in colliders)
        {
            col.isTrigger = false;
        }
    }
    
    private void SpawnSplashEffect()
    {
        if (splashEffectPrefab != null)
        {
            Instantiate(splashEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}

