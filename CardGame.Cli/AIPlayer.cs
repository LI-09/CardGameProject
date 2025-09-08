using System;                               // 引入 System 命名空间，提供 Random 类等

namespace CardGame.Cli                      // 命名空间：保持和其他类一致
{
    /// <summary>
    /// AI 玩家类，继承自 Player。
    /// 作用：在电脑回合时能自动选择要出的牌。
    /// </summary>
    public class AIPlayer : Player           // 定义 AIPlayer 类，继承 Player
    {
        private readonly Random _rng = new();// 随机数生成器，用于随机选择一张牌（readonly 表示构造后不可更改）

        // 构造函数：调用父类 Player 的构造函数，传入名字（通常就是 "电脑"）
        public AIPlayer(string name) : base(name) { }

        /// <summary>
        /// 决策方法：返回要出的牌在手牌中的索引。
        /// - 如果手牌为空 → 返回 -1。
        /// - 否则 → 出第一张牌
        /// </summary>
        public int DecideMove()//决定出哪张牌
        {
            if (Hand.Count == 0)             // 如果没有手牌
                return -1;                   // 返回 -1 表示无牌可出

            return 0;//每次都出第一张固定
        }
    }
}
