namespace CardGame.Cli;                // 定义命名空间 CardGame.Cli，保证和项目统一，避免类名冲突

public class Card                      // 定义一个公共类 Card，表示一张牌
{
    // 点数: "A","2",...,"K","Joker"
    public string Rank { get; }        // 牌面点数，例如 "A"、"10"、"Q"、"Joker1"、"Joker2"

    // 花色: "♠","♥","♦","♣"，Joker 没有花色
    public string Suit { get; }        // 花色（黑桃♠、红心♥、方块♦、梅花♣），大小王为空格 " "

    // 用于结算的数值: A=1, J=11, Q=12, K=13, Joker=0
    public int Value { get; }          // 结算点数，用来判断输赢（规则里定义）

    public Card(string rank, string suit, int value)   // 构造函数：创建一张牌
    {
        Rank = rank;                   // 赋值牌的点数
        Suit = suit;                   // 赋值牌的花色
        Value = value;                 // 赋值牌的分值
    }

    //显示花色
    public override string ToString()  // 重写 ToString()，用于打印牌面
    {
        // Joker 特殊显示
        if (Rank == "Joker1") {        // 如果是小王
            return "Joker1";           // 打印 "Joker1"
        } else if (Rank == "Joker2") { // 如果是大王
            return "Joker2";           // 打印 "Joker2"
        } else {
            return $"{Rank}{Suit}";    // 普通牌打印成 Rank+Suit，例如 "A♠"
        }
    }
}
