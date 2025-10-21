using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 隐藏书本怪脚本
/// 继承自BookEnemy，添加隐藏/显示功能
/// 依赖PlayerAbilityManager的修复视觉功能
/// </summary>
public class HiddenBookEnemy : BookEnemy
{
    [Header("隐藏设置")]
    [SerializeField] private bool isHidden = true;
    [SerializeField] private bool isPlayerVisionFixed = false;
    
    [Header("视觉效果")]
    [SerializeField] private Color hiddenColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color visibleColor = new Color(1f, 1f, 1f, 1f);
    
    private PlayerAbilityManager playerAbilityManager;
    private SpriteRenderer hiddenEnemyRenderer;
    
    void Start()
    {
        // 获取组件
        hiddenEnemyRenderer = GetComponent<SpriteRenderer>();
        
        // 查找玩家能力管理器
        FindPlayerAbilityManager();
        
        // 初始化隐藏状态
        UpdateVisibility();
    }
    
    protected override void Update()
    {
        // 调用父类Update
        base.Update();
    }
    
    /// <summary>
    /// 查找玩家能力管理器
    /// </summary>
    private void FindPlayerAbilityManager()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerAbilityManager = playerObj.GetComponent<PlayerAbilityManager>();
            if (playerAbilityManager != null)
            {
                // 订阅修复视觉切换事件
                playerAbilityManager.OnRepairVisionToggled += OnRepairVisionToggled;
                
                // 注册到PlayerAbilityManager
                playerAbilityManager.RegisterHiddenBookEnemy(this);
                
                Debug.Log("HiddenBookEnemy: 已订阅PlayerAbilityManager的修复视觉事件并完成注册");
            }
            else
            {
                Debug.LogWarning("HiddenBookEnemy: 玩家对象上未找到PlayerAbilityManager组件！");
            }
        }
        else
        {
            Debug.LogWarning("HiddenBookEnemy: 未找到玩家对象！");
        }
    }
    
    /// <summary>
    /// 修复视觉切换事件处理
    /// </summary>
    public void OnRepairVisionToggled(bool isActive)
    {
        isPlayerVisionFixed = isActive;
        UpdateVisibility();
        
        Debug.Log($"HiddenBookEnemy: 修复视觉状态变化 - {isActive}");
    }
    
    /// <summary>
    /// 更新可见性
    /// </summary>
    private void UpdateVisibility()
    {
        if (hiddenEnemyRenderer == null) return;
        
        if (isPlayerVisionFixed)
        {
            // 玩家修复视觉时，显示隐藏书本怪
            isHidden = false;
            hiddenEnemyRenderer.color = visibleColor;
            hiddenEnemyRenderer.enabled = true;
        }
        else
        {
            // 玩家未修复视觉时，隐藏书本怪
            isHidden = true;
            hiddenEnemyRenderer.color = hiddenColor;
            // 注意：这里不设置hiddenEnemyRenderer.enabled = false，因为隐藏书本怪在隐藏时仍能造成伤害
        }
    }
    
    /// <summary>
    /// 重写碰撞检测 - 隐藏书本怪在隐藏时也能造成伤害
    /// </summary>
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (IsDead()) return;
        
        // 检查是否接触到玩家
        if (other.CompareTag("Player"))
        {
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null && !playerStatus.IsDead())
            {
                // 对玩家造成接触伤害（无论是否隐藏）
                DealContactDamage(playerStatus);
                Debug.Log($"隐藏书本怪对玩家造成伤害 (隐藏状态: {isHidden})");
            }
        }
    }
    
    /// <summary>
    /// 重写碰撞检测 - 隐藏书本怪在隐藏时也能造成持续伤害
    /// </summary>
    protected override void OnTriggerStay2D(Collider2D other)
    {
        if (IsDead()) return;
        
        // 检查是否持续接触玩家
        if (other.CompareTag("Player"))
        {
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null && !playerStatus.IsDead())
            {
                // 对玩家造成持续接触伤害（无论是否隐藏）
                DealContactDamage(playerStatus);
            }
        }
    }
    
    /// <summary>
    /// 重写重置方法，包含隐藏状态重置
    /// </summary>
    public override void ResetEnemy()
    {
        base.ResetEnemy();
        
        // 重置隐藏状态
        isHidden = true;
        isPlayerVisionFixed = false;
        
        // 重新查找玩家能力管理器（因为对象池重用）
        FindPlayerAbilityManager();
        
        // 重置视觉效果
        UpdateVisibility();
    }
    
    /// <summary>
    /// 销毁时取消事件订阅和注册
    /// </summary>
    void OnDestroy()
    {
        if (playerAbilityManager != null)
        {
            // 取消事件订阅
            playerAbilityManager.OnRepairVisionToggled -= OnRepairVisionToggled;
            
            // 取消注册
            playerAbilityManager.UnregisterHiddenBookEnemy(this);
        }
    }
    
    /// <summary>
    /// 获取是否隐藏
    /// </summary>
    public bool IsHidden()
    {
        return isHidden;
    }
    
    /// <summary>
    /// 获取玩家是否修复视觉
    /// </summary>
    public bool IsPlayerVisionFixed()
    {
        return isPlayerVisionFixed;
    }
    
    /// <summary>
    /// 设置隐藏状态（用于调试）
    /// </summary>
    public void SetHidden(bool hidden)
    {
        isHidden = hidden;
        UpdateVisibility();
    }
}
