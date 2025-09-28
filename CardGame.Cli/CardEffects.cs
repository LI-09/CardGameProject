// CardEffects.cs
// 这个文件的作用：提供一个“卡牌效果系统”的框架。
// 它允许我们为特定的牌（点数、花色等）定义一些特殊效果。
// 例如：如果某人打出了红心 ♥，可以触发一条规则（加分、提示、改变状态等）。
// 好处：不需要改动核心流程，只要在这里登记规则即可。

using System;   // 引入基础功能（如 Console.WriteLine 打印信息）
using System.Collections.Generic;   // 引入集合类型（如 List、Dictionary）

namespace CardGame.Cli   // 命名空间，确保这个文件属于项目 CardGame.Cli
{
    /// <summary>
    /// 效果触发时机（什么时候检查效果）。
    /// OnPlay 表示“出牌时”，OnDraw 表示“抽到牌时”。
    /// 后续如果想扩展别的时机（比如洗牌时），也可以在这里加。
    /// </summary>
    public enum CardTrigger
    {
        OnPlay, // 出牌时触发
        OnDraw  // 抽到时触发
    }

    /// <summary>
    /// 下面两个 delegate（委托）定义了“条件”和“动作”的模板。
    /// - CardPredicate：传入当前的牌、玩家、引擎 → 返回 true/false，表示是否命中效果。
    /// - CardAction：传入当前的牌、玩家、引擎 → 执行具体效果（比如加分、打印信息等）。
    /// </summary>
    public delegate bool CardPredicate(Card card, Player player, GameEngine game);
    public delegate void CardAction(Card card, Player player, GameEngine game);

    /// <summary>
    /// 一条“效果规则”。
    /// 每条规则有：唯一标识 Id、触发时机 Trigger、触发条件 When、执行动作 Do、备注 Note。
    /// </summary>
    public sealed class CardEffect
    {
        public string Id { get; }           // 规则唯一 ID，方便调试和管理
        public CardTrigger Trigger { get; } // 规则的触发时机（出牌 / 抽牌）
        public CardPredicate When { get; }  // 判定条件：什么时候算“触发”
        public CardAction Do { get; }       // 真正的效果动作
        public string? Note { get; }        // 可选备注（说明这条规则干嘛的）

        // 构造函数：用来创建一条新的规则
        public CardEffect(string id, CardTrigger trigger, CardPredicate when, CardAction action, string? note = null)
        {
            Id = id;         // 赋值 ID
            Trigger = trigger;  // 赋值触发时机
            When = when;        // 赋值条件
            Do = action;        // 赋值动作
            Note = note;        // 赋值备注（可以为空）
        }
    }

    /// <summary>
    /// 整个效果系统的“管理中心”。
    /// - 负责登记（Register）规则。
    /// - 负责在合适的时机执行（Run）规则。
    /// 我们用一个静态类，这样所有人都能方便地访问，不需要实例化。
    /// </summary>
    public static class CardEffects
    {
        // 内部存储规则的地方。
        // 用 Dictionary（字典）来按触发时机分类存放。
        // 比如：OnPlay → 一堆规则；OnDraw → 另一堆规则。
        private static readonly Dictionary<CardTrigger, List<CardEffect>> _rules = new()
        {
            [CardTrigger.OnPlay] = new List<CardEffect>(), // 存放出牌时的规则
            [CardTrigger.OnDraw] = new List<CardEffect>()  // 存放抽牌时的规则
        };

        /// <summary>
        /// 登记一条规则的方法。
        /// 比如：CardEffects.Register(new CardEffect(...))。
        /// 这样这条规则就会被存到 _rules 中。
        /// </summary>
        public static void Register(CardEffect effect)
        {
            _rules[effect.Trigger].Add(effect); // 按触发时机加入对应的规则表
        }

        /// <summary>
        /// 在合适的时机触发规则。
        /// 比如出牌后，我们会调用：CardEffects.Run(CardTrigger.OnPlay, card, player, game)。
        /// 这个方法会自动检查所有 OnPlay 规则，依次执行符合条件的动作。
        /// </summary>
        public static void Run(CardTrigger trigger, Card card, Player player, GameEngine game)
        {
            // 找到对应触发时机的规则列表
            var list = _rules[trigger];

            // 如果没有任何规则，直接返回（零开销）
            if (list.Count == 0) return;

            // 遍历每一条规则
            foreach (var rule in list)
            {
                try
                {
                    // 检查条件：When 返回 true 表示命中
                    if (rule.When(card, player, game))
                    {
                        // 执行动作：Do（比如打印提示、修改资源等）
                        rule.Do(card, player, game);
                    }
                }
                catch (Exception ex)
                {
                    // 如果某条规则报错，不让游戏崩掉，打印提示继续
                    Console.WriteLine($"[效果执行异常] {rule.Id}: {ex.Message}");
                }
            }
        }
    }
}
