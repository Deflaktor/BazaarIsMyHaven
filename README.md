# BazaarIsMyHaven

This mod is a rewrite of the excellent [BazaarIsMyHaven](https://thunderstore.io/package/Lunzir2/BazaarIsMyHaven/) mod by Lunzir. This is a server-side mod, as such only the host needs it.

## Capabilities

- Have anywhere between 1 and 

## Config Settings
| Setting| Default Value| 
|:-------|:-------:|
[00 Setting设置]|
EnableMod|true
EnableAutoOpenShop|true
EnableNoKickFromShop|true
EnableShoperWontDie|false
NewtSecondLifeMode |Evil
EnableWelcomeWord|true
PenaltyCoefficient|10
SpawnCountByStage|true
SpawnCountOffset|0
[01 Printer打印机]|
PrinterCount|9
PrinterTier1Weight|0.8
PrinterTier2Weight|0.2
PrinterTier3Weight|0.01
PrinterBossWeight|0.01
PrinterTierLunarHackChance|0.05
PrinterTierVoidHackChance|0.05
[02 Cauldron大锅]|
CauldronCount|7
CauldronWhiteWeight|0.3
CauldronGreenWeight|0.6
CauldronRedWeight|0.1
[02.1 Cauldron Hack大锅数据修改]|
EnableCauldronHacking|true
CauldronWhiteToGreenCost|3
CauldronGreenToRedCost|5
CauldronRedToWhiteCost|1
CauldronWhiteCostTypeChange|true
CauldronWhiteHackedChance|0.2
CauldronGreenHackedChance|0.2
CauldronRedHackedChance|0.2
CauldronYellowWeight|0.33
CauldronBlueWeight|0.33
CauldronPurpleWeight|0.33
CauldronWhiteToGreenCost_Hacked|3
CauldronGreenToRedCost_Hacked|5
CauldronRedToWhiteCost_Hacked|4
[03 Scrapper收割机]|
ScrapperCount|4
[04 Equipment主动装备]|
EquipmentCount|6
[05 Lunar月球装备]|
LunarShopTerminalCount|11
EnableLunarShopTerminalInjection|true
LunarShopTerminalCost|2
[06 ShrineRestack跌序]|
EnableShrineRestack|true
ShrineRestackMaxCount|99
ShrineRestackCost|1
ShrineRestackScalar|2
[07 CleanPool净化池]|
EnableShrineCleanse|true
[08 Pray祈祷]|
EnablePray|true
RewardCount|3
PrayCost|10
PrayNormalWeight|0.5
PrayEliteWeight|0.25
PrayPeculiarWeight|0.25
PrayPeculiarList|
[09 LunarRecycler切片]|
EnableLunarRecyclerInjection|true
LunarRecyclerAvailable|true
LunarRecyclerRerollCount|10
LunarRecyclerRerollCost|1
LunarRecyclerRerollScalar|2
[10 预言地图]|
EnableSeerStationsInjection|true
SeerStationAvailable|true
LunarSeerStationsCost|3
[98 Newts lines商人台词]|
EnableLines|true
~~Lines~~|~~纽特：请不要打我。Newt: Please don\'t hit me.~~
[99 Decorate装饰]|
EnableDecorate|true

### Items Pool of Pray 祈祷物品池信息
|en|cn|describe|
|---|---|---|
[Normal Items 一般物品]||White(Tier1), Green(Tier2), Red(Tier3) 白、绿、红颜色物品
[Elite Equipments 精英主动装备]||
His Reassurance|大地之证|
Ifrit's Distinction|伊夫利特的卓越|
Spectral Circlet|幽灵头饰|
Her Biting Embrace|她的噬咬拥抱|
Silence Between Two Strikes|暴风雨前的宁静|
N'kuhana's Retort|恩库哈纳的反驳|
EQUIPMENT_AFFIXVOID_NAME|EQUIPMENT_AFFIXVOID_NAME|Void Elite equip 虚空精英装备
Shared Design|共同的设计|
Elegy of Extinction|灭绝挽歌|
[Peculiar Items 奇特物品]||items from the game that have not been developed have no name or description. 来源于游戏没有开发完的内部物品，他们没有名字和描述
ITEM_BOOSTATTACKSPEED_NAME|ITEM_BOOSTATTACKSPEED_NAME|+10% attack speed per stack. +10%攻击速度/物品数量
ITEM_BOOSTDAMAGE_NAME|ITEM_BOOSTDAMAGE_NAME|+10% dmg per stack. +10%基础伤害/物品数量
ITEM_BOOSTEQUIPMENTRECHARGE_NAME|ITEM_BOOSTEQUIPMENTRECHARGE_NAME|-10% equip CD per stack, benefits of diminishing. -10%主动装备冷却时间，效益递减
ITEM_BOOSTHP_NAME|ITEM_BOOSTHP_NAME|+10% HP per stack. +10%生命值/物品数量
ITEM_BURNNEARBY_NAME|ITEM_BURNNEARBY_NAME|permanent burning. 永久燃烧
Wicked Ring|邪恶的戒指|+5% crit, when crit -0.5s all skill CD per stack. +5%暴击率，暴击时候-0.5秒技能CD
ITEM_CRIPPLEWARDONLEVEL_NAME|ITEM_CRIPPLEWARDONLEVEL_NAME|when a monster levels up, a permanent slow down aura is generated for the player that owns the item. 当怪物升级，向拥有该物品的玩家生成永久减速光环
ITEM_EMPOWERALWAYS_NAME|ITEM_EMPOWERALWAYS_NAME|just an ornament. 装饰品
ITEM_GHOST_NAME|ITEM_GHOST_NAME|become a ghost and can't be attacked. 变成幽魂，无法被攻击
ITEM_LEVELBONUS_NAME|ITEM_LEVELBONUS_NAME|+1 team level per stack. +1等级/物品数量
Ancestral Incubator|先祖孵化器|
ITEM_INVADINGDOPPELGANGER_NAME|ITEM_INVADINGDOPPELGANGER_NAME|+1000% hp, -96% dmg(dmg has been fixed to 1). +1000%生命值，-96%基础伤害(伤害已修正为1)
ITEM_TEMPESTONKILL_NAME|ITEM_TEMPESTONKILL_NAME|When killed, there is a chance to summon the circle of Storms. touching the circle of Storms. gains N 'Kuhana's Retort Buff for 10 seconds. 当击杀后有一定机率召唤风暴圈，触碰风暴圈获得N'kuhana's Retort Buff，持续10秒
ITEM_WARCRYONCOMBAT_NAME|ITEM_WARCRYONCOMBAT_NAME|Trigger effect: +50% movement speed, +100% attack speed for 2 seconds (+4 seconds per stack), 30 seconds cooldown. 当发动攻击触发效果，+50%移动速度，+100%攻击速度，持续2秒(+4秒/物品数量)，冷却30秒

## What's Next 以后的想法
- [ ] Increase the probability of something good depending on the difficulty of the level. 根据关卡难度提升好东西的概率
- [ ] Recycle can reroll custom LunarShopBud. 切片翻滚包含所有月球蓓蕾，目前只有原本的5个在滚，有点难不一定能做到
- [ ] Developing new mods, and three other ideas that have something to do with Newt. 开发新的mod，还有三个想法都和商人有点关系。


## Known issues 已知问题
- Although no need for unmoded, for players who have not installed the same mod, some labels are not correctly displayed and will not affect normal use. For example, moon bud physical size, shrinehealing price tag is displayed as money, the actual cost of lunar coins. 虽然不需要unmoded，但是未安装同样mod的玩家，一些标签不能准确显示，但不影响正常使用。比如月球蓓蕾实体大小，树灵价格标签显示为金钱，实际是花费月币。
- Equipment price is currently set to 0, there is no better way to change the price type so far. 主动装备价格目前设置成0，目前没有找到办法修改价格类型。
