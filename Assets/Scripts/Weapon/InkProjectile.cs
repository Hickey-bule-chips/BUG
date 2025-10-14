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
        
        // 忽略玩家
        if (collision.CompareTag("Player"))
            return;
        
        // 检测是否击中null方块（空方块）
        if (collision.CompareTag("NullBlock") || collision.name.Contains("Null"))
        {
            hasHit = true;
            
            Debug.Log($"墨水击中空方块: {collision.name}");
            
            // 记录方块位置
            Vector3 blockPosition = collision.transform.position;
            
            // 销毁null方块
            Destroy(collision.gameObject);
            
            // 在该位置生成平台
            CreatePlatform(blockPosition);
            
            // 生成飞溅特效
            SpawnSplashEffect();
            
            // 归还到对象池
            ReturnToPool();
        }
        // 如果击中其他物体，也销毁墨水
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        {
            hasHit = true;
            
            // 生成飞溅特效
            SpawnSplashEffect();
            
            // 归还到对象池
            ReturnToPool();
        }
    }
    
    private void CreatePlatform(Vector3 position)
    {
        if (platformPrefab == null)
        {
            Debug.LogWarning("墨水平台预制体未设置！");
            return;
        }
        
        // 实例化平台
        GameObject platform = Instantiate(platformPrefab, position, Quaternion.identity);
        
        // 设置平台的存在时间
        InkPlatform inkPlatform = platform.GetComponent<InkPlatform>();
        if (inkPlatform != null)
        {
            inkPlatform.SetDuration(platformDuration);
        }
        else
        {
            // 如果没有InkPlatform脚本，直接定时销毁
            Destroy(platform, platformDuration);
        }
        
        Debug.Log($"生成墨水平台，持续 {platformDuration} 秒");
    }
    
    private void SpawnSplashEffect()
    {
        if (splashEffectPrefab != null)
        {
            Instantiate(splashEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}

