# 卡牌对战规则规格文档 Ver 1.0

> 本文档为 **设计规格文档**，完全对应设计者提供的规则说明。  
> 用途：开发团队可以逐条对照，检查 **哪些功能已实现，哪些功能待完成**。  
> 注意：内容来自原始设计规则，不做删减与简化。

---

## 一、基础设定

- 本游戏基于一副 **非标准的 52+2 张扑克牌**。  
  （不一定是 13×4+2，由每个玩家带的半副牌决定）

---

## 二、对局开始

1. **牌库组成**  
   - 每个人展示自己的半副牌（26+1），拼成一副公共牌（52+2）作为牌库。  

2. **起始发牌**  
   - 每个人从公共牌库抽 6 张牌。  
   - 公共区域放入 14 张牌。  
   - 抛硬币决定先后手。  
   - **后手额外再抽 1 张牌**。  

3. **换牌**  
   - 每个人可以选择把一部分手牌洗回去重新抽。  

4. **资源**  
   - 每个人获得 **3 个【+】** 和 **1 个【×】**，作为可消耗资源。  

---

## 三、胜负判定

- 公共区域牌被抽完时，游戏结束。  
- 双方揭示所有手牌 → 按总点数比较，决定胜负。  

---

## 四、回合流程

1. **展示手牌**  
   - 必须把总手牌数量减半后放在桌上，其余叠盖，用来迷惑对手。  
   - 展示数量 = 手牌的一半，向下取整。  
     - 举例：6 张手 → 7 张手时，需展示 3 张。  
   - 盖住的部分随机叠放，直到下一个步骤打牌时逐张揭牌。  
   - 每回合会自动重置，重新选择要展示或盖住的牌。  

2. **打牌**  
   - 每回合默认 1 个行动点。  
   - 每个行动点允许先弃 1 张牌，再抽牌、触发：  
     - 常规效果  
     - 被动技能效果  
     - 主动技能效果  
   - 然后回合结束。  

---

## 五、技能系统

### 1. 被动技能
- 被动技能会让玩家的操作带来更多效果。  
- 每个人可以获得 **3–5 个被动技能**。  
- 示例：在摸牌阶段额外摸牌、遇到不同手牌带来的不同效果。  

### 2. 主动技能（Hero Power）
- 每回合可以用一次【×】，每名玩家回合可以用一次。  
- 示例：你每次打出的黑色牌会影响公共区消耗效果，每局可以使用 2 次。  

---

## 六、局内资源

1. **【+】号**  
   - 每局初始 3 个。  
   - 需要两个和和时可以用 2 以内，临时无法补充。  

2. **【×】号**  
   - 每局初始 1 个。  
   - 每回合可用 1 次，无法补充。  

3. **用途**  

   - **数学用途（Math Mode）**  
     - 通过两张牌等于另一个数字 → 额外弃牌 / 打出效果。  
     - 例如：4+4=8 → 弃 4 和 4，用【+】弃掉打出一张 8。  

   - **强化用途（Enhance Mode）**  
     - 增强目标牌效果。  
     - 示例：指定某张目标牌，如果用【+】增强，目标牌效果会变强。  

---

## 七、牌库机制

- **打出堆 (Played stack)**：每当一张牌被打出，进入此堆。  
- **摧毁堆 (Destroyed stack)**：每当一张牌被摧毁，进入此堆。  
  - 摧毁和弃牌堆不等价 → 一般规则不会混用。  
  - 智能分开，是因为游戏里的牌平均价值可能更高。  
- **牌库机制**  
  - 开局牌库 27 张牌（双方抽 6+6+14+1 张牌）。  
  - 剩下的牌组成公共牌库，用于补牌。  

---

## 八、卡牌效果表（Card Effect Table）

（以下逐张列出，完全照原文）

### A, 2–10, J, Q, K, Joker

| Rank | Card Effect | 升级后效果 |
|------|-------------|------------|
| A | Cannot be played alone | Any two of A/2/3 and a【+】destroys a folded card in opponent’s hand. Then, draw a card. |
| 2 | When this card is played: replenish a【+】mark to your item bar. Then, draw a card. | 同左 |
| 3 | Cannot be played alone. | Any two of A/2/3 and a【+】destroys a folded card in opponent’s hand. Then, draw a card. |
| 4 | Cannot be played, destroyed or stolen. | 同左 |
| 5 | **Only when played alone**: points = #f, where #f = #cards left in the public pool. | 同左 |
| 6 | Reveal 2 of opponent’s folded cards for 1 round. Draw a card. | **需消耗【+】**才能打出，效果 = 3 张翻面而不是 2 张 |
| 7 | Draw 2 cards. If their sum > 16, consume a【+】. If不能消耗，跳过。 | 同左 |
| 8 | Swap 1 card in your hand with 1 folded card in opponent’s hand. Draw a card. | **需消耗【+】**才能打出，效果 = 同时交换 3 张。 |
| 9 | Draw 3 cards from public pool, choose 1 from中加入手牌。 | 同左 |
| 10 | Steal a card from opponent’s folded cards. 如果点数 < 10，可消耗【+】偷取，否则无效。Draw a card. | 同左 |
| J (11) | 当被打出或摧毁：抽 2 张。 | **需消耗【×】**才能打出：Steal 1 card from opponent’s hand. Draw a card. |
| Q (12) | Freeze 2 of opponent’s folded cards. Draw a card. | **需消耗【×】**才能打出：Destroy 2 cards from enemy hand. Not followed by drawing a card. |
| K (13) | Disable all opponent passives and Hero Power for 1 turn. Draw a card. | **需消耗【×】**才能打出：Gain 2 extra turns if no Joker in your hand. |
| Joker | Treated as any value when discarded. Points = 0. | 同左 |

---

## 九、术语表

- **AP (行动点)**：每回合默认 1，每次出牌消耗。  
- **Turn (回合)**：展示 → 打牌 → 回合结束。  
- **Round (轮次)**：双方各进行一个 Turn。  
- **Show (展示牌)**：每回合打牌前，必须选择 [Hand Size ÷ 2 – 1] 张手牌摊开。  
- **Folded (盖牌)**：未展示的手牌，随机盖放，可能受效果影响。  
- **Played Stack**：所有打出牌的归宿。  
- **Destroyed Stack**：所有被摧毁的牌的归宿。  

---

## 十、效果列表（简明索引）

- **Reveal（翻开）**：翻开对手盖牌。  
- **Swap（交换）**：和对手交换手牌。  
- **Steal（偷取）**：拿走对手手牌。  
- **Freeze（冻结）**：指定对手的牌被冻结。  
- **Destroy（摧毁）**：指定的牌进入摧毁堆。  
- **Disable（封锁）**：禁止对手技能/英雄能力。  
