using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{
    [Header("修复视觉能力设置")]
    [SerializeField] private bool isRepairVisionActive = false; // 修复视觉是否激活
    [SerializeField] private float visionActivationCost = 5f; // 激活视觉的最低精神值要求
    
    [Header("视觉效果设置")]
    [SerializeField] private LayerMask hiddenEnemyLayer = -1; // 隐藏敌人层
    [SerializeField] private LayerMask hiddenGroundLayer = -1; // 隐藏地面层
    [SerializeField] private LayerMask hiddenTrapLayer = -1; // 隐藏陷阱层
    [SerializeField] private LayerMask obstacleWallLayer = -1; // 阻碍墙层
    
    [Header("视觉效果")]
    [SerializeField] private Color repairVisionTint = new Color(1f, 0.5f, 0.5f, 0.3f); // 修复视觉色调
    [SerializeField] private float fadeSpeed = 2f; // 淡入淡出速度
    
    // 组件引用
    private PlayerStatus playerStatus;
    private Camera mainCamera;
    
    // 存储原始状态
    private Dictionary<GameObject, bool> originalEnemyStates = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, bool> originalGroundStates = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, bool> originalTrapStates = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, bool> originalWallStates = new Dictionary<GameObject, bool>();
    
    // 存储隐藏对象的引用
    private List<GameObject> hiddenEnemyObjects = new List<GameObject>();
    private List<GameObject> hiddenGroundObjects = new List<GameObject>();
    private List<GameObject> hiddenTrapObjects = new List<GameObject>();
    
    // 事件
    public System.Action<bool> OnRepairVisionToggled; // 修复视觉切换事件
    
    void Start()
    {
        // 获取组件
        playerStatus = GetComponent<PlayerStatus>();
        mainCamera = Camera.main;
        
        if (playerStatus == null)
        {
            Debug.LogError("PlayerStatus组件未找到！请确保玩家对象上有PlayerStatus脚本。");
        }
        
        // 订阅精神值耗尽事件
        if (playerStatus != null)
        {
            playerStatus.OnMentalHealthDepleted += OnMentalHealthDepleted;
        }
        
        // 游戏开始时隐藏所有应该隐藏的对象
        HideAllHiddenObjects();
    }
    
    void Update()
    {
        // 检测Shift键输入
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            ToggleRepairVision();
        }
    }
    
    /// <summary>
    /// 切换修复视觉能力
    /// </summary>
    private void ToggleRepairVision()
    {
        if (isRepairVisionActive)
        {
            // 关闭修复视觉
            DeactivateRepairVision();
        }
        else
        {
            // 激活修复视觉
            ActivateRepairVision();
        }
    }
    
    /// <summary>
    /// 激活修复视觉能力
    /// </summary>
    private void ActivateRepairVision()
    {
        // 检查精神值是否足够
        if (playerStatus != null && !playerStatus.HasEnoughMentalHealth(visionActivationCost))
        {
            Debug.Log("精神值不足，无法激活修复视觉！");
            return;
        }
        
        isRepairVisionActive = true;
        Debug.Log("激活修复视觉能力");
        
        // 开始消耗精神值
        if (playerStatus != null)
        {
            playerStatus.StartDraining();
        }
        
        // 显示隐藏的物体
        ShowHiddenObjects();
        
        // 隐藏阻碍墙
        Debug.Log("开始隐藏阻碍墙...");
        HideObstacleWalls();
        
        // 应用视觉效果
        ApplyRepairVisionEffect();
        
        // 触发事件
        OnRepairVisionToggled?.Invoke(true);
    }
    
    /// <summary>
    /// 关闭修复视觉能力
    /// </summary>
    private void DeactivateRepairVision()
    {
        isRepairVisionActive = false;
        Debug.Log("关闭修复视觉能力");
        
        // 停止消耗精神值并开始恢复
        if (playerStatus != null)
        {
            playerStatus.StopDraining();
        }
        
        // 隐藏之前显示的物体
        HideRevealedObjects();
        
        // 恢复阻碍墙
        ShowObstacleWalls();
        
        // 移除视觉效果
        RemoveRepairVisionEffect();
        
        // 触发事件
        OnRepairVisionToggled?.Invoke(false);
    }
    
    /// <summary>
    /// 显示隐藏的物体
    /// </summary>
    private void ShowHiddenObjects()
    {
        Debug.Log("开始显示隐藏的物体...");
        
        // 显示隐藏的敌人
        Debug.Log($"显示隐藏的敌人 - 数量: {hiddenEnemyObjects.Count}");
        ShowObjectsFromList(hiddenEnemyObjects, originalEnemyStates);
        
        // 显示隐藏的地面
        Debug.Log($"显示隐藏的地面 - 数量: {hiddenGroundObjects.Count}");
        ShowObjectsFromList(hiddenGroundObjects, originalGroundStates);
        
        // 显示隐藏的陷阱
        Debug.Log($"显示隐藏的陷阱 - 数量: {hiddenTrapObjects.Count}");
        ShowObjectsFromList(hiddenTrapObjects, originalTrapStates);
        
        Debug.Log("隐藏物体显示完成");
    }
    
    /// <summary>
    /// 隐藏之前显示的物体
    /// </summary>
    private void HideRevealedObjects()
    {
        Debug.Log("开始隐藏之前显示的物体...");
        
        // 恢复隐藏的敌人
        Debug.Log($"恢复隐藏的敌人 - 数量: {originalEnemyStates.Count}");
        RestoreObjectsInLayer(originalEnemyStates);
        
        // 恢复隐藏的地面
        Debug.Log($"恢复隐藏的地面 - 数量: {originalGroundStates.Count}");
        RestoreObjectsInLayer(originalGroundStates);
        
        // 恢复隐藏的陷阱
        Debug.Log($"恢复隐藏的陷阱 - 数量: {originalTrapStates.Count}");
        RestoreObjectsInLayer(originalTrapStates);
        
        Debug.Log("隐藏物体恢复完成");
    }
    
    /// <summary>
    /// 游戏开始时隐藏所有应该隐藏的对象
    /// </summary>
    private void HideAllHiddenObjects()
    {
        Debug.Log("游戏开始，隐藏所有应该隐藏的对象...");
        
        // 隐藏隐藏的敌人
        if (hiddenEnemyLayer != -1)
        {
            HideObjectsInLayer(hiddenEnemyLayer, hiddenEnemyObjects);
            Debug.Log($"隐藏了隐藏敌人层的所有对象，数量: {hiddenEnemyObjects.Count}");
        }
        
        // 隐藏隐藏的地面
        if (hiddenGroundLayer != -1)
        {
            HideObjectsInLayer(hiddenGroundLayer, hiddenGroundObjects);
            Debug.Log($"隐藏了隐藏地面层的所有对象，数量: {hiddenGroundObjects.Count}");
        }
        
        // 隐藏隐藏的陷阱
        if (hiddenTrapLayer != -1)
        {
            HideObjectsInLayer(hiddenTrapLayer, hiddenTrapObjects);
            Debug.Log($"隐藏了隐藏陷阱层的所有对象，数量: {hiddenTrapObjects.Count}");
        }
        
        Debug.Log("所有隐藏对象已隐藏");
    }
    
    /// <summary>
    /// 隐藏指定层的所有对象
    /// </summary>
    private void HideObjectsInLayer(LayerMask layer, List<GameObject> objectList)
    {
        GameObject[] objectsInLayer = FindObjectsInLayer(layer);
        
        // 清空列表并保存引用
        objectList.Clear();
        
        foreach (GameObject obj in objectsInLayer)
        {
            if (obj != null)
            {
                Debug.Log($"游戏开始时隐藏对象: {obj.name}");
                objectList.Add(obj); // 保存对象引用
                obj.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 从列表中显示对象
    /// </summary>
    private void ShowObjectsFromList(List<GameObject> objectList, Dictionary<GameObject, bool> stateDict)
    {
        foreach (GameObject obj in objectList)
        {
            if (obj != null)
            {
                Debug.Log($"准备显示对象: {obj.name}");
                Debug.Log($"对象当前状态 - activeInHierarchy: {obj.activeInHierarchy}, activeSelf: {obj.activeSelf}");
                
                // 只有在第一次显示时才保存原始状态
                if (!stateDict.ContainsKey(obj))
                {
                    stateDict[obj] = obj.activeInHierarchy;
                    Debug.Log($"首次保存原始状态: {obj.activeInHierarchy}");
                }
                else
                {
                    Debug.Log($"使用已保存的原始状态: {stateDict[obj]}");
                }
                
                // 显示物体
                obj.SetActive(true);
                Debug.Log($"设置对象激活状态为true");
                Debug.Log($"设置后状态 - activeInHierarchy: {obj.activeInHierarchy}, activeSelf: {obj.activeSelf}");
                
                // 暂时不使用淡入效果，直接显示
                // StartCoroutine(FadeInObject(obj));
                Debug.Log($"对象 {obj.name} 已直接显示");
            }
        }
    }
    
    /// <summary>
    /// 显示指定层的物体
    /// </summary>
    private void ShowObjectsInLayer(LayerMask layer, Dictionary<GameObject, bool> stateDict)
    {
        if (layer == -1) 
        {
            Debug.Log("Layer未设置，跳过显示");
            return; // 如果层未设置，跳过
        }
        
        GameObject[] objectsInLayer = FindObjectsInLayer(layer);
        Debug.Log($"在Layer {layer}中找到 {objectsInLayer.Length} 个对象");
        
        foreach (GameObject obj in objectsInLayer)
        {
            if (obj != null)
            {
                Debug.Log($"准备显示对象: {obj.name} (Layer: {obj.layer})");
                Debug.Log($"对象当前状态 - activeInHierarchy: {obj.activeInHierarchy}, activeSelf: {obj.activeSelf}");
                
                // 保存原始状态（应该是false，因为游戏开始时被隐藏了）
                stateDict[obj] = obj.activeInHierarchy;
                Debug.Log($"保存原始状态: {obj.activeInHierarchy}");
                
                // 显示物体
                obj.SetActive(true);
                Debug.Log($"设置对象激活状态为true");
                Debug.Log($"设置后状态 - activeInHierarchy: {obj.activeInHierarchy}, activeSelf: {obj.activeSelf}");
                
                // 暂时不使用淡入效果，直接显示
                // StartCoroutine(FadeInObject(obj));
                Debug.Log($"对象 {obj.name} 已直接显示");
            }
        }
    }
    
    /// <summary>
    /// 恢复指定层的物体状态
    /// </summary>
    private void RestoreObjectsInLayer(Dictionary<GameObject, bool> stateDict)
    {
        foreach (var kvp in stateDict)
        {
            if (kvp.Key != null)
            {
                Debug.Log($"恢复对象状态: {kvp.Key.name} -> {kvp.Value}");
                
                // 直接设置状态，不使用淡出效果
                kvp.Key.SetActive(kvp.Value);
                
                // 暂时不使用淡出效果
                // StartCoroutine(FadeOutObject(kvp.Key, kvp.Value));
            }
        }
        
        // 不要清空状态字典，保持原始状态记录
        // stateDict.Clear();
    }
    
    /// <summary>
    /// 隐藏阻碍墙
    /// </summary>
    private void HideObstacleWalls()
    {
        if (obstacleWallLayer == -1) 
        {
            Debug.LogWarning("阻碍墙层未设置！请在Inspector中设置Obstacle Wall Layer");
            return;
        }
        
        GameObject[] walls = FindObjectsInLayer(obstacleWallLayer);
        Debug.Log($"找到 {walls.Length} 个阻碍墙");
        
        foreach (GameObject wall in walls)
        {
            if (wall != null)
            {
                Debug.Log($"隐藏阻碍墙: {wall.name}");
                
                // 保存原始状态
                originalWallStates[wall] = wall.activeInHierarchy;
                
                // 隐藏阻碍墙
                wall.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 显示阻碍墙
    /// </summary>
    private void ShowObstacleWalls()
    {
        foreach (var kvp in originalWallStates)
        {
            if (kvp.Key != null)
            {
                kvp.Key.SetActive(kvp.Value);
            }
        }
        
        // 清空状态字典
        originalWallStates.Clear();
    }
    
    /// <summary>
    /// 应用修复视觉效果
    /// </summary>
    private void ApplyRepairVisionEffect()
    {
        if (mainCamera != null)
        {
            // 可以在这里添加后处理效果
            // 例如：改变屏幕色调、添加滤镜等
        }
    }
    
    /// <summary>
    /// 移除修复视觉效果
    /// </summary>
    private void RemoveRepairVisionEffect()
    {
        if (mainCamera != null)
        {
            // 移除后处理效果
        }
    }
    
    /// <summary>
    /// 淡入物体
    /// </summary>
    private IEnumerator FadeInObject(GameObject obj)
    {
        Debug.Log($"开始淡入对象: {obj.name}");
        
        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        Debug.Log($"找到 {renderers.Length} 个SpriteRenderer组件");
        
        if (renderers.Length == 0)
        {
            Debug.Log($"对象 {obj.name} 没有SpriteRenderer组件，直接显示");
            yield break; // 如果没有SpriteRenderer，直接结束协程
        }
        
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null)
            {
                Color originalColor = renderer.color;
                Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
                
                float elapsedTime = 0f;
                while (elapsedTime < 1f / fadeSpeed)
                {
                    elapsedTime += Time.deltaTime;
                    renderer.color = Color.Lerp(originalColor, targetColor, elapsedTime * fadeSpeed);
                    yield return null;
                }
                
                renderer.color = targetColor;
                Debug.Log($"完成淡入: {renderer.name}");
            }
        }
        
        Debug.Log($"淡入完成: {obj.name}");
    }
    
    /// <summary>
    /// 淡出物体
    /// </summary>
    private IEnumerator FadeOutObject(GameObject obj, bool shouldBeActive)
    {
        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null)
            {
                Color originalColor = renderer.color;
                Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
                
                float elapsedTime = 0f;
                while (elapsedTime < 1f / fadeSpeed)
                {
                    elapsedTime += Time.deltaTime;
                    renderer.color = Color.Lerp(originalColor, targetColor, elapsedTime * fadeSpeed);
                    yield return null;
                }
                
                renderer.color = targetColor;
            }
        }
        
        // 根据原始状态设置物体激活状态
        obj.SetActive(shouldBeActive);
    }
    
    /// <summary>
    /// 查找指定层的所有物体
    /// </summary>
    private GameObject[] FindObjectsInLayer(LayerMask layer)
    {
        List<GameObject> objects = new List<GameObject>();
        
        // 遍历所有游戏对象
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            // 使用LayerMask的正确比较方式
            if ((layer.value & (1 << obj.layer)) != 0)
            {
                objects.Add(obj);
            }
        }
        
        return objects.ToArray();
    }
    
    /// <summary>
    /// 精神值耗尽时的处理
    /// </summary>
    private void OnMentalHealthDepleted()
    {
        if (isRepairVisionActive)
        {
            // 精神值耗尽时自动关闭修复视觉能力
            // 注意：不要调用DeactivateRepairVision()，因为PlayerStatus已经处理了恢复逻辑
            isRepairVisionActive = false;
            Debug.Log("精神值耗尽，自动关闭修复视觉能力");
            
            // 隐藏之前显示的物体
            HideRevealedObjects();
            
            // 恢复阻碍墙
            ShowObstacleWalls();
            
            // 移除视觉效果
            RemoveRepairVisionEffect();
            
            // 触发事件
            OnRepairVisionToggled?.Invoke(false);
        }
    }
    
    /// <summary>
    /// 获取修复视觉是否激活
    /// </summary>
    public bool IsRepairVisionActive()
    {
        return isRepairVisionActive;
    }
    
    void OnDestroy()
    {
        // 取消订阅事件
        if (playerStatus != null)
        {
            playerStatus.OnMentalHealthDepleted -= OnMentalHealthDepleted;
        }
    }
}
