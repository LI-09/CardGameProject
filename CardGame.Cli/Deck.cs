using System;                          // 提供基本类型和异常
using System.Collections.Generic;      // 提供 List<T>
using System.Linq;                     // 提供 OrderBy 等 LINQ 方法（用于洗牌）

namespace CardGame.Cli;                 // 命名空间 CardGame.Cli

public class Deck                       // 定义公共类 Deck，表示一堆牌（牌库/公共区）
{
    private List<Card> cards = new();   // 内部存储的牌列表

    // 构造函数：用一组牌初始化
    public Deck(List<Card> initialCards) 
    {
        cards = initialCards;           // 将外部传入的牌放入牌堆
    }

    // 生成标准 52+2 的牌库
    public static Deck CreateStandardDeck()
    {
        List<Card> allCards = new();    // 新建一个空列表，用来放所有牌

        string[] suits = { "♠", "♥", "♦", "♣" };   // 定义花色数组
        string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" }; // 定义点数数组

        // 52 张普通牌
        foreach (var suit in suits)             // 遍历 4 种花色
        {
            for (int i = 0; i < ranks.Length; i++)  // 遍历 13 个点数
            {
                string rank = ranks[i];         // 当前点数
                int value = i + 1;              // 分值：A=1, ..., J=11, Q=12, K=13
                allCards.Add(new Card(rank, suit, value));  // 创建一张牌并加入列表
            }
        }

        // 2 张 Joker（大小王，规则一样，只是显示区分）
        allCards.Add(new Card("Joker1", " ", 0));  // 小王，分值=0
        allCards.Add(new Card("Joker2", " ", 0));  // 大王，分值=0

        return new Deck(allCards);           // 返回一个新的 Deck 对象
    }

    // 洗牌
    public void Shuffle()
    {
        var rng = new Random();              // 随机数生成器
        cards = cards.OrderBy(c => rng.Next()).ToList();  // 用随机排序重新生成列表，实现洗牌
    }

    // 抽一张牌
    public Card Draw()
    {
        if (cards.Count == 0)                // 如果牌堆为空
            throw new InvalidOperationException("牌堆已空！");  // 抛出异常

        Card top = cards[0];                 // 取出第一张牌
        cards.RemoveAt(0);                   // 从牌堆移除这张牌
        return top;                          // 返回这张牌
    }

    // 查看剩余张数
    public int Count => cards.Count;         // 属性 Count：返回牌堆里剩余的张数
}
