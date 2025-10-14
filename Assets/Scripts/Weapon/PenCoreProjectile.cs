using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenCoreProjectile : MonoBehaviour
{
    [Header("笔芯设置")]
    [SerializeField] private float lifetime = 3f; // 生命周期
    [SerializeField] private int damage = 1; // 伤害值
    
    [Header("效果")]
    [SerializeField] private GameObject hitEffectPrefab; // 击中特效
    
    private bool hasHit = false; // 是否已击中
    private float timer = 0f;
    private string poolName = ""; // 对象池名称（如果使用对象池）
    
    private void OnEnable()
    {
        // 重置状态
        hasHit = false;
        timer = 0f;
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
            
            // 对敌人造成伤害（需要敌人有相应的接口或方法）
            // 示例：
            var enemy = collision.GetComponent<IEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            
            Debug.Log($"笔芯击中 {collision.name}，造成 {damage} 点伤害！");
            
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

/// <summary>
/// 敌人接口（示例）
/// </summary>
public interface IEnemy
{
    void TakeDamage(int damage);
}

