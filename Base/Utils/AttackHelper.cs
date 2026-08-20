using System.Reflection;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;

namespace UltraLib.Base.Utils;

/// <summary>
/// AttackCommand building helpers.
/// </summary>
/// <remarks>
/// AttackCommand 构建辅助方法。
/// </remarks>
public static class AttackHelper
{
    private static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    /// <summary>
    /// Sets the attacker of an AttackCommand (via reflection).
    /// </summary>
    /// <remarks>
    /// 通过反射设置攻击命令（AttackCommand）的发起者（Attacker）。
    /// </remarks>
    /// <param name="cmd">要修改的攻击命令。</param>
    /// <param name="attacker">发起攻击的生物。</param>
    /// <returns>修改后的攻击命令，便于链式调用。</returns>
    public static AttackCommand SetAttacker(this AttackCommand cmd, Creature attacker)
    {
        typeof(AttackCommand).GetProperty("Attacker", Flags)?.SetValue(cmd, attacker);
        return cmd;
    }

    /// <summary>
    /// Sets the damage properties of an AttackCommand (via reflection).
    /// </summary>
    /// <remarks>
    /// 通过反射设置攻击命令（AttackCommand）的伤害属性（DamageProps）。
    /// </remarks>
    /// <param name="cmd">要修改的攻击命令。</param>
    /// <param name="prop">要设置的伤害属性标记。</param>
    /// <returns>修改后的攻击命令，便于链式调用。</returns>
    public static AttackCommand SetDamageProps(this AttackCommand cmd, ValueProp prop)
    {
        typeof(AttackCommand).GetProperty("DamageProps", Flags)?.SetValue(cmd, prop);
        return cmd;
    }

    /// <summary>
    /// Adds extra damage properties to an AttackCommand (via reflection, OR-combined).
    /// </summary>
    /// <remarks>
    /// 通过反射给攻击命令（AttackCommand）追加额外伤害属性（按位或合并）。
    /// </remarks>
    /// <param name="cmd">要修改的攻击命令。</param>
    /// <param name="prop">要追加的伤害属性标记。</param>
    /// <returns>修改后的攻击命令，便于链式调用。</returns>
    public static AttackCommand AddDamageProps(this AttackCommand cmd, ValueProp prop)
    {
        var pi = typeof(AttackCommand).GetProperty("DamageProps", Flags);
        if (pi != null)
        {
            var current = (ValueProp)pi.GetValue(cmd);
            pi.SetValue(cmd, current | prop);
        }
        return cmd;
    }

    /// <summary>
    /// Modifies the single final damage amount of an AttackCommand (via reflection).
    /// </summary>
    /// <remarks>
    /// 通过反射修改攻击命令（AttackCommand）的单次最终伤害值。
    /// 若尚未计算最终伤害，则直接修改基础伤害字段；否则修改计算伤害变量的基础值。
    /// </remarks>
    /// <param name="cmd">要修改的攻击命令。</param>
    /// <param name="damageModifier">对伤害值进行转换的函数。</param>
    /// <returns>修改后的攻击命令，便于链式调用。</returns>
    public static AttackCommand ModifySingleFinalDamage(this AttackCommand cmd, Func<decimal, decimal> damageModifier)
    {
        var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        FieldInfo? dmgField = typeof(AttackCommand).GetField("_damagePerHit", flags);
        decimal baseAmount = dmgField != null ? (decimal)dmgField.GetValue(cmd) : 0M;

        FieldInfo? calcField = typeof(AttackCommand).GetField("_calculatedDamageVar", flags);
        object? calcVarInstance = calcField?.GetValue(cmd);

        if (baseAmount < 0M && calcVarInstance != null)
        {
            FieldInfo? varDmgField = calcVarInstance.GetType().GetField("_baseDamage", flags)
                                     ?? calcVarInstance.GetType().GetField("baseDamage", flags)
                                     ?? calcVarInstance.GetType().GetField("_damage", flags);
            if (varDmgField != null)
            {
                decimal currentVarBase = Convert.ToDecimal(varDmgField.GetValue(calcVarInstance));
                varDmgField.SetValue(calcVarInstance, damageModifier(currentVarBase));
            }
        }
        else if (dmgField != null)
        {
            decimal newFinal = damageModifier(baseAmount);
            dmgField.SetValue(cmd, newFinal);
            calcField?.SetValue(cmd, null);
        }

        return cmd;
    }

    /// <summary>
    /// Clears the targets of an AttackCommand (via reflection).
    /// </summary>
    /// <remarks>
    /// 通过反射清除攻击命令（AttackCommand）的攻击目标：清空单个目标、战斗状态、
    /// 随机目标标志与目标侧。
    /// </remarks>
    /// <param name="cmd">要修改的攻击命令。</param>
    /// <returns>修改后的攻击命令，便于链式调用。</returns>
    public static AttackCommand ClearTargets(this AttackCommand cmd)
    {
        if (cmd == null) return null!;

        var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        var type = typeof(AttackCommand);

        try
        {
            type.GetField("_singleTarget", flags)?.SetValue(cmd, null);
            type.GetField("_combatState", flags)?.SetValue(cmd, null);

            var randomProp = type.GetProperty("IsRandomlyTargeted", flags) ?? type.GetProperty("IsRandomlyTargeted");
            if (randomProp != null && randomProp.CanWrite)
                randomProp.SetValue(cmd, false);
            else
                type.GetField("<IsRandomlyTargeted>k__BackingField", flags)?.SetValue(cmd, false);

            var sideProp = type.GetProperty("TargetSide", flags) ?? type.GetProperty("TargetSide");
            if (sideProp != null && sideProp.CanWrite)
                sideProp.SetValue(cmd, default);
            else
                type.GetField("<TargetSide>k__BackingField", flags)?.SetValue(cmd, default);
        }
        catch (Exception ex)
        {
            Godot.GD.PrintErr($"[UltraLib] 清除目标反射错误: {ex.Message} / ClearTargets reflection error: {ex.Message}");
        }

        return cmd;
    }
}
