using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池管理器（单例模式）
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }
    
    [System.Serializable]
    public class PoolConfig
    {
        public string poolName;
        public GameObject prefab;
        public int initialSize = 10;
        public int maxSize = 50;
    }
    
    [Header("对象池配置")]
    [SerializeField] private List<PoolConfig> poolConfigs = new List<PoolConfig>();
    
    private Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();
    
    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化所有对象池
    /// </summary>
    private void InitializePools()
    {
        foreach (var config in poolConfigs)
        {
            CreatePool(config.poolName, config.prefab, config.initialSize, config.maxSize);
        }
    }
    
    /// <summary>
    /// 创建对象池
    /// </summary>
    public void CreatePool(string poolName, GameObject prefab, int initialSize = 10, int maxSize = 50)
    {
        if (pools.ContainsKey(poolName))
        {
            Debug.LogWarning($"对象池 {poolName} 已存在！");
            return;
        }
        
        GameObject poolObj = new GameObject($"Pool_{poolName}");
        poolObj.transform.SetParent(transform);
        
        ObjectPool pool = poolObj.AddComponent<ObjectPool>();
        // 通过反射或公共方法设置参数
        pool.SetPoolSettings(prefab, initialSize, maxSize);
        
        pools.Add(poolName, pool);
        
        Debug.Log($"创建对象池: {poolName}，初始大小: {initialSize}");
    }
    
    /// <summary>
    /// 从对象池获取对象
    /// </summary>
    public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(poolName))
        {
            Debug.LogError($"对象池 {poolName} 不存在！");
            return null;
        }
        
        return pools[poolName].Get(position, rotation);
    }
    
    /// <summary>
    /// 归还对象到池中
    /// </summary>
    public void Despawn(string poolName, GameObject obj)
    {
        if (!pools.ContainsKey(poolName))
        {
            Debug.LogError($"对象池 {poolName} 不存在！");
            return;
        }
        
        pools[poolName].Return(obj);
    }
    
    /// <summary>
    /// 延迟归还对象
    /// </summary>
    public void DespawnAfterDelay(string poolName, GameObject obj, float delay)
    {
        if (!pools.ContainsKey(poolName))
        {
            Debug.LogError($"对象池 {poolName} 不存在！");
            return;
        }
        
        pools[poolName].ReturnAfterDelay(obj, delay);
    }
    
    /// <summary>
    /// 清空所有对象池
    /// </summary>
    public void ClearAll()
    {
        foreach (var pool in pools.Values)
        {
            pool.Clear();
        }
    }
    
    /// <summary>
    /// 获取对象池信息
    /// </summary>
    public void LogPoolInfo(string poolName)
    {
        if (!pools.ContainsKey(poolName))
        {
            Debug.LogError($"对象池 {poolName} 不存在！");
            return;
        }
        
        pools[poolName].GetPoolInfo(out int available, out int active);
        Debug.Log($"对象池 {poolName} - 可用: {available}, 活动: {active}");
    }
    
    /// <summary>
    /// 检查对象池是否存在
    /// </summary>
    public bool PoolExists(string poolName)
    {
        return pools.ContainsKey(poolName);
    }
    
    /// <summary>
    /// 创建书本怪对象池
    /// </summary>
    public void CreateBookEnemyPools(GameObject bookEnemyPrefab, GameObject hiddenBookEnemyPrefab, int initialSize = 10, int maxSize = 30)
    {
        if (bookEnemyPrefab != null)
        {
            CreatePool("BookEnemy", bookEnemyPrefab, initialSize, maxSize);
        }
        
        if (hiddenBookEnemyPrefab != null)
        {
            CreatePool("HiddenBookEnemy", hiddenBookEnemyPrefab, initialSize, maxSize);
        }
    }
    
    /// <summary>
    /// 生成书本怪
    /// </summary>
    public GameObject SpawnBookEnemy(Vector3 position, BookEnemyData enemyData)
    {
        GameObject bookEnemy = Spawn("BookEnemy", position, Quaternion.identity);
        if (bookEnemy != null)
        {
            BookEnemy bookEnemyScript = bookEnemy.GetComponent<BookEnemy>();
            if (bookEnemyScript != null)
            {
                bookEnemyScript.SetEnemyData(enemyData);
                bookEnemyScript.SetPoolInfo("BookEnemy", true);
                bookEnemyScript.ResetEnemy();
            }
        }
        return bookEnemy;
    }
    
    /// <summary>
    /// 生成隐藏书本怪
    /// </summary>
    public GameObject SpawnHiddenBookEnemy(Vector3 position, BookEnemyData enemyData)
    {
        GameObject hiddenBookEnemy = Spawn("HiddenBookEnemy", position, Quaternion.identity);
        if (hiddenBookEnemy != null)
        {
            HiddenBookEnemy hiddenBookEnemyScript = hiddenBookEnemy.GetComponent<HiddenBookEnemy>();
            if (hiddenBookEnemyScript != null)
            {
                hiddenBookEnemyScript.SetEnemyData(enemyData);
                hiddenBookEnemyScript.SetPoolInfo("HiddenBookEnemy", true);
                hiddenBookEnemyScript.ResetEnemy();
            }
        }
        return hiddenBookEnemy;
    }
}


