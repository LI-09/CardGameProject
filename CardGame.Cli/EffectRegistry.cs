// EffectRegistry.cs
// 作用：把“所有卡牌效果”的注册集中到一个地方，程序启动前调用一次。
// 重要：在你的项目中：
//   - Card.Suit 是“字符串”，例如 "♠" "♥" "♦" "♣"；Joker 可能是 " "（空格）
//   - Card.Rank 是“字符串”，例如 "A" "7" "Q" …
//   - Card.Value 是“数字”（int），A=1…K=13，Joker=0
// 因此判断红心要用 card.Suit == "♥"，判断 7 点建议用 card.Value == 7。

using System;            // 提供 Console.WriteLine 等基础功能
using CardGame.Cli;      // 引用项目命名空间（访问 Card / Player / GameEngine / CardEffects等）

namespace CardGame.Cli   // 命名空间必须与项目一致，确保其他文件能找到这里的类型
{
    /// <summary>
    /// 效果注册器（集中登记规则）。外部只需要在启动前调用一次 RegisterAll()。
    /// </summary>
    public static class EffectRegistry
    {
        /// <summary>
        /// 注册所有需要的效果规则。
        /// 以后新增/删除规则，都在这里增删 CardEffects.Register(...) 即可。
        /// </summary>
        public static void RegisterAll()
        {
            // =============== 规则 1：出牌为红心（♥） → 打印提示（占位，不改数值） ===============

            CardEffects.Register(                               // 向框架登记一条规则
                new CardEffect(                                 // 创建规则对象
                    id: "onplay.hearts.echo",                   // 规则唯一 ID，方便定位/开关
                    trigger: CardTrigger.OnPlay,                // 触发时机：出牌时
                    when: (card, player, game) =>               // 命中条件：当……
                    {
                        return card.Suit == "♥";                // ……这张牌的花色是红心（注意：Suit 是“字符串”）
                    },
                    action: (card, player, game) =>             // 动作：命中后做什么
                    {
                        Console.WriteLine($"[效果] {player.Name} 打出红心 {card}（占位提示）");
                        // 将来你可以把这里改成真实效果：加资源/扣分/改变状态等
                    },
                    note: "出牌=红心时触发，占位效果"            // 备注，给查看代码的人看的说明
                )
            );

            // =============== 规则 2：出牌点数为 7 → 打印提示（占位，不改数值） ===============

            CardEffects.Register(
                new CardEffect(
                    id: "onplay.value7.echo",                   // 规则唯一 ID：出牌点数=7 提示
                    trigger: CardTrigger.OnPlay,                // 触发时机：出牌时
                    when: (card, player, game) =>               // 命中条件：
                    {
                        return card.Value == 7;                 // 用“数值”比较最稳妥（Value 是 int）
                        // 备选：return card.Rank == "7";      // 若你更喜欢用 Rank（字符串）判断
                    },
                    action: (card, player, game) =>
                    {
                        Console.WriteLine($"[效果] {player.Name} 打出点数 7：{card}（占位提示）");
                    },
                    note: "出牌=7 时触发，占位效果"
                )
            );

            // =============== 以后在这里继续追加你的真实规则 ===============
            // 例如（示例占位，先注释掉）：
            //
            // CardEffects.Register(
            //     new CardEffect(
            //         id: "onplay.clubs.extra-draw",
            //         trigger: CardTrigger.OnPlay,
            //         when: (card, player, game) => card.Suit == "♣",   // 出牌是梅花
            //         action: (card, player, game) =>
            //         {
            //             Console.WriteLine($"[效果] {player.Name} 打出梅花 {card} → 触发【额外抽 1】（占位）");
            //             // TODO：未来替换成真实逻辑：从主牌库再抽 1 张等
            //         },
            //         note: "出牌=梅花：示例规则（未来可替换为真实效果）"
            //     )
            // );
        }
    }
}
