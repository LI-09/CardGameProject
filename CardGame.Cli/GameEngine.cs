// 引入基础命名空间，提供控制台、集合等类型
using System;                                     // Console.WriteLine / Console.ReadLine 等
using System.Collections.Generic;                 // List<T> 等泛型集合
using System.Linq;                                // 提供 LINQ 扩展：Take/Select/Sum/ToList 等

namespace CardGame.Cli                             // 命名空间：与项目其他文件一致，便于类型查找与组织
{
    /// <summary>
    /// GameEngine：整局游戏的“导演”
    /// - 负责：初始化发牌、每回合的玩家行动（展示/出牌/补牌）、公共区抽牌、结束结算
    /// - 依赖：Deck（牌堆）、Player / AIPlayer（玩家）、Card（牌）、CardEffects（效果钩子，可选）
    /// </summary>
    public class GameEngine
    {
        // ======== 字段（整局共享的状态） ========

        private Deck mainDeck;                     // 主牌库：开局 54 张（52+2 Joker），发完人类/AI/公共区后剩余 27 张；出牌后从这里“补牌”
        private Deck publicDeck;                   // 公共区（暗牌堆）：开局从主牌库抽 14 张放入；当某一方手牌为 0 时，从这里抽 1 张
        private Player human;                      // 人类玩家：需要手动选择展示与出牌
        private AIPlayer ai;                       // 电脑玩家（AI）：展示自动（前一半），出牌用简单策略（通常“出第一张”）
        private int round;                         // 当前回合号：从 1 开始，便于在输出中定位

        // ======== 构造：完成洗牌/发牌/公共区构建，并给出数目校验 ========
        public GameEngine()                        // 创建 GameEngine 的同时，马上把游戏状态准备好
        {
            // ⚠ 修正点 1：你的 Deck 没有无参构造，这里用工厂方法创建标准牌堆
            mainDeck = Deck.CreateStandardDeck();  // ① 生成标准 52+2 牌堆（含 Joker1/Joker2，分值=0）
            mainDeck.Shuffle();                    // ② 洗牌：保证后续抽牌顺序随机

            // ③ 构建公共区：一次性从主牌库抽 14 张作为暗牌堆（不展示内容）
            //    ⚠ 修正点 2：你的 Deck 不存在 DrawMany，这里用循环 Draw 14 次再 new Deck(list)
            var publicCards = new List<Card>();    // 临时列表：收集 14 张公共区的牌
            for (int i = 0; i < 14; i++)           // 循环 14 次
            {
                publicCards.Add(mainDeck.Draw());  // 每次从主牌库顶端抽 1 张，放入公共区列表
            }
            publicDeck = new Deck(publicCards);    // 用收集好的 14 张牌创建公共区牌堆（暗牌，不洗）

            // ④ 创建两个玩家对象（仅用于保存手牌等信息；名字用于输出）
            human = new Player("人类");            // 人类：手动交互
            ai = new AIPlayer("电脑");             // 电脑：自动策略

            // ⑤ 发起手牌：人类 6 张、电脑 7 张（“后手+1”的占位规则，便于测试）
            human.Draw(mainDeck, 6);               // 从主牌库抽 6 张到人类手牌
            ai.Draw(mainDeck, 7);                  // 从主牌库抽 7 张到电脑手牌

            round = 1;                             // ⑥ 回合计数器从 1 开始
        }

        // ======== 整局入口：打印初始化信息 → 轮流行动 → 公共区抽空后结算 ========
        public void Start()                        // 对外调用入口：Program.cs 会调用 engine.Start()
        {
            // --- 初始化状态校验打印（方便设计检查是否符合规则） ---
            Console.WriteLine("===== 初始化完成 =====");                                // 标题分隔
            Console.WriteLine($"人类手牌: {human.Hand.Count}（应为 6）");               // 应该是 6
            Console.WriteLine($"电脑手牌: {ai.Hand.Count}（应为 7）");                  // 应该是 7
            Console.WriteLine($"公共区(暗牌): {publicDeck.Count}（应为 14）");          // 应该是 14
            Console.WriteLine($"主牌库剩余: {mainDeck.Count}（应为 27）");              // 应该是 27（54-6-7-14=27）
            Console.WriteLine();                                                       // 空行分隔

            // --- 主循环：人类→电脑 为一轮；当公共区抽空后进入结算 ---
            //     为了兼容中途有人打空手牌等情况，这里循环条件选择“仍有牌可玩”。
            while (publicDeck.Count > 0 || human.Hand.Count > 0 || ai.Hand.Count > 0)  // 只要还有资源/手牌，就继续推进
            {
                Console.WriteLine($"===== 回合 {round} =====");                        // 回合标题

                // 1) 人类回合
                TakeTurn(human);                                                       // 人类：手动选择展示与出牌

                // 如果此时双方手牌都空且公共区也空，说明已经没有资源可玩，提前结束
                if (publicDeck.Count == 0 && human.Hand.Count == 0 && ai.Hand.Count == 0)
                    break;                                                             // 立即跳出循环，进入结算

                // 2) 电脑回合
                TakeTurn(ai);                                                          // 电脑：自动展示、自动选牌

                // 回合尾打印一个状态汇总，方便观察推进情况
                Console.WriteLine($"--- 状态：人类手牌 {human.Hand.Count}，电脑手牌 {ai.Hand.Count}，公共区 {publicDeck.Count} ---");
                Console.WriteLine();                                                   // 空行分隔

                round++;                                                               // 下一回合
            }

            // 走到这里意味着：公共区被抽空（或双方都无牌），进入结算并打印结果
            EndGame();                                                                 // 结算与胜负判定
        }

        // ======== 执行“某个玩家”的一个完整回合：展示 → 出牌/抽公共区 → 出牌后补主牌库 ========
        private void TakeTurn(Player p)                                                // p：当前行动玩家（可能是 human 或 ai）
        {
            Console.WriteLine();                                                       // 在上一个输出块与本回合之间留空行
            Console.WriteLine($"---- {p.Name} 的回合 ----");                           // 回合标题（“人类/电脑 的回合”）

            // A) 若回合开始时“手牌为 0”：根据规则，直接从公共区抽 1 张，本回合结束（不出牌）
            if (p.Hand.Count == 0)                                                     // 检查是否无手牌
            {
                if (publicDeck.Count > 0)                                              // 公共区是否还有牌
                {
                    // 这里也可以用 p.Draw(publicDeck, 1)；保持直观演示抽牌过程
                    var drawn = publicDeck.Draw();                                     // 从公共区顶端抽 1 张
                    p.Hand.Add(drawn);                                                 // 补到手牌
                    Console.WriteLine($"{p.Name} 无牌可出 → 从公共区抽一张");           // 打印提示
                }
                else                                                                   // 公共区也空了
                {
                    Console.WriteLine($"{p.Name} 无牌可出 → 公共区为空");               // 只能提示
                }
                Console.WriteLine($"{p.Name} 回合结束。当前手牌数: {p.Hand.Count}");    // 打印回合收尾的手牌数
                return;                                                                // 本回合到此结束（不进入展示/出牌）
            }

            // B) 展示阶段（展示“手牌的一半”，向下取整）
            List<Card> revealed;                                                       // 用于保存“将要展示”的那一组牌
            if (p is AIPlayer)                                                         // 如果是电脑（AI）
            {
                // 电脑：自动展示“手牌前一半”（简单可预测，便于测试）
                revealed = RevealByAuto(p);                                            // 计算需要展示的数量 + 取前半段
                Console.WriteLine($"{p.Name} 展示: {string.Join(", ", revealed)}（展示 {revealed.Count} 张）");
            }
            else                                                                        // 否则是人类
            {
                // 人类：**手动选择**要展示的牌（数量必须等于手牌的一半）
                revealed = RevealByHumanChoice(p);                                     // 引导输入 indices，做数量与范围校验
                Console.WriteLine($"{p.Name} 选择展示: {string.Join(", ", revealed)}（展示 {revealed.Count} 张）");
            }

            // C) 出牌阶段（人类与 AI 分别处理“如何选牌”，其余逻辑一致）
            Card playedCard;                                                           // 保存“打出的那张”牌
            if (p is AIPlayer aiPlayer)                                                // 电脑出牌流程
            {
                // ⚠ 修正点 3：你的 AI 为无参 DecideMove()，不要传 hand 参数
                int idx = aiPlayer.DecideMove();                                       // 让 AI 决定要出的手牌索引（当前实现可能固定为 0）
                playedCard = p.Hand[idx];                                              // 取到要打出的牌
                p.Hand.RemoveAt(idx);                                                  // 从手牌里移除（表示“打出”）
                Console.WriteLine($"{p.Name} 打出: {playedCard}");                     // 打印出牌信息
            }
            else                                                                        // 人类出牌流程
            {
                int idx = ReadHumanCardIndex(p.Hand);                                  // 循环读取输入直到有效（0..hand.Count-1）
                playedCard = p.Hand[idx];                                              // 取到要打出的牌
                p.Hand.RemoveAt(idx);                                                  // 从手牌移除
                Console.WriteLine($"{p.Name} 打出: {playedCard}");                     // 打印出牌信息
            }

            // （可选）触发“出牌时”的效果钩子：目前我们只做提示，不改变手牌数与分值
            // 说明：如果你暂时不想看到效果提示（比如压测 IO），可以把这一行注释掉
            CardEffects.Run(CardTrigger.OnPlay, playedCard, p, this);                  // 触发效果系统（例如 “♥” 或 “点数 7” 打出时的占位提示）

            // D) 出牌后补牌：只要主牌库还有牌，就从“主牌库”抽 1 张
            if (mainDeck.Count > 0)                                                    // 主牌库还有牌
            {
                // 这里也可以用 p.Draw(mainDeck, 1)；为展示抽牌过程，保持手动 Add
                var drawn = mainDeck.Draw();                                           // 从主牌库顶端抽 1 张
                p.Hand.Add(drawn);                                                     // 补回到玩家手牌
                Console.WriteLine($"{p.Name} 从主牌库补 1 张（主牌库剩余: {mainDeck.Count}）"); // 打印补牌信息
            }
            else                                                                        // 主牌库已经空了
            {
                Console.WriteLine("主牌库已空，无法补牌。");                            // 只提示，不影响流程
            }

            // E) 回合结束状态打印（便于观察是否保持了“恒定手牌数”的规则）
            Console.WriteLine($"{p.Name} 回合结束。当前手牌数: {p.Hand.Count}");         // 打印手牌数量
        }

        // ======== 人类：手动选择“展示的一半” ========
        // 交互形式：
        // 1) 先把整手牌列出来并编号；
        // 2) 提示需要展示的数量（手牌一半，向下取整）；
        // 3) 让玩家用“空格分隔的编号”输入（例如：0 2 5）；
        // 4) 做数量与范围校验，不通过就反复提示直至正确。
        private List<Card> RevealByHumanChoice(Player p)                                // 返回：被展示的那组牌（并不移出手牌）
        {
            int need = p.Hand.Count / 2;                                               // 规则：展示“手牌的一半”，向下取整
            if (need == 0)                                                             // 如果手牌为 0 或 1，need 可能为 0
            {
                Console.WriteLine($"{p.Name} 展示: （展示 0 张）");                      // 按统一格式提示“展示 0 张”
                return new List<Card>();                                               // 返回空列表
            }

            // 打印“可选清单”给人类参考
            Console.WriteLine($"请选择 {need} 张要展示的牌（输入编号，用空格分隔）：");  // 提示要求
            for (int i = 0; i < p.Hand.Count; i++)                                     // 遍历整手牌
            {
                Console.WriteLine($"  [{i}] {p.Hand[i]}");                             // 显示：编号 + 牌面（例如 [0] 7♣）
            }

            // 读取输入并严格校验：数量必须等于 need，且每个编号都在合法范围内
            while (true)                                                               // 不断重试直到输入有效
            {
                Console.Write("你的选择：");                                            // 输入提示前缀
                var line = Console.ReadLine();                                         // 读一整行（可能含多个编号）
                if (string.IsNullOrWhiteSpace(line))                                   // 空输入 → 重试
                {
                    Console.WriteLine("输入为空，请重新输入。");                        // 反馈错误
                    continue;                                                          // 回到 while 开头
                }

                // 拆分空格，过滤空项，尝试把每一段转为整数编号
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);    // 以空格拆分
                var ok = true;                                                         // 标记：假设有效，检验若失败再改为 false
                var idxs = new List<int>();                                            // 临时存放“解析出的编号”

                foreach (var s in parts)                                               // 遍历每一段字符串
                {
                    if (!int.TryParse(s, out int idx))                                 // 不能解析成整数 → 非法
                    {
                        ok = false;                                                    // 标记失败
                        break;                                                         // 跳出循环
                    }
                    if (idx < 0 || idx >= p.Hand.Count)                                // 超出有效编号范围
                    {
                        ok = false;                                                    // 标记失败
                        break;                                                         // 跳出循环
                    }
                    idxs.Add(idx);                                                     // 记录一个合法编号
                }

                // 数量必须“恰好等于 need”（多/少都不行）；同时可以去重（不允许重复编号）
                if (ok)                                                                // 如果前面每一项都合法
                {
                    idxs = idxs.Distinct().ToList();                                   // 去重：避免同一张牌被重复展示
                    if (idxs.Count != need)                                            // 数量不等于需求
                    {
                        Console.WriteLine($"需要选择 {need} 张，当前选了 {idxs.Count} 张，请重新输入。");
                        ok = false;                                                    // 标记失败，继续 while
                    }
                }

                if (!ok)                                                               // 若失败，继续下一轮输入
                {
                    Console.WriteLine("输入格式或编号范围不正确，请按示例：0 2 5");
                    continue;                                                          // 回到 while 开头
                }

                // 走到这里：说明输入“通过校验”，据此取出需要展示的牌并返回
                var revealed = idxs.Select(i => p.Hand[i]).ToList();                   // 把编号映射成牌对象列表
                return revealed;                                                       // 返回展示牌组（注意：没从手牌移除）
            }
        }

        // ======== 电脑：自动展示“前一半” ========
        // 逻辑很简单：按当前手牌顺序，直接取前 need 张作为“展示”。
        private List<Card> RevealByAuto(Player p)                                      // 返回：被展示的那组牌（不移出手牌）
        {
            int need = p.Hand.Count / 2;                                               // 需要展示的数量
            if (need <= 0)                                                             // 如果无需展示（手牌为 0 或 1）
            {
                return new List<Card>();                                               // 返回空列表
            }
            var revealed = p.Hand.Take(need).ToList();                                 // 直接“取前 need 张”
            return revealed;                                                           // 返回展示牌组（不移除）
        }

        // ======== 人类：读取“要打出的牌”的编号（0..hand.Count-1），循环校验直到合法 ========
        private int ReadHumanCardIndex(List<Card> hand)                                // hand：当前人类的整手牌
        {
            Console.WriteLine("请选择要出的牌编号：");                                   // 提示输入

            // 打印可选清单：编号 + 牌面 + 分值（帮助人类做选择）
            for (int i = 0; i < hand.Count; i++)                                       // 逐个打印
            {
                Console.WriteLine($"  [{i}] {hand[i]} (分值={hand[i].Value})");        // 如：  [0] 7♣ (分值=7)
            }

            // 循环读取，直到输入一个合法的索引
            while (true)                                                               // 不断重试
            {
                Console.Write("你的选择：");                                            // 输入前缀
                var input = Console.ReadLine();                                        // 读一行
                if (int.TryParse(input, out int idx) && idx >= 0 && idx < hand.Count) // 尝试转换 + 范围校验
                {
                    return idx;                                                        // 合法 → 返回
                }
                Console.WriteLine("输入无效，请重新输入。");                             // 非法 → 给出提示并重试
            }
        }

        // ======== 结算：公共区抽空后，双方摊牌 → 计算分值之和 → 判定胜负 ========
        private void EndGame()                                                         // 不返回值，仅打印最终信息
        {
            Console.WriteLine();                                                       // 空行分隔
            Console.WriteLine("===== 正式结算（公共区已抽空）=====");                    // 结算标题

            // 打印双方剩余手牌（如果某方已空手，会打印一个空串）
            Console.WriteLine($"人类剩余手牌：{string.Join(", ", human.Hand)}");        // 把牌对象用 ", " 连接
            Console.WriteLine($"电脑剩余手牌：{string.Join(", ", ai.Hand)}");           // 同上

            // 计算双方总分：把剩余手牌的 Value 相加（A=1, 2-10 按面值，J/Q/K=11/12/13，Joker=0）
            int humanScore = human.Hand.Sum(c => c.Value);                             // 人类总分
            int aiScore = ai.Hand.Sum(c => c.Value);                                   // 电脑总分

            // 打印总分
            Console.WriteLine($"人类总分：{humanScore}");                               // 示例：人类总分：23
            Console.WriteLine($"电脑总分：{aiScore}");                                  // 示例：电脑总分：19

            // 判定胜负（大者胜；相等为平局）
            if (humanScore > aiScore)                                                  // 人类分数更高
                Console.WriteLine("结果：人类胜！");                                     // 人类胜
            else if (aiScore > humanScore)                                             // 电脑分数更高
                Console.WriteLine("结果：电脑胜！");                                     // 电脑胜
            else                                                                       // 分数相等
                Console.WriteLine("结果：平局！");                                       // 平局
        }
    }
}
