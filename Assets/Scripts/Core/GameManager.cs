using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("游戏状态")]
    [SerializeField] private bool isGameOver = false;
    [SerializeField] private bool isPaused = false;
    
    [Header("死亡设置")]
    [SerializeField] private float deathAnimationDuration = 2f; // 死亡动画持续时间
    [SerializeField] private float gameOverDelay = 3f; // 游戏结束延迟时间
    
    [Header("UI引用")]
    [SerializeField] private GameObject gameOverUI; // 游戏结束UI
    [SerializeField] private GameObject pauseUI; // 暂停UI
    
    // 组件引用
    private PlayerStatus playerStatus;
    private Animator playerAnimator;
    
    // 事件
    public System.Action OnGameOver; // 游戏结束事件
    public System.Action OnGamePaused; // 游戏暂停事件
    public System.Action OnGameResumed; // 游戏恢复事件
    
    // 单例模式
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // 获取玩家组件
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStatus = player.GetComponent<PlayerStatus>();
            playerAnimator = player.GetComponent<Animator>();
            
            // 订阅玩家死亡事件
            if (playerStatus != null)
            {
                playerStatus.OnPlayerDeath += OnPlayerDeath;
            }
        }
        
        // 初始化UI状态
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        
        if (pauseUI != null)
        {
            pauseUI.SetActive(false);
        }
    }
    
    void Update()
    {
        // 检测暂停输入（ESC键）
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            TogglePause();
        }
    }
    
    /// <summary>
    /// 玩家死亡处理
    /// </summary>
    private void OnPlayerDeath()
    {
        if (isGameOver) return; // 防止重复处理
        
        Debug.Log("玩家死亡，开始死亡流程");
        StartCoroutine(HandlePlayerDeath());
    }
    
    /// <summary>
    /// 处理玩家死亡流程
    /// </summary>
    private IEnumerator HandlePlayerDeath()
    {
        isGameOver = true;
        
        // 等待死亡动画播放（由PlayerController处理）
        yield return new WaitForSeconds(deathAnimationDuration);
        
        // 显示游戏结束UI
        ShowGameOverUI();
        
        // 触发游戏结束事件
        OnGameOver?.Invoke();
        
        // 等待一段时间后自动重新开始或返回主菜单
        yield return new WaitForSeconds(gameOverDelay);
        
        // 重新加载场景
        RestartGame();
    }
    
    /// <summary>
    /// 显示游戏结束UI
    /// </summary>
    private void ShowGameOverUI()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        
        Debug.Log("显示游戏结束界面");
    }
    
    /// <summary>
    /// 切换暂停状态
    /// </summary>
    public void TogglePause()
    {
        if (isGameOver) return;
        
        isPaused = !isPaused;
        
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }
    
    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pauseUI != null)
        {
            pauseUI.SetActive(true);
        }
        
        OnGamePaused?.Invoke();
        Debug.Log("游戏暂停");
    }
    
    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pauseUI != null)
        {
            pauseUI.SetActive(false);
        }
        
        OnGameResumed?.Invoke();
        Debug.Log("游戏恢复");
    }
    
    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("重新开始游戏");
    }
    
    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // 假设主菜单场景名为"MainMenu"
        Debug.Log("返回主菜单");
    }
    
    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    /// <summary>
    /// 获取游戏是否结束
    /// </summary>
    public bool IsGameOver()
    {
        return isGameOver;
    }
    
    /// <summary>
    /// 获取游戏是否暂停
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
    
    void OnDestroy()
    {
        // 取消订阅事件
        if (playerStatus != null)
        {
            playerStatus.OnPlayerDeath -= OnPlayerDeath;
        }
    }
}
