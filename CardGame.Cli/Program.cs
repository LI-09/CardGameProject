using System;

namespace CardGame.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // === 在启动游戏前，集中注册所有效果规则 ===
            EffectRegistry.RegisterAll();

            //（可选）最小自检：验证出牌效果钩子能工作
            // 说明：默认注释，不影响正式运行；需要时去掉下面 5 行的注释即可。
            // var testGame = new GameEngine();
            // var testPlayer = new Player("测试玩家");
            // var testCard = new Card("7", "♥", 7);   // 注意：三参构造 (rank, suit, value)
            // Console.WriteLine(">>> 自检：马上手动触发一次 OnPlay");
            // CardEffects.Run(CardTrigger.OnPlay, testCard, testPlayer, testGame);

            // 创建游戏引擎实例
            var engine = new GameEngine();

            // 启动游戏（包含初始化 + 人类回合 + 电脑回合）
            engine.Start();

            // TODO: 后续我们会改成 while(!engine.IsGameOver()) { ... }
            //       这样可以无限回合直到游戏结束
        }
    }
}
