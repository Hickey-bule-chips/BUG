using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkPlatform : MonoBehaviour
{
    [Header("平台设置")]
    [SerializeField] private float duration = 5f; // 平台存在时间
    [SerializeField] private float fadeStartTime = 1f; // 开始闪烁的时间（销毁前多久）
    [SerializeField] private float blinkInterval = 0.2f; // 闪烁间隔
    
    [Header("视觉效果")]
    [SerializeField] private bool enableFade = true; // 是否启用淡出效果
    [SerializeField] private Color normalColor = Color.white; // 正常颜色
    [SerializeField] private Color warningColor = Color.yellow; // 警告颜色
    
    private SpriteRenderer spriteRenderer;
    private float timer;
    private bool isBlinking = false;
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timer = duration;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }
    
    private void Update()
    {
        timer -= Time.deltaTime;
        
        // 开始闪烁警告
        if (timer <= fadeStartTime && !isBlinking)
        {
            isBlinking = true;
            if (enableFade)
            {
                StartCoroutine(BlinkCoroutine());
            }
        }
        
        // 时间到，销毁平台
        if (timer <= 0)
        {
            DestoryPlatform();
        }
    }
    
    /// <summary>
    /// 设置平台存在时间
    /// </summary>
    public void SetDuration(float newDuration)
    {
        duration = newDuration;
        timer = newDuration;
    }
    
    /// <summary>
    /// 闪烁协程
    /// </summary>
    private IEnumerator BlinkCoroutine()
    {
        while (timer > 0)
        {
            if (spriteRenderer != null)
            {
                // 切换颜色
                spriteRenderer.color = spriteRenderer.color == normalColor ? warningColor : normalColor;
            }
            yield return new WaitForSeconds(blinkInterval);
        }
    }
    
    /// <summary>
    /// 销毁平台
    /// </summary>
    private void DestoryPlatform()
    {
        // 可以在这里添加销毁特效
        Debug.Log("墨水平台消失");
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 获取剩余时间百分比
    /// </summary>
    public float GetRemainingTimePercent()
    {
        return timer / duration;
    }
}

