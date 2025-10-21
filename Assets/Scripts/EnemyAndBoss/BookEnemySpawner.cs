using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 书本怪生成管理器
/// 统一管理普通书本怪和隐藏书本怪的生成
/// </summary>
public class BookEnemySpawner : MonoBehaviour
{
    [Header("生成设置")]
    [SerializeField] private float spawnInterval = 3f; // 生成间隔
    [SerializeField] private int maxEnemies = 10; // 最大敌人数量
    [SerializeField] private float spawnRadius = 8f; // 生成半径
    [SerializeField] private float minDistanceFromPlayer = 3f; // 距离玩家的最小距离
    
    [Header("敌人数据")]
    [SerializeField] private BookEnemyData normalBookEnemyData; // 普通书本怪数据
    [SerializeField] private BookEnemyData hiddenBookEnemyData; // 隐藏书本怪数据
    
    [Header("预制体")]
    [SerializeField] private GameObject normalBookEnemyPrefab; // 普通书本怪预制体
    [SerializeField] private GameObject hiddenBookEnemyPrefab; // 隐藏书本怪预制体
    
    [Header("生成比例")]
    [SerializeField] private float normalEnemyRatio = 0.7f; // 普通书本怪生成比例 (70%)
    // 隐藏书本怪生成比例 = 1 - normalEnemyRatio
    
    [Header("调试")]
    [SerializeField] private bool enableSpawning = true; // 是否启用生成
    [SerializeField] private bool showDebugInfo = true; // 是否显示调试信息
    
    // 私有变量
    private Transform player;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float lastSpawnTime = 0f;
    private bool isInitialized = false;
    
    // 事件
    public System.Action<GameObject> OnEnemySpawned;
    public System.Action<GameObject> OnEnemyDestroyed;
    
    void Start()
    {
        InitializeSpawner();
    }
    
    void Update()
    {
        if (!isInitialized || !enableSpawning) return;
        
        // 检查生成时机
        if (ShouldSpawnEnemy())
        {
            SpawnRandomEnemy();
        }
        
        // 清理已死亡的敌人
        CleanupDeadEnemies();
    }
    
    /// <summary>
    /// 初始化生成器
    /// </summary>
    private void InitializeSpawner()
    {
        // 查找玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("BookEnemySpawner: 未找到玩家！");
            return;
        }
        
        // 初始化对象池（检查是否已存在）
        if (PoolManager.Instance != null)
        {
            // 检查书本怪对象池是否已存在
            if (!PoolManager.Instance.PoolExists("BookEnemy") || !PoolManager.Instance.PoolExists("HiddenBookEnemy"))
            {
                PoolManager.Instance.CreateBookEnemyPools(normalBookEnemyPrefab, hiddenBookEnemyPrefab);
                Debug.Log("BookEnemySpawner: 成功创建书本怪对象池");
            }
            else
            {
                Debug.Log("BookEnemySpawner: 书本怪对象池已存在，跳过创建");
            }
        }
        else
        {
            Debug.LogError("BookEnemySpawner: PoolManager未找到！");
            return;
        }
        
        // 验证数据
        if (normalBookEnemyData == null)
        {
            Debug.LogError("BookEnemySpawner: 普通书本怪数据未设置！");
            return;
        }
        
        if (hiddenBookEnemyData == null)
        {
            Debug.LogError("BookEnemySpawner: 隐藏书本怪数据未设置！");
            return;
        }
        
        isInitialized = true;
        
        if (showDebugInfo)
        {
            Debug.Log("BookEnemySpawner初始化完成");
        }
    }
    
    /// <summary>
    /// 检查是否应该生成敌人
    /// </summary>
    private bool ShouldSpawnEnemy()
    {
        // 检查时间间隔
        if (Time.time - lastSpawnTime < spawnInterval)
        {
            return false;
        }
        
        // 检查敌人数量限制
        if (activeEnemies.Count >= maxEnemies)
        {
            return false;
        }
        
        // 检查玩家是否死亡
        if (player != null)
        {
            PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
            if (playerStatus != null && playerStatus.IsDead())
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 生成随机敌人
    /// </summary>
    private void SpawnRandomEnemy()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        if (spawnPosition == Vector3.zero)
        {
            return; // 无法找到合适的生成位置
        }
        
        // 根据比例决定生成哪种敌人
        float randomValue = Random.Range(0f, 1f);
        bool spawnNormalEnemy = randomValue < normalEnemyRatio;
        
        GameObject newEnemy = null;
        
        if (spawnNormalEnemy)
        {
            newEnemy = SpawnNormalBookEnemy(spawnPosition);
        }
        else
        {
            newEnemy = SpawnHiddenBookEnemy(spawnPosition);
        }
        
        if (newEnemy != null)
        {
            activeEnemies.Add(newEnemy);
            lastSpawnTime = Time.time;
            
            // 订阅死亡事件
            BookEnemy bookEnemy = newEnemy.GetComponent<BookEnemy>();
            if (bookEnemy != null)
            {
                bookEnemy.OnEnemyDeath += OnEnemyDeathHandler;
            }
            
            OnEnemySpawned?.Invoke(newEnemy);
            
            if (showDebugInfo)
            {
                string enemyType = spawnNormalEnemy ? "普通书本怪" : "隐藏书本怪";
                Debug.Log($"生成{enemyType}在位置: {spawnPosition}");
            }
        }
    }
    
    /// <summary>
    /// 生成普通书本怪
    /// </summary>
    private GameObject SpawnNormalBookEnemy(Vector3 position)
    {
        if (PoolManager.Instance != null)
        {
            return PoolManager.Instance.SpawnBookEnemy(position, normalBookEnemyData);
        }
        else
        {
            // 备用方案：直接实例化
            GameObject enemy = Instantiate(normalBookEnemyPrefab, position, Quaternion.identity);
            BookEnemy bookEnemy = enemy.GetComponent<BookEnemy>();
            if (bookEnemy != null)
            {
                bookEnemy.SetEnemyData(normalBookEnemyData);
            }
            return enemy;
        }
    }
    
    /// <summary>
    /// 生成隐藏书本怪
    /// </summary>
    private GameObject SpawnHiddenBookEnemy(Vector3 position)
    {
        if (PoolManager.Instance != null)
        {
            return PoolManager.Instance.SpawnHiddenBookEnemy(position, hiddenBookEnemyData);
        }
        else
        {
            // 备用方案：直接实例化
            GameObject enemy = Instantiate(hiddenBookEnemyPrefab, position, Quaternion.identity);
            HiddenBookEnemy hiddenBookEnemy = enemy.GetComponent<HiddenBookEnemy>();
            if (hiddenBookEnemy != null)
            {
                hiddenBookEnemy.SetEnemyData(hiddenBookEnemyData);
            }
            return enemy;
        }
    }
    
    /// <summary>
    /// 获取随机生成位置
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        if (player == null) return Vector3.zero;
        
        Vector3 playerPosition = player.position;
        int maxAttempts = 10;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            // 在圆形区域内随机生成位置
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 candidatePosition = playerPosition + new Vector3(randomCircle.x, randomCircle.y, 0);
            
            // 检查距离玩家是否足够远
            float distanceFromPlayer = Vector3.Distance(candidatePosition, playerPosition);
            if (distanceFromPlayer < minDistanceFromPlayer)
            {
                continue;
            }
            
            // 检查是否在有效位置（可选：检查地面等）
            if (IsValidSpawnPosition(candidatePosition))
            {
                return candidatePosition;
            }
        }
        
        // 如果找不到合适位置，在玩家周围生成
        Vector2 fallbackCircle = Random.insideUnitCircle * spawnRadius;
        return playerPosition + new Vector3(fallbackCircle.x, fallbackCircle.y, 0);
    }
    
    /// <summary>
    /// 检查是否是有效的生成位置
    /// </summary>
    private bool IsValidSpawnPosition(Vector3 position)
    {
        // 检查是否在地面上方
        Vector2 groundCheckPos = new Vector2(position.x, position.y - 1f);
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheckPos, Vector2.down, 2f);
        
        if (groundHit.collider != null)
        {
            // 检查是否在地面层上
            if (groundHit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                // 调整生成位置到地面上方
                position.y = groundHit.point.y + 1f;
                return true;
            }
        }
        
        // 如果没有检测到地面，检查当前位置是否合理
        if (position.y >= -5f) // 确保不会生成得太低
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 清理已死亡的敌人
    /// </summary>
    private void CleanupDeadEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null || !activeEnemies[i].activeInHierarchy)
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// 敌人死亡处理
    /// </summary>
    private void OnEnemyDeathHandler(BookEnemy deadEnemy)
    {
        if (deadEnemy != null)
        {
            activeEnemies.Remove(deadEnemy.gameObject);
            OnEnemyDestroyed?.Invoke(deadEnemy.gameObject);
            
            if (showDebugInfo)
            {
                Debug.Log($"书本怪死亡，当前活跃敌人数量: {activeEnemies.Count}");
            }
        }
    }
    
    /// <summary>
    /// 清空所有敌人
    /// </summary>
    public void ClearAllEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] != null)
            {
                // 取消订阅事件
                BookEnemy bookEnemy = activeEnemies[i].GetComponent<BookEnemy>();
                if (bookEnemy != null)
                {
                    bookEnemy.OnEnemyDeath -= OnEnemyDeathHandler;
                }
                
                // 销毁或归还到对象池
                if (PoolManager.Instance != null)
                {
                    // 尝试归还到对象池
                    if (activeEnemies[i].GetComponent<BookEnemy>() != null)
                    {
                        PoolManager.Instance.Despawn("BookEnemy", activeEnemies[i]);
                    }
                    else if (activeEnemies[i].GetComponent<HiddenBookEnemy>() != null)
                    {
                        PoolManager.Instance.Despawn("HiddenBookEnemy", activeEnemies[i]);
                    }
                    else
                    {
                        Destroy(activeEnemies[i]);
                    }
                }
                else
                {
                    Destroy(activeEnemies[i]);
                }
            }
        }
        
        activeEnemies.Clear();
        
        if (showDebugInfo)
        {
            Debug.Log("清空所有书本怪");
        }
    }
    
    /// <summary>
    /// 设置生成开关
    /// </summary>
    public void SetSpawningEnabled(bool enabled)
    {
        enableSpawning = enabled;
        
        if (showDebugInfo)
        {
            Debug.Log($"书本怪生成: {(enabled ? "启用" : "禁用")}");
        }
    }
    
    /// <summary>
    /// 获取当前活跃敌人数量
    /// </summary>
    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }
    
    /// <summary>
    /// 设置生成间隔
    /// </summary>
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = Mathf.Max(0.1f, interval);
    }
    
    /// <summary>
    /// 设置最大敌人数量
    /// </summary>
    public void SetMaxEnemies(int maxCount)
    {
        maxEnemies = Mathf.Max(1, maxCount);
    }
    
    // 在编辑器中显示生成范围
    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            // 绘制生成范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, spawnRadius);
            
            // 绘制最小距离
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);
        }
    }
}
