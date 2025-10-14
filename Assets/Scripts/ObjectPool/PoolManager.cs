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
}


