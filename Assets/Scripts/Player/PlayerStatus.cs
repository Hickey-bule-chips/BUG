using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("精神值设置")]
    [SerializeField] private float maxMentalHealth = 100f; // 最大精神值
    [SerializeField] private float mentalHealthDrainRate = 20f; // 精神值消耗速度（每秒）
    [SerializeField] private float mentalHealthRecoveryRate = 15f; // 精神值恢复速度（每秒）
    [SerializeField] private float minMentalHealth = 0f; // 最小精神值
    
    [Header("健康值设置")]
    [SerializeField] private float maxHealth = 100f; // 最大健康值
    [SerializeField] private float minHealth = 0f; // 最小健康值
    [SerializeField] private float healthLossOnMentalDepletion = 50f; // 精神值耗尽时失去的健康值百分比
    
    [Header("状态")]
    [SerializeField] private float currentMentalHealth; // 当前精神值
    [SerializeField] private float currentHealth; // 当前健康值
    [SerializeField] private bool isDraining = false; // 是否正在消耗精神值
    [SerializeField] private bool isRecovering = false; // 是否正在恢复精神值
    [SerializeField] private bool isDead = false; // 是否已死亡
    [SerializeField] private bool hasLostHealthFromMentalDepletion = false; // 是否已因精神值耗尽而失去健康值
    
    // 事件
    public System.Action<float> OnMentalHealthChanged; // 精神值变化事件
    public System.Action OnMentalHealthDepleted; // 精神值耗尽事件
    public System.Action OnMentalHealthRestored; // 精神值恢复事件
    public System.Action<float> OnHealthChanged; // 健康值变化事件
    public System.Action OnHealthDepleted; // 健康值耗尽事件
    public System.Action OnPlayerDeath; // 玩家死亡事件
    
    void Start()
    {
        // 初始化精神值为最大值
        currentMentalHealth = maxMentalHealth;
        OnMentalHealthChanged?.Invoke(currentMentalHealth);
        
        // 初始化健康值为最大值
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    void Update()
    {
        if (isDraining)
        {
            // 消耗精神值
            DrainMentalHealth();
        }
        else if (isRecovering)
        {
            // 恢复精神值（只有在主动停止消耗时才开始恢复）
            RecoverMentalHealth();
        }
    }
    
    /// <summary>
    /// 开始消耗精神值
    /// </summary>
    public void StartDraining()
    {
        isDraining = true;
        isRecovering = false; // 停止恢复
        Debug.Log("开始消耗精神值");
    }
    
    /// <summary>
    /// 停止消耗精神值并开始恢复
    /// </summary>
    public void StopDraining()
    {
        isDraining = false;
        isRecovering = true; // 开始恢复
        Debug.Log("停止消耗精神值，开始恢复");
    }
    
    /// <summary>
    /// 消耗精神值
    /// </summary>
    private void DrainMentalHealth()
    {
        if (currentMentalHealth > minMentalHealth)
        {
            currentMentalHealth -= mentalHealthDrainRate * Time.deltaTime;
            currentMentalHealth = Mathf.Max(currentMentalHealth, minMentalHealth);
            OnMentalHealthChanged?.Invoke(currentMentalHealth);
            
            // 检查是否耗尽
            if (currentMentalHealth <= minMentalHealth)
            {
                OnMentalHealthDepleted?.Invoke();
                Debug.Log("精神值耗尽！");
                
                // 如果还没有因精神值耗尽而失去健康值，则减少健康值
                if (!hasLostHealthFromMentalDepletion)
                {
                    LoseHealthFromMentalDepletion();
                }
                
                // 精神值耗尽时停止消耗，并自动开始恢复
                isDraining = false;
                isRecovering = true;
            }
        }
    }
    
    /// <summary>
    /// 恢复精神值
    /// </summary>
    private void RecoverMentalHealth()
    {
        if (currentMentalHealth < maxMentalHealth)
        {
            currentMentalHealth += mentalHealthRecoveryRate * Time.deltaTime;
            currentMentalHealth = Mathf.Min(currentMentalHealth, maxMentalHealth);
            OnMentalHealthChanged?.Invoke(currentMentalHealth);
            
            // 检查是否完全恢复
            if (currentMentalHealth >= maxMentalHealth)
            {
                OnMentalHealthRestored?.Invoke();
                Debug.Log("精神值完全恢复！");
                
                // 停止恢复
                isRecovering = false;
                
                // 重置健康值损失标记，允许下次精神值耗尽时再次失去健康值
                ResetHealthLossFlag();
            }
        }
    }
    
    /// <summary>
    /// 获取当前精神值百分比
    /// </summary>
    public float GetMentalHealthPercentage()
    {
        return currentMentalHealth / maxMentalHealth;
    }
    
    /// <summary>
    /// 获取当前精神值
    /// </summary>
    public float GetCurrentMentalHealth()
    {
        return currentMentalHealth;
    }
    
    /// <summary>
    /// 获取最大精神值
    /// </summary>
    public float GetMaxMentalHealth()
    {
        return maxMentalHealth;
    }
    
    /// <summary>
    /// 检查是否有足够的精神值
    /// </summary>
    public bool HasEnoughMentalHealth(float requiredAmount = 0f)
    {
        return currentMentalHealth > requiredAmount;
    }
    
    /// <summary>
    /// 设置精神值（用于调试或特殊效果）
    /// </summary>
    public void SetMentalHealth(float value)
    {
        currentMentalHealth = Mathf.Clamp(value, minMentalHealth, maxMentalHealth);
        OnMentalHealthChanged?.Invoke(currentMentalHealth);
    }
    
    /// <summary>
    /// 恢复精神值到最大值
    /// </summary>
    public void RestoreFullMentalHealth()
    {
        currentMentalHealth = maxMentalHealth;
        OnMentalHealthChanged?.Invoke(currentMentalHealth);
        OnMentalHealthRestored?.Invoke();
    }
    
    /// <summary>
    /// 因精神值耗尽而失去健康值
    /// </summary>
    private void LoseHealthFromMentalDepletion()
    {
        if (isDead) return; // 如果已经死亡，不再处理
        
        float healthLoss = maxHealth * (healthLossOnMentalDepletion / 100f);
        currentHealth -= healthLoss;
        currentHealth = Mathf.Max(currentHealth, minHealth);
        
        hasLostHealthFromMentalDepletion = true;
        OnHealthChanged?.Invoke(currentHealth);
        
        Debug.Log($"因精神值耗尽失去 {healthLoss} 点健康值，当前健康值: {currentHealth}");
        
        // 检查是否死亡
        if (currentHealth <= minHealth)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return; // 如果已经死亡，不再处理
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, minHealth);
        OnHealthChanged?.Invoke(currentHealth);
        
        Debug.Log($"受到 {damage} 点伤害，当前健康值: {currentHealth}");
        
        // 检查是否死亡
        if (currentHealth <= minHealth)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 恢复健康值
    /// </summary>
    public void Heal(float healAmount)
    {
        if (isDead) return; // 如果已经死亡，不再处理
        
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
        
        Debug.Log($"恢复 {healAmount} 点健康值，当前健康值: {currentHealth}");
    }
    
    /// <summary>
    /// 玩家死亡
    /// </summary>
    private void Die()
    {
        if (isDead) return; // 防止重复死亡
        
        isDead = true;
        currentHealth = minHealth;
        
        Debug.Log("玩家死亡！");
        
        // 触发死亡事件
        OnHealthDepleted?.Invoke();
        OnPlayerDeath?.Invoke();
        
        // 停止所有移动和攻击
        StopAllPlayerActions();
    }
    
    /// <summary>
    /// 停止所有玩家行为
    /// </summary>
    private void StopAllPlayerActions()
    {
        // 停止移动
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        // 停止精神值消耗
        isDraining = false;
        
        // 禁用玩家控制器
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // 禁用攻击系统
        PlayerAttack playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }
        
        // 禁用能力管理器
        PlayerAbilityManager abilityManager = GetComponent<PlayerAbilityManager>();
        if (abilityManager != null)
        {
            abilityManager.enabled = false;
        }
    }
    
    /// <summary>
    /// 重置健康值损失标记（当精神值完全恢复时调用）
    /// </summary>
    public void ResetHealthLossFlag()
    {
        hasLostHealthFromMentalDepletion = false;
    }
    
    /// <summary>
    /// 获取当前健康值
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    /// <summary>
    /// 获取最大健康值
    /// </summary>
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    
    /// <summary>
    /// 获取健康值百分比
    /// </summary>
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    /// <summary>
    /// 检查是否死亡
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
    
    /// <summary>
    /// 复活玩家（用于调试或特殊功能）
    /// </summary>
    public void Resurrect()
    {
        isDead = false;
        currentHealth = maxHealth;
        currentMentalHealth = maxMentalHealth;
        hasLostHealthFromMentalDepletion = false;
        
        OnHealthChanged?.Invoke(currentHealth);
        OnMentalHealthChanged?.Invoke(currentMentalHealth);
        
        // 重新启用组件
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        PlayerAttack playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.enabled = true;
        }
        
        PlayerAbilityManager abilityManager = GetComponent<PlayerAbilityManager>();
        if (abilityManager != null)
        {
            abilityManager.enabled = true;
        }
        
        Debug.Log("玩家复活！");
    }
}