using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Rewards;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Rewards;

namespace UltraLib.Base.Utils;

/// <summary>
/// Reward screen helpers.
/// </summary>
/// <remarks>
/// 奖励（Reward）界面辅助工具。
/// <para>
/// 提供获取当前奖励界面中玩家数据的高效方法。
/// </para>
/// </remarks>
public static class RewardHelper
{
    private static FieldInfo? _rewardButtonsField;
    private static FieldInfo? _rewardsScreenRunStateField;
    private static bool _reflectionInitialized;

    /// <summary>
    /// 获取指定玩家在当前奖励界面中持有的所有 Reward 数据。
    /// <para>
    /// 使用反射访问 NRewardsScreen 的私有字段，并进行玩家身份过滤，
    /// 确保联机模式下每个玩家只获取自己的奖励。
    /// </para>
    /// </summary>
    /// <param name="player">要获取奖励的玩家。</param>
    /// <returns>该玩家当前可见的奖励列表。</returns>
    public static List<Reward> GetCurrentRewards(Player player)
    {
        var result = new List<Reward>();
        if (player == null) return result;

        try
        {
            // 获取当前顶层的奖励屏幕
            if (NOverlayStack.Instance?.Peek() is not NRewardsScreen rewardsScreen)
                return result;

            InitializeReflection();

            // 多人安全校验：确认屏幕与玩家属于同一个运行状态
            if (_rewardsScreenRunStateField != null &&
                _rewardsScreenRunStateField.GetValue(rewardsScreen) != player.RunState)
                return result;

            if (_rewardButtonsField?.GetValue(rewardsScreen) is not System.Collections.IList buttonList)
                return result;

            foreach (var buttonObj in buttonList.Cast<object>().ToList())
            {
                if (buttonObj is not Control button) continue;

                Reward? reward = ExtractReward(button);
                if (reward?.Player == player)
                    result.Add(reward);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[UltraLib] 获取当前奖励出错: {ex.Message} / GetCurrentRewards error: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// 获取指定玩家当前所有的 <see cref="CardReward"/> 卡牌奖励。
    /// </summary>
    /// <param name="player">目标玩家。</param>
    /// <returns>卡牌奖励列表。</returns>
    public static List<CardReward> GetCurrentCardRewards(Player player)
        => GetCurrentRewards(player).OfType<CardReward>().ToList();

    /// <summary>
    /// 惰性初始化反射缓存。
    /// </summary>
    private static void InitializeReflection()
    {
        if (_reflectionInitialized) return;

        try
        {
            var screenType = typeof(NRewardsScreen);
            _rewardButtonsField = screenType.GetField("_rewardButtons",
                BindingFlags.NonPublic | BindingFlags.Instance);
            _rewardsScreenRunStateField = screenType.GetField("_runState",
                BindingFlags.NonPublic | BindingFlags.Instance);
            _reflectionInitialized = true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[UltraLib] 反射初始化失败: {ex.Message} / reflection init failed: {ex.Message}");
        }
    }

    /// <summary>
    /// 从 Control 按钮中提取 Reward 数据。
    /// </summary>
    private static Reward? ExtractReward(Control button)
    {
        if (button is NRewardButton rewardButton)
            return rewardButton.Reward;

        return null;
    }
}
