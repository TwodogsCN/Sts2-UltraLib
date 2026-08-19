using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;

namespace UltraLib.Base.Utils;

/// <summary>
/// 远古事件（AncientEvent）相关辅助方法。
/// </summary>
public static class AncientEventHelper
{
    /// <summary>
    /// 通过反射调用 AncientEventModel 的私有方法 RelicOption，
    /// 为指定遗物类型 T 创建一个事件选项。
    /// </summary>
    /// <typeparam name="T">遗物模型类型。</typeparam>
    /// <param name="instance">远古事件模型实例。</param>
    /// <returns>生成的事件选项。</returns>
    public static EventOption CreateRelicOption<T>(this AncientEventModel instance) where T : RelicModel
    {
        var method = AccessTools.Method(typeof(AncientEventModel), "RelicOption",
            [typeof(RelicModel), typeof(string), typeof(string)]);

        var relic = ModelDb.Relic<T>().ToMutable();
        return (EventOption)method.Invoke(instance, [relic, "INITIAL", null]);
    }
}
