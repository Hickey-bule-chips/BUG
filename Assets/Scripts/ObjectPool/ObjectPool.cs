using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用对象池类
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [Header("对象池设置")]
    [SerializeField] private GameObject prefab; // 预制体
    [SerializeField] private int initialSize = 10; // 初始池大小
    [SerializeField] private int maxSize = 50; // 最大池大小
    [SerializeField] private bool autoExpand = true; // 是否自动扩展
    
    private Queue<GameObject> pool = new Queue<GameObject>();
    private List<GameObject> activeObjects = new List<GameObject>();
    private Transform poolContainer;
    
    private void Awake()
    {
        if (prefab != null)
        {
            // 创建对象池容器
            poolContainer = new GameObject($"Pool_{prefab.name}").transform;
            poolContainer.SetParent(transform);
            
            // 初始化对象池
            InitializePool();
        }
    }
    
    /// <summary>
    /// 设置对象池参数（供PoolManager使用）
    /// </summary>
    public void SetPoolSettings(GameObject poolPrefab, int initSize, int maxCapacity)
    {
        prefab = poolPrefab;
        initialSize = initSize;
        maxSize = maxCapacity;
        
        // 创建对象池容器
        if (poolContainer == null)
        {
            poolContainer = new GameObject($"Pool_{prefab.name}").transform;
            poolContainer.SetParent(transform);
        }
        
        // 初始化对象池
        InitializePool();
    }
    
    /// <summary>
    /// 初始化对象池
    /// </summary>
    private void InitializePool()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }
    
    /// <summary>
    /// 创建新对象
    /// </summary>
    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, poolContainer);
        obj.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }
    
    /// <summary>
    /// 从对象池获取对象
    /// </summary>
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj;
        
        // 如果池中没有可用对象
        if (pool.Count == 0)
        {
            if (autoExpand && activeObjects.Count < maxSize)
            {
                obj = CreateNewObject();
            }
            else
            {
                Debug.LogWarning($"对象池 {prefab.name} 已达到最大容量！");
                return null;
            }
        }
        else
        {
            obj = pool.Dequeue();
        }
        
        // 设置对象
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        
        // 添加到活动列表
        activeObjects.Add(obj);
        
        return obj;
    }
    
    /// <summary>
    /// 归还对象到池中
    /// </summary>
    public void Return(GameObject obj)
    {
        if (obj == null)
            return;
        
        // 从活动列表移除
        activeObjects.Remove(obj);
        
        // 重置对象
        obj.SetActive(false);
        obj.transform.SetParent(poolContainer);
        
        // 归还到池中
        pool.Enqueue(obj);
    }
    
    /// <summary>
    /// 延迟归还对象
    /// </summary>
    public void ReturnAfterDelay(GameObject obj, float delay)
    {
        StartCoroutine(ReturnAfterDelayCoroutine(obj, delay));
    }
    
    private IEnumerator ReturnAfterDelayCoroutine(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Return(obj);
    }
    
    /// <summary>
    /// 清空对象池
    /// </summary>
    public void Clear()
    {
        // 归还所有活动对象
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            Return(activeObjects[i]);
        }
        
        activeObjects.Clear();
    }
    
    /// <summary>
    /// 获取对象池信息
    /// </summary>
    public void GetPoolInfo(out int available, out int active)
    {
        available = pool.Count;
        active = activeObjects.Count;
    }
}

