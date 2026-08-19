using System.Reflection;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;

namespace UltraLib.Base.Utils;

/// <summary>
/// AttackCommand 构建辅助方法。
/// </summary>
public static class AttackHelper
{
    private static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    public static AttackCommand SetAttacker(this AttackCommand cmd, Creature attacker)
    {
        typeof(AttackCommand).GetProperty("Attacker", Flags)?.SetValue(cmd, attacker);
        return cmd;
    }

    public static AttackCommand SetDamageProps(this AttackCommand cmd, ValueProp prop)
    {
        typeof(AttackCommand).GetProperty("DamageProps", Flags)?.SetValue(cmd, prop);
        return cmd;
    }

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
            Godot.GD.PrintErr($"[UltraLib] ClearTargets reflection error: {ex.Message}");
        }

        return cmd;
    }
}
