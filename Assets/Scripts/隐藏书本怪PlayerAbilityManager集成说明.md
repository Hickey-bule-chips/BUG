# 隐藏书本怪与PlayerAbilityManager集成说明

## 概述
隐藏书本怪现在完全依赖PlayerAbilityManager的修复视觉功能来实现显示和隐藏效果。这种设计确保了系统的一致性和可维护性。

## 集成架构

### 1. 事件驱动系统
- **PlayerAbilityManager** 提供 `OnRepairVisionToggled` 事件
- **HiddenBookEnemy** 订阅此事件来响应修复视觉状态变化
- 当玩家按Shift键时，所有隐藏书本怪会同时响应

### 2. 注册管理
- HiddenBookEnemy在生成时自动注册到PlayerAbilityManager
- PlayerAbilityManager维护一个隐藏书本怪列表
- 支持动态注册和取消注册

### 3. 状态同步
- 修复视觉激活时：显示所有隐藏书本怪
- 修复视觉关闭时：隐藏所有隐藏书本怪
- 精神值耗尽时：自动关闭修复视觉并隐藏书本怪

## 关键功能

### PlayerAbilityManager新增方法
```csharp
// 注册隐藏书本怪
public void RegisterHiddenBookEnemy(HiddenBookEnemy hiddenBookEnemy)

// 取消注册隐藏书本怪
public void UnregisterHiddenBookEnemy(HiddenBookEnemy hiddenBookEnemy)

// 更新所有隐藏书本怪的可见性
public void UpdateHiddenBookEnemiesVisibility()
```

### HiddenBookEnemy集成功能
```csharp
// 自动查找并注册到PlayerAbilityManager
private void FindPlayerAbilityManager()

// 响应修复视觉状态变化
private void OnRepairVisionToggled(bool isActive)

// 对象池重用时重新注册
public override void ResetEnemy()
```

## 使用流程

### 1. 生成隐藏书本怪
1. BookEnemySpawner生成HiddenBookEnemy
2. HiddenBookEnemy.Start()自动查找PlayerAbilityManager
3. 自动订阅OnRepairVisionToggled事件
4. 自动注册到PlayerAbilityManager的隐藏书本怪列表

### 2. 修复视觉激活
1. 玩家按Shift键
2. PlayerAbilityManager.ActivateRepairVision()
3. 更新所有隐藏书本怪的可见性
4. 触发OnRepairVisionToggled事件
5. 所有HiddenBookEnemy响应并显示

### 3. 修复视觉关闭
1. 玩家松开Shift键或精神值耗尽
2. PlayerAbilityManager.DeactivateRepairVision()
3. 更新所有隐藏书本怪的可见性
4. 触发OnRepairVisionToggled事件
5. 所有HiddenBookEnemy响应并隐藏

### 4. 对象池回收
1. HiddenBookEnemy死亡
2. OnDestroy()取消事件订阅和注册
3. 对象返回对象池
4. ResetEnemy()重新注册到PlayerAbilityManager

## 配置要求

### PlayerAbilityManager设置
1. 确保HiddenEnemyLayer正确设置
2. 确保visionActivationCost合理配置
3. 确保精神值消耗和恢复机制正常工作

### HiddenBookEnemy设置
1. 设置合适的hiddenColor和visibleColor
2. 确保Tag设置为"Enemy"
3. 确保Layer设置为HiddenEnemy层

## 优势

### 1. 系统一致性
- 所有隐藏物体都通过PlayerAbilityManager统一管理
- 修复视觉功能对所有隐藏元素生效

### 2. 性能优化
- 事件驱动，避免每帧检查输入
- 集中管理，减少重复代码

### 3. 扩展性
- 可以轻松添加其他类型的隐藏敌人
- 可以添加更复杂的修复视觉效果

### 4. 维护性
- 单一职责原则，PlayerAbilityManager专门管理修复视觉
- HiddenBookEnemy专注于敌人行为逻辑

## 调试信息
系统会输出详细的调试信息：
- 隐藏书本怪注册/取消注册
- 修复视觉状态变化
- 可见性更新过程

## 注意事项
1. 确保PlayerAbilityManager存在于玩家对象上
2. 确保隐藏书本怪正确设置Layer和Tag
3. 对象池重用时会自动重新注册
4. 精神值耗尽时会自动关闭修复视觉

这种集成方式确保了隐藏书本怪与游戏的整体修复视觉系统完美配合，提供了更好的用户体验和系统架构。
