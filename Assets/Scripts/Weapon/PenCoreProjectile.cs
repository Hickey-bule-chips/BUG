using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenCoreProjectile : MonoBehaviour
{
    [Header("笔芯设置")]
    [SerializeField] private float lifetime = 3f; // 生命周期
    private int damage = 1; // 伤害值（由PlayerAttack设置）
    
    [Header("效果")]
    [SerializeField] private GameObject hitEffectPrefab; // 击中特效
    
    private bool hasHit = false; // 是否已击中
    private float timer = 0f;
    private string poolName = ""; // 对象池名称（如果使用对象池）
    
    // 组件引用
    private PlayerAbilityManager playerAbilityManager;
    
    private void OnEnable()
    {
        // 重置状态
        hasHit = false;
        timer = 0f;
        
        // 查找PlayerAbilityManager
        FindPlayerAbilityManager();
    }
    
    private void Update()
    {
        timer += Time.deltaTime;
        
        // 超时归还
        if (timer >= lifetime)
        {
            ReturnToPool();
        }
    }
    
    /// <summary>
    /// 设置伤害值
    /// </summary>
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    /// <summary>
    /// 设置对象池名称（如果使用对象池）
    /// </summary>
    public void SetPoolName(string name)
    {
        poolName = name;
    }
    
    /// <summary>
    /// 查找PlayerAbilityManager
    /// </summary>
    private void FindPlayerAbilityManager()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerAbilityManager = playerObj.GetComponent<PlayerAbilityManager>();
        }
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
        
        // 检测是否击中敌人或Boss
        if (collision.CompareTag("Enemy") || collision.CompareTag("Boss"))
        {
            hasHit = true;
            
            // 对敌人造成伤害
            Debug.Log($"笔芯击中 {collision.name}，造成 {damage} 点伤害！");
            
            // 尝试对书本怪造成伤害
            BookEnemy bookEnemy = collision.GetComponent<BookEnemy>();
            if (bookEnemy != null)
            {
                // 检查是否是隐藏书本怪
                HiddenBookEnemy hiddenBookEnemy = bookEnemy as HiddenBookEnemy;
                if (hiddenBookEnemy != null)
                {
                    // 隐藏书本怪只有在修复视觉开启时才能被伤害
                    if (playerAbilityManager != null && playerAbilityManager.IsRepairVisionActive())
                    {
                        bookEnemy.TakeDamage(damage);
                        Debug.Log($"笔芯击中隐藏书本怪，造成 {damage} 点伤害！（修复视觉已开启）");
                    }
                    else
                    {
                        Debug.Log("笔芯击中隐藏书本怪，但修复视觉未开启，无法造成伤害！");
                    }
                }
                else
                {
                    // 普通书本怪总是可以受到伤害
                    bookEnemy.TakeDamage(damage);
                }
            }
            // 尝试对其他类型的敌人造成伤害
            else
            {
                // 这里可以添加其他敌人类型的伤害逻辑
                Debug.Log($"击中敌人 {collision.name}，但未找到对应的伤害处理脚本");
            }
            
            // 生成击中特效
            SpawnHitEffect();
            
            // 归还到对象池
            ReturnToPool();
        }
        // 检测是否击中障碍物
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        {
            hasHit = true;
            
            // 生成击中特效
            SpawnHitEffect();
            
            // 归还到对象池
            ReturnToPool();
        }
    }
    
    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}


