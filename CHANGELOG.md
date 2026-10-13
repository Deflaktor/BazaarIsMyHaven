# 5.0.0

Updated for the latest Risk of Rain 2 patch. Requires ItemStringParser 2.1.0. Many settings were renamed or removed, so check your config after updating.

Thanks to WeeabooGod for the Lunar Shop rework and many of the fixes in this version.

### Director

- Add a director that decides which interactables appear in the Bazaar, like the game's own scene director. It gets `DirectorExtraCredits`, plus `DirectorCreditsPerStage` for every stage cleared, at most `DirectorCreditsCap`, and spends them by each interactable's `DirectorCost` and `DirectorWeight`. It replaces `SpawnCountByStage` and `SpawnCountOffset`.
- Rename the `Amount` settings to `MaxAmount`. Old values are not carried over.

### New sections

- Add a Drones section for the Alloyed Collective: the director can spawn a Drone Combiner Station, a Drone Upgrade Station, a Drone Scrapper and a Triple Drone Shop, and the players' drones come along into the Bazaar.
- Add a Command Chest section: it drops a Command Essence that lets you choose any item of its `Tier`. It has its own price, instanced purchases, and a buy to inventory option that opens the item picker right away.

### Lunar Shop

- Use real Lunar Buds for any `MaxAmount` (up to 20) instead of shop terminals. `ReplaceLunarBudsWithTerminals` is removed.
- Instanced purchases now sync to each player, also for unmodded clients, who now also see the buds at the right size and opening.
- The Lunar Recycler rerolls the Lunar Buds, and is removed when no buds spawn.
- Newt comments on Lunar Bud purchases.
- Lunar Buds now cost 2 Lunar Coins by default, like in the base game.

### Prices

- Add a `CostType` setting to Equipment Terminals, Lunar Buds and the Donation Altar, e.g. Lunar Coins, money or items of a tier. The price tag shows it too.
- Add `ScaleMoneyCosts`: money prices grow with difficulty, like the game's own shops.
- Equipment Terminals now cost $50 by default instead of nothing, and show their price above them.
- The Shrine of Order keeps its price instead of doubling it after every use. Its `CostMultiplier` setting never had an effect and is removed.

### Donation Altar

- Each player sees rewards they could get on Lunar Buds above the altar.
- Characters whose own reward list gives nothing now get `RewardListCharacterDefault`.
- Add default reward lists for Acolyte and Enforcer.

### Newt

- Replace `Greeting` and `TrashTalk` with `Dialogue`: `Off`, `Default` (the game's own unused Newt lines, in each player's language) or `BazaarIsMyHome`.
- Add `KarmaTrigger` (`OnDamage` or `OnKill`) and `KarmaTriggerOncePerVisit`, for when angering Newt raises the run's karma.
- In `Tank` mode, Newt starts with a third less health.

### Drifter

- Add `PreventDrifterBagging`, `PreventDrifterJunk` and `PreventDrifterTinker`, so Drifter can't bag things, farm temporary items or tinker in the Bazaar.

### Other changes

- Config changes now apply from the next run, without restarting the game.
- The Wandering CHEF only appears with the Alloyed Collective. With `CraftableRecipesOnly`, he only offers a recipe that someone can cook, and otherwise doesn't appear.
- At most 2 Scrappers spawn, instead of 4.
- Remove the Bazaar decoration (`DecorateBazaar`) and the unused Green to Purple and Green to Yellow cauldron weights.
- The settings in each config section are now in a sensible order.

### Fixes

- Purchases and donations are only counted and charged after a successful payment, and donations only for a valid reward.
- Fix equipment rewards: they fill the free slots from Functional Couplers, and equipment that doesn't fit drops instead of disappearing.
- Fix the item selection of Void printers, and skip printer tiers without available items.
- Fix cauldrons without choices, and pay ShareSuite's cauldron refund only after a successful trade.
- Newt settings no longer affect other characters, such as SPEX in the Computational Exchange.
- Stop QoLChests from hiding Lunar Buds, and keep the Lunar Recycler's cooldown.
- The hovering Lunar Coin no longer appears without the Cleansing Pool.
- Clear leftover state between runs and visits, and keep setting up the Bazaar if one section fails.

# 4.2.0

- Add LunarShop AmountDependingOnCharacter
- Add the two new surivors to the default DonateRewardListCharacters

# 4.1.1

- Fix various issues with the Wandering Chef
- Fix equipments not working when buy to inventory is set

# 4.1.0

- Add Wandering Chef to the bazaar
- Fix Cleansing Pool not giving Lunar Coins as intended
- Fix equipment duplication when having Functional Coupler

# 4.0.0

- Update for Alloyed Collective
- Add proper R2API dependencies
- Use ItemStringParser for all item lists
- Fix donation of unused items
- Limit amount of donations per visit to the Bazaar and per run
- Fix equipments in bazaar not getting replaced with the elite list

# 3.0.0

- Donation Shrine no longer requires to donate 10 times to get reward

# 2.1.2

- Add item colors in donate chat message
- Better placement of lunar shop terminals

# 2.1.1

- Fix sequential items in lunar shop starting with wrong index

# 2.1.0

- Add feature for custom donation reward lists depending on the character
- Add feature for sequential rewards at donation altar

# 2.0.1

- Fix readme

# 2.0.0

- Rename RewardNormalList->RewardList1
- Rename RewardEliteList->RewardList2
- Rename RewardPeculiarList->RewardList3
- Add feature to replace equipment with elite aspects
- Fix link in readme to the original mod for real

# 1.0.1

- Fix link in readme to the original mod
- Fix language file

# 1.0.0

- Initial Release
