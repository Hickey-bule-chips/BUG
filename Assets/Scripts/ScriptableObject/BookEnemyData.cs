using UnityEngine;

/// <summary>
/// 书本怪数据ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "BookEnemyData", menuName = "Enemy/Book Enemy Data")]
public class BookEnemyData : ScriptableObject
{
    [Header("基础设置")]
    [Tooltip("书本怪名称")]
    public string enemyName = "书本怪";
    
    [Tooltip("书本怪最大血量")]
    public float maxHealth = 50f;
    
    [Tooltip("书本怪对玩家的伤害")]
    public float damage = 10f;
    
    [Header("移动设置")]
    [Tooltip("书本怪移动速度")]
    public float moveSpeed = 3f;
    
    [Tooltip("追随玩家的距离")]
    public float followDistance = 8f;
    
    [Tooltip("停止追随的距离")]
    public float stopFollowDistance = 12f;
    
    [Header("攻击设置")]
    [Tooltip("攻击间隔时间")]
    public float attackInterval = 1f;
    
    [Tooltip("接触伤害间隔时间")]
    public float contactDamageInterval = 0.5f;
    
    [Header("视觉效果")]
    [Tooltip("书本怪精灵")]
    public Sprite enemySprite;
    
    [Tooltip("死亡特效预制体")]
    public GameObject deathEffect;
    
    [Tooltip("死亡音效")]
    public AudioClip deathSound;
    
    [Header("碰撞设置")]
    [Tooltip("碰撞检测半径")]
    public float colliderRadius = 0.5f;
    
    [Tooltip("是否使用触发器")]
    public bool isTrigger = true;
}
