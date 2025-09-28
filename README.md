# 终端版卡牌游戏（测试原型）

> 这是一个用于验证玩法流程的**命令行原型**（人机对战）。不包含图形界面，目标是让设计师能在本地快速编译并试玩规则原型。

## 参考文档

- [设计规格文档 (GameDesignSpec.md)](./GameDesignSpec.md)


## 目录结构

```
CardGameProject/
├─ CardGameProject.sln           解决方案文件
└─ CardGame.Cli/                 控制台程序（入口工程）
   ├─ CardGame.Cli.csproj
   ├─ Program.cs
   ├─ Card.cs
   ├─ Deck.cs
   ├─ Player.cs
   ├─ AIPlayer.cs
   ├─ GameEngine.cs
   ├─ CardEffects.cs
   └─ EffectRegistry.cs
```

## 环境准备

- **.NET SDK 8.0**（或兼容版本）  
  - Windows / macOS / Linux 通用。  
  - 验证安装：在终端/PowerShell 执行 `dotnet --info`，应能看到 SDK 信息。
- 终端/命令行工具（PowerShell、cmd、macOS Terminal 均可）。
- 可选：VS Code（方便查看/编辑代码）。

> **Windows 显示花色符号小贴士**：本程序会打印 `♠ ♥ ♦ ♣`。建议在 **Windows Terminal** 或 **PowerShell 7+** 下运行以确保 UTF-8 字符正常显示；若使用旧版 cmd，可先运行 `chcp 65001` 切换到 UTF-8。

## 一键编译与运行

### 方式 A：从解决方案根目录运行
```bash
cd CardGameProject
dotnet build
dotnet run --project CardGame.Cli
```

### 方式 B：直接在项目目录运行
```bash
cd CardGameProject/CardGame.Cli
dotnet build
dotnet run
```

首次运行会在控制台打印初始化与回合信息；**到你回合**时，程序会让你输入编号选择要出的牌。

## 交互与流程（当前实现）

- 对战双方：**人类**（你） vs **电脑（AI）**。
- 程序会提示类似：
  ```
  请选择要出的牌编号：
    [0] K♥ (分值=13)
    [1] 8♣ (分值=8)
    ...
  你的选择：
  ```
  输入数字（如 `1`）回车即可出对应的牌。
- 电脑 AI 的当前策略：**总是出手牌中的第一张**（用于稳定测试）。
- 若某一方在回合开始时**手牌为 0**，则：
  - 从**公共区**抽 1 张牌，本回合结束（不再出牌）。
- 对局以“人类→电脑”为一轮交替进行，**直到公共区被抽空**为止，然后立即结算。

## 牌与规则（当前实现对齐的部分）

### 牌库
- 使用标准 **52 + 2 Joker**：4 花色 × 13 点数 + 大/小王各 1 张。
- Joker 分值为 **0**；A=1，2–10 按面值，J=11，Q=12，K=13。

### 开局发牌
- **人类 6 张**、**电脑 7 张**（后手 +1 的占位规则，用于测试）、**公共区 14 张（暗牌）**。主牌库剩余 27 张暂不使用。

### 回合结构（测试版）
- 回合开始：打印“展示的一半手牌”（当前实现为**列表前一半**，仅打印，不改变牌状态）。
- 出牌：
  - 人类回合：**编号选牌**出牌；出牌后自动从**主牌库**补 1 张。  
  - 电脑回合：AI **固定出第一张**；出牌后自动从**主牌库**补 1 张。  
- 若回合开始手牌为 0：从公共区抽 1 张，回合结束（不出牌）。

### 出牌后补牌（新增规则）
- 玩家（人类或电脑）**每次打出 1 张牌后，都会立即从主牌库补充 1 张手牌**。  
- 若主牌库已空 → 仅提示“主牌库已空，无法补牌”，不再补。  
- 此规则保证手牌数量在主牌库未空前保持恒定（人类 6 张，电脑 7 张）。  
- “无牌可出 → 从公共区抽 1 张”的原规则保持不变。  

### 结束条件与结算（按设计规则）
- **公共区被抽空** → **立即结束**（不继续执行另一方的回合）。
- 结算：双方**摊开手牌**，按分值求和比较，高分者胜；相同为平局（仅手牌，Joker=0）。

## 目前已实现的功能

- 跨平台控制台程序（.NET 8，Windows/macOS/Linux）。  
- 标准牌库构建、洗牌与发牌。  
- 对战循环（人类↔电脑），**公共区抽空即结算**。  
- 人类回合：**编号选牌**出牌。  
- 电脑回合：AI **固定出第一张**（稳定、可预测，便于测试）。  
- 出牌后自动补牌：每次出牌后都会从主牌库补 1 张（主牌库为空时仅提示）。    
- 结算：打印双方**剩余手牌**与**总分**并**判定胜负**。
- 新增：**出牌提示效果（占位）**。当玩家或电脑**打出红心（♥）**或**点数为 7 的牌**时，终端会额外打印一行以 `[效果]` 开头的提示。
- 不改变：该提示**不影响任何规则或数值**（不会改变手牌数/得分/流程）；**原有“出牌后从主牌库补 1 张”的规则保持不变**。
- 目的：用于后续扩展“特定牌触发效果”的机制，目前仅用于可视化提示与验证钩子。
## 与最终设计的差距（已知限制）

> 这些是**刻意留白**，以便先跑通骨架，后续逐步补齐。

1. **展示机制**：当前仅“打印前一半”做提示，未做“玩家自主选择展示的一半/AI 的展示策略”，也未记录“展示/隐藏”状态。  
2. **回合结束重置**：未实现“洗剩余手牌并在下回合重新选择展示”。  
3. **效果/资源系统**：如冻结、翻牌、【+】/【×】等均未实现（`Player` 中的资源字段暂未使用）。  
4. **无法出牌的定义**：目前仅在**手牌为 0**时才从公共区抽牌；尚未实现“有手牌但**无合法可出**时也要抽”的判定。  
5. **AI 策略**：当前为最简策略（出第一张），后续将逐步替换为随机/启发式/基于效果的策略。

## 常见问题（FAQ）

- **Q：Windows 下花色符号乱码？**  
  A：建议使用 **Windows Terminal** 或 **PowerShell 7+**；若用 cmd，先执行 `chcp 65001` 切至 UTF-8，并使用支持 Unicode 的等宽字体（如 Consolas）。

- **Q：怎么只跑一小段做演示？**  
  A：启动后在你回合连续出牌，也可以在某些回合让自己手牌清空，从公共区抽牌以快进进度；公共区抽空即自动进入结算。

- **Q：如何修改 AI 行为？**  
  A：`AIPlayer.DecideMove()` 返回值是要出的**手牌索引**；当前固定返回 `0` 表示**出第一张**，可改为随机或其他策略。

## 开发者备注（代码入口）

- 入口：`Program.cs` → `GameEngine.Start()`。  
- 关键类：
  - `Card`：牌的点数/花色/分值与打印格式。  
  - `Deck`：牌堆（标准牌库、洗牌、抽牌、计数）。  
  - `Player`：玩家手牌与（占位）资源。  
  - `AIPlayer`：电脑决策（返回手牌索引）。  
  - `GameEngine`：初始化、回合流程、结束判定与结算。

## 后续路线（建议）

1. **真实展示机制**：人类选择展示的一半；AI 随机或简单策略；记录展示/隐藏。  
2. **回合结束重置**：未出的牌合并洗牌，下回合重新选择展示。  
3. **效果系统**：按设计接入“冻结/翻牌/资源”等效果与合法性判定。  
4. **AI 升级**：从“出第一张”→“随机/基于信息与效果的启发式”。  
5. **回放/日志**：把关键动作写入日志文件，方便复盘设计。

---

## 附录 A：示例对局输出（节选）

> 说明：以下是一次真实运行的“节选”，仅展示关键片段。实际输出会因洗牌不同而变化。

```text
$ dotnet run

===== 初始化完成 =====
人类手牌: 6（应为 6）
电脑手牌: 7（应为 7）
公共区(暗牌): 14（应为 14）
主牌库剩余: 27（应为 27）
...
===== 正式结算（公共区已抽空）=====
人类剩余手牌：(无牌)
电脑剩余手牌：4♠
人类总分：0
电脑总分：4
结果：电脑胜！
```

## 附录 B：Windows 终端显示设置（UTF-8 与花色符号）

为了让 `♠ ♥ ♦ ♣` 等符号正常显示，推荐 **Windows Terminal** 或 **PowerShell 7+**。如果使用传统 cmd，可以按下面做：

### 方案 1：Windows Terminal / PowerShell 7（推荐）
1. 安装 **PowerShell 7**（可从 Microsoft Store）。
2. 打开 **Windows Terminal**（或 PowerShell 7），默认就是 UTF-8。
3. 运行项目：
   ```powershell
   cd .\CardGameProject\CardGame.Cli
   dotnet run
   ```
4. 如果仍有乱码，打开 Windows Terminal 设置 → Profiles → Defaults → Appearance → Font face 选择 **Consolas** 或其他支持 Unicode 的等宽字体。

### 方案 2：传统 cmd（临时切 UTF-8）
1. 打开 **cmd**。
2. 切换代码页为 UTF-8：
   ```bat
   chcp 65001
   ```
3. 运行：
   ```bat
   cd CardGameProject\CardGame.Cli
   dotnet run
   ```
4. 若仍有方块/乱码，右键标题栏 → 属性 → 字体，换成 **Consolas**（或其它支持 Unicode 的字体）。

> 如果 PowerShell 5 里输出还有编码问题，可改用 PowerShell 7；或者执行：
> ```powershell
> $OutputEncoding = [Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()
> ```

## 附录 C：把 README 加入项目并打包分享

> 建议在**项目根目录**放置 `README.md` 与 `README.pdf`。结构应类似：
```
CardGameProject/
  CardGameProject.sln
  README.md
  README.pdf
  CardGame.Cli/
    CardGame.Cli.csproj
    Program.cs
    Card.cs
    Deck.cs
    Player.cs
    AIPlayer.cs
    GameEngine.cs
```

### macOS / Linux（终端）
```bash
# 1) 把 README.md / README.pdf 复制到项目根目录
cp README.md ~/Desktop/CardGameProject/README.md
cp README.pdf ~/Desktop/CardGameProject/README.pdf

# 2) 压缩整个项目文件夹
cd ~/Desktop
zip -r CardGameProject.zip CardGameProject
```

### Windows（PowerShell）
```powershell
# 1) 复制 README 到项目根目录
Copy-Item .\README.md   .\CardGameProject\README.md
Copy-Item .\README.pdf  .\CardGameProject\README.pdf

# 2) 压缩
Compress-Archive -Path .\CardGameProject -DestinationPath .\CardGameProject.zip -Force
```

## 附录 D：快速排查

- **`dotnet: command not found`**  
  未安装 .NET SDK。安装 .NET 8（或兼容版本），重开终端再试。

- **构建报错找不到命名空间**  
  确认所有 `.cs` 文件都在 `CardGame.Cli` 目录下，且顶部命名空间一致：
  ```csharp
  namespace CardGame.Cli
  {
      ...
  }
  ```

- **花色符号是方块/乱码**  
  用 Windows Terminal 或 PowerShell 7；切到 UTF-8（`chcp 65001`）；换成 Unicode 等宽字体（Consolas）。

- **运行后一直让我输入编号**  
  这是正常交互：每到你回合都会要求你输入 `[0] [1] ...` 中的一个数字来出牌。输入回车即可继续。
