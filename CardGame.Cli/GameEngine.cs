using System;                                     // 引入基础命名空间，提供 Console、Exception 等类型
using System.Collections.Generic;                 // 引入泛型集合，如 List<T>
using System.Linq;                                // 引入 LINQ 扩展方法，如 Take、ToList、Sum

namespace CardGame.Cli                            // 命名空间：与项目保持一致，便于类型组织与引用
{
    public class GameEngine                       // 游戏引擎类：负责整局对战的管理（初始化、回合、结束）
    {
        private Deck deck = null!;                // 主牌库：整副 52+2（目前阶段使用标准牌）；null! 表示稍后会在 Setup() 内赋值
        private Deck publicPool = null!;          // 公共区（暗牌堆）：开局从主牌库抽 14 张放入
        private Player human = null!;             // 人类玩家对象：保存手牌与资源等
        // private Player computer = null!;       // 电脑玩家对象：保存手牌与资源等（后续可替换为 AIPlayer）
        private AIPlayer computer = null!;        // NEW: 将电脑玩家类型改为 AIPlayer，这样才能调用 AI 的决策方法

        public void Setup()                       // 对局初始化：按规则完成洗牌、发手牌、构建公共区，并打印校验信息
        {
            deck = Deck.CreateStandardDeck();     // ① 生成标准 52+2 牌堆（含 Joker1/Joker2，分值=0）
            deck.Shuffle();                       // ② 洗牌（规则要求开局洗牌）

            human = new Player("人类");            // ③ 创建人类玩家实例，名称仅用于输出
            // computer = new Player("电脑");      // ④ 创建电脑玩家实例（当前仍用 Player，后续可替换 AI 逻辑）
            computer = new AIPlayer("电脑");       // NEW: 电脑玩家使用 AIPlayer，这样后续回合里可调用 DecideMove()

            //TODO抛硬币决定抽牌顺序
            human.Draw(deck, 6);                  // ⑤ 人类抽 6 张起始手牌（规则：起手 6）
            computer.Draw(deck, 7);               // ⑥ 电脑抽 7 张（规则：后手 +1；此处暂定电脑为后手）

            var publicCards = new List<Card>();   // ⑦ 临时列表：收集 14 张公共区的牌（暗牌，不展示内容）
            for (int i = 0; i < 14; i++)          // ⑧ 循环 14 次，从主牌库依次抽牌
                publicCards.Add(deck.Draw());     // ⑨ 每次从主牌库顶端抽 1 张，加入公共区列表

            publicPool = new Deck(publicCards);   // ⑩ 用收集好的 14 张牌创建公共区牌堆（不洗，暗牌堆）

            // === 以下为校验输出，确保数量符合规则（便于你/设计师确认初始化正确） ===
            Console.WriteLine("===== 初始化完成 =====");                              // 分隔线
            Console.WriteLine($"人类手牌: {human.Hand.Count}（应为 6）");             // 校验：人类 6 张
            Console.WriteLine($"电脑手牌: {computer.Hand.Count}（应为 7）");          // 校验：电脑 7 张（后手 +1）
            Console.WriteLine($"公共区(暗牌): {publicPool.Count}（应为 14）");        // 校验：公共区 14 张
            Console.WriteLine($"主牌库剩余: {deck.Count}（应为 27）");                // 校验：54 - 6 - 7 - 14 = 27
        }

        public void PlayTurn(Player p)            // 执行单个玩家的一个回合（当前为“占位骨架”，后续逐步完善）
        {
            Console.WriteLine($"\n---- {p.Name} 的回合 ----");                       // 打印回合开始提示（\n 便于分隔回合）

            int toReveal = p.Hand.Count / 2;      // ① 计算需要“展示”的张数：手牌一半（向下取整）
            var revealed = p.Hand.Take(toReveal)  // ② 使用 LINQ 取前 toReveal 张牌作为“展示集合”（当前仅打印，不做状态区分）
                                 .ToList();       // ③ 物化为 List，便于后续多次使用
            Console.WriteLine($"{p.Name} 展示: {string.Join(", ", revealed)}（展示 {toReveal} 张）");  // ④ 打印展示列表（终端可见）

            // === 出牌阶段：根据是否为人类/AI 采用不同的选择方式（动作仍由引擎执行） ===
            if (p == human)                        // 人类分支：允许输入编号选择要出的牌（由 ReadHumanCardIndex 辅助）
            {
                if (p.Hand.Count > 0)              // 若手牌非空
                {
                    int idx = ReadHumanCardIndex(p);   // NEW: 读取人类输入的编号，循环校验直到有效
                    var card = p.Hand[idx];            // NEW: 根据编号取出要出的那张牌
                    p.Hand.RemoveAt(idx);              // NEW: 从手牌中移除这张牌（表示正式打出）
                    Console.WriteLine($"{p.Name} 打出: {card}");    // NEW: 打印人类打出的牌
                    // TODO：在这里调用“牌面效果/资源消耗/结算”的处理器，例如 EffectHandler.Apply(card, ...)
                }
                else                                // 若手牌为空
                {
                    Console.WriteLine($"{p.Name} 无牌可出 → 从公共区抽一张");  // 无法出牌时必须抽牌
                    if (publicPool.Count > 0)       // 公共区还有牌可抽
                        p.Draw(publicPool, 1);      // 抽 1 张补充到手牌（本回合仅抽，不继续出）
                    else
                        Console.WriteLine("公共区已空。"); // 公共区没有牌，打印提示（后续 IsGameOver 会据此判断终局）
                }
            }
            else                                    // NEW: AI 分支 —— 让 AI 决策“出哪张牌”（返回索引），由引擎执行移除/打印
            {
                var ai = (AIPlayer)p;              // NEW: 将通用 Player 强制转换为 AIPlayer，以便调用 DecideMove()
                int idx = ai.DecideMove();         // NEW: 让 AI 决策（你当前的 AI 始终返回 0 → 永远出第一张）
                if (idx >= 0)                      // NEW: idx >= 0 表示有牌可出
                {
                    var card = p.Hand[idx];        // NEW: 根据 AI 返回的索引，取出要打出的那张牌
                    p.Hand.RemoveAt(idx);          // NEW: 从手牌中移除该牌（实际的“出牌动作”仍由引擎负责）
                    Console.WriteLine($"{p.Name} 打出: {card}");  // NEW: 打印 AI 打出的牌
                    // TODO：在这里调用“牌面效果/资源消耗/结算”的处理器，例如 EffectHandler.Apply(card, ...)
                }
                else                                // NEW: idx == -1 → 无牌可出，按规则从公共区抽一张
                {
                    Console.WriteLine($"{p.Name} 无牌可出 → 从公共区抽一张");  // NEW: 无牌可出提示
                    if (publicPool.Count > 0)       // NEW: 公共区还有牌
                        p.Draw(publicPool, 1);      // NEW: 抽 1 张补充到手牌（本回合仅抽，不继续出）
                    else
                        Console.WriteLine("公共区已空。"); // NEW: 公共区没有牌
                }
            }

            Console.WriteLine($"{p.Name} 回合结束。当前手牌数: {p.Hand.Count}");  // ⑭ 回合结束提示（后续要在此实现“洗手牌并重新选择展示”）
        }

        // NEW: 读取人类输入的牌索引（循环直到输入有效）
        private int ReadHumanCardIndex(Player p)
        {
            Console.WriteLine("请选择要出的牌编号：");                // NEW: 引导提示
            for (int i = 0; i < p.Hand.Count; i++)                  // NEW: 遍历手牌，为每张牌标上编号
            {
                Console.WriteLine($"  [{i}] {p.Hand[i]} (分值={p.Hand[i].Value})"); // NEW: 显示编号 + 牌面 + 分值
            }

            while (true)                                            // NEW: 循环读取直到输入有效
            {
                Console.Write("你的选择：");                         // NEW: 输入提示
                var input = Console.ReadLine();                     // NEW: 读取一行字符串
                if (int.TryParse(input, out int idx)                // NEW: 尝试解析为整数 idx
                    && idx >= 0 && idx < p.Hand.Count)              // NEW: 判断 idx 是否在有效范围内
                    return idx;                                     // NEW: 返回有效的索引
                Console.WriteLine("输入无效，请输入有效编号（例如 0、1、2 ...）。"); // NEW: 错误提示并继续循环
            }
        }

        // 计算某玩家当前手牌的“调试用总分”（A=1..K=13, Joker=0）
        // 注意：这只是当前时刻的快照，不是最终胜负结果。
        private int SumHand(Player p)
        {
            // p.Hand 中每张牌的 Value 已按规则设定（Joker=0）
            return p.Hand.Sum(c => c.Value);
        }

        // NEW: 设计规则版“是否游戏结束”判定
        // 规则（正式）：当 公共区=0 时立刻结束，不再继续后续回合
        private bool IsGameOverByDesign()
        {
            return publicPool.Count == 0;          // NEW: 只检测公共区是否被抽空
        }

        // NEW: 将玩家手牌拼成字符串，便于结算时展示
        private string HandToString(Player p)
        {
            return p.Hand.Count == 0               // NEW: 如果没有牌
                ? "(无牌)"                         // NEW: 返回占位文本
                : string.Join(", ", p.Hand);       // NEW: 否则把每张牌的 ToString() 用逗号连接
        }

        // NEW: 正式结算：展示双方剩余手牌与总分，并判定胜负
        private void FinalSettlement()
        {
            Console.WriteLine("\n===== 正式结算（公共区已抽空）=====");      // NEW: 结算标题

            Console.WriteLine($"人类剩余手牌：{HandToString(human)}");      // NEW: 展示人类剩余手牌
            Console.WriteLine($"电脑剩余手牌：{HandToString(computer)}");   // NEW: 展示电脑剩余手牌

            int humanScore = SumHand(human);                                // NEW: 计算人类总分
            int aiScore = SumHand(computer);                                // NEW: 计算电脑总分

            Console.WriteLine($"人类总分：{humanScore}");                   // NEW: 打印人类分
            Console.WriteLine($"电脑总分：{aiScore}");                      // NEW: 打印电脑分

            if (humanScore > aiScore)                                       // NEW: 比较分数判定胜负
                Console.WriteLine("结果：人类胜！");
            else if (humanScore < aiScore)
                Console.WriteLine("结果：电脑胜！");
            else
                Console.WriteLine("结果：平局！");
        }

        public void Start()                        // 启动一局对战（现在：循环交替，公共区抽空即结算）
        {
            Setup();                               // ① 执行初始化（洗牌、发手牌、构建公共区、打印校验信息）

            int round = 1;                         // NEW: 轮次计数（“人类→电脑”算一轮）
            int safety = 0;                        // NEW: 安全计数，防止意外死循环

            while (true)                           // NEW: 交替回合，直到公共区被抽空
            {
                Console.WriteLine($"\n===== 回合 {round} ====="); // NEW: 打印回合号，便于观察流程

                PlayTurn(human);                   // ② 人类回合
                if (IsGameOverByDesign()) break;  // NEW: 若人类回合中把公共区抽空 → 立刻结束（不再给电脑回合）

                PlayTurn(computer);                // ③ 电脑回合
                if (IsGameOverByDesign()) break;  // NEW: 若电脑回合中把公共区抽空 → 立刻结束

                // NEW: 每轮结束打印一次简要状态
                Console.WriteLine($"--- 状态：人类手牌 {human.Hand.Count}，电脑手牌 {computer.Hand.Count}，公共区 {publicPool.Count} ---");

                round++;                           // NEW: 轮次 +1
                safety++;                          // NEW: 安全计数 +1
                if (safety > 500)                  // NEW: 超过 500 轮则强行中断，防止意外死循环（极端保险）
                {
                    Console.WriteLine("\n[安全中断] 回合数超过上限（500）。");
                    break;
                }
            }

            FinalSettlement();                     // NEW: 公共区抽空后进行正式结算（展示手牌、统计总分、判胜负）
        }
    }
}
