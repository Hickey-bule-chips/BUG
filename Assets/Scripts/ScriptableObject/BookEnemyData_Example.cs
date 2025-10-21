using UnityEngine;

/// <summary>
/// 书本怪数据示例
/// 这个文件展示了如何创建和配置书本怪数据
/// </summary>
public class BookEnemyData_Example : MonoBehaviour
{
    [Header("示例数据创建指南")]
    [TextArea(10, 20)]
    public string instructions = @"
创建书本怪数据的步骤：

1. 在Project窗口右键 → Create → Enemy → Book Enemy Data
2. 命名文件（例如：NormalBookEnemyData, HiddenBookEnemyData）
3. 设置以下参数：

普通书本怪数据示例：
- Enemy Name: 普通书本怪
- Max Health: 50
- Damage: 10
- Move Speed: 3
- Follow Distance: 8
- Stop Follow Distance: 12
- Attack Interval: 1
- Contact Damage Interval: 0.5
- Collider Radius: 0.5
- Is Trigger: true

隐藏书本怪数据示例：
- Enemy Name: 隐藏书本怪
- Max Health: 30
- Damage: 15
- Move Speed: 2.5
- Follow Distance: 6
- Stop Follow Distance: 10
- Attack Interval: 0.8
- Contact Damage Interval: 0.3
- Collider Radius: 0.4
- Is Trigger: true

注意：
- 隐藏书本怪通常血量较少但伤害更高
- 可以根据游戏平衡调整这些数值
- 记得设置合适的精灵图片和音效
";

    void Start()
    {
        Debug.Log("请查看Inspector面板中的创建指南");
    }
}
