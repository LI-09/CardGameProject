namespace CardGame.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 创建游戏引擎实例
            var engine = new GameEngine();

            // 启动游戏（包含初始化 + 人类回合 + 电脑回合）
            engine.Start();

            // TODO: 后续我们会改成 while(!engine.IsGameOver()) { ... }
            //       这样可以无限回合直到游戏结束
        }
    }
}
