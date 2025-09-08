using System;                              // 提供 Console、异常
using System.Collections.Generic;          // 提供 List<T>
using System.Linq;                         // 提供 Select 等 LINQ 方法

namespace CardGame.Cli                     // 命名空间
{
    public class Player                    // 玩家类：保存手牌与资源
    {
        public string Name { get; }        // 玩家名字
        public List<Card> Hand { get; } = new();   // 手牌，初始化为空 List
        public int PlusResources { get; set; } = 3;   // 【+】资源，初始=3
        public int CrossResources { get; set; } = 1;  // 【x】资源，初始=1

        public Player(string name) => Name = name;   // 构造函数：传入名字

        public void Draw(Deck from, int count = 1)   // 从牌堆抽牌（默认抽 1 张）
        {
            for (int i = 0; i < count; i++)          // 循环 count 次
            {
                if (from.Count == 0)                 // 如果来源牌堆为空
                    throw new InvalidOperationException("来源牌堆为空，无法继续抽牌。");
                Hand.Add(from.Draw());               // 抽一张牌加入手牌
            }
        }

        public void ShowHandLine()                   // 打印玩家手牌
        {
            Console.WriteLine($"{Name} 手牌({Hand.Count}): {string.Join(", ", Hand.Select(c => c.ToString()))}");
            // 输出示例：“人类 手牌(6): A♠, 3♦, Joker1...”
        }
    }
}
