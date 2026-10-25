# 5.2.0

- New `DisableTransferMoney` setting, named after BiggerBazaar's: with it `Off`, you keep your gold when you take the portal to the Bazaar, so you can spend it there. `InheritFromBiggerBazaar`, the default with BiggerBazaar installed, leaves it to BiggerBazaar's own `disableTransferMoney`. Works with ShareSuite's shared gold.
- No void fog in the Bazaar in Simulacrum.
- Works with ProperSave: loading a save keeps the Donation Altar's per-run reward limit.
- Works with Quality: the Command Pedestal's item can have a quality, with the chance set in Quality's config.
- The Wandering CHEF's meal now visibly flies to you when it goes straight into your inventory.
- Items bought straight into your inventory fly to you with the whoosh of an item being handed over.
- Triple Drone Shop: new `BuyAll` setting to buy all three drones instead of one.
- Triple Drone Shop: new `InstancedPurchases` setting, on by default, so each player can buy from it.
- Requires the new PerClientSync library, which sends each player their own state of the instanced shops.
- For mod authors: `BazaarAPI` lets other mods add their own sections to the Bazaar, which the director buys for with the same credits, add their own printers, cauldrons and Command Pedestal tiers to the Bazaar's, and give their survivors a Donation Altar reward of their own. See the README.

# 5.1.0

- Triple Drone Shop: new `CostType`, plus a cost and a weight for common and uncommon drones.
- Drone Combiner Station: moved to make room for the Void Suppressor of the SuppressorInBazaar mod.
- Command Pedestal: one `DirectorWeight` and a `<Tier>Weight` per tier, like printers, with defaults based on how rare each tier is. The old `<Tier>DirectorWeight` settings are no longer read, so check your config.
- Cleansing Pool: `DirectorCost` now defaults to 0, so it always spawns. Only the pool this mod spawns gives Lunar Coins, not ones other mods add.
- Equipment swaps with `BuyToInventory`:
  - Lunar Buds and Equipment Terminals: swapping back at a slot you bought is free. Fixes swapping at Equipment Terminals.
  - Command Pedestal: no longer needs a free equipment slot. Your old equipment goes into the vault; use the vault again to swap back.
  - Wandering CHEF: no longer needs a free equipment slot. Your old equipment is discarded.
- Donation Altar: reward equipment replaces yours, and the old one is discarded. The altar shows as used once you reach your donation limit.
- Price signs disappear for players who bought from the Command Pedestal or can't donate anymore.
- Newt no longer comments on equipment swaps.

# 5.0.0

Updated for the latest Risk of Rain 2 patch. Requires ItemStringParser 2.1.0. Many settings were renamed or removed, so check your config after updating.

Thanks to WeeabooGod for the Lunar Shop rework and many of the fixes in this version.

### Director

- Add a director that decides which interactables appear in the Bazaar, like the game's own scene director. It gets `DirectorExtraCredits`, plus `DirectorCreditsPerStage` for every stage cleared, at most `DirectorCreditsCap`, and spends them by each interactable's `DirectorCost` and `DirectorWeight`. It replaces `SpawnCountByStage` and `SpawnCountOffset`.
- Rename the `Amount` settings to `MaxAmount`. Old values are not carried over.

### New sections

- Add a Drones section for the Alloyed Collective: the director can spawn a Drone Combiner Station, a Drone Upgrade Station, a Drone Scrapper and a Triple Drone Shop, and the players' drones come along into the Bazaar. Unlike in the base game, the Drone Upgrade Station refuses players without drones instead of taking their item for nothing.
- Add a Command Pedestal section: buying from it gives a Command Essence that lets you choose any item of its tier. The director picks the tier, and each tier has its own price, director cost and weight. Unmodded clients can't spawn the game's own Command Chest, so it is the Alloyed Collective's Ocular Vault, with the essence it gives shown above it. With `InstancedPurchases`, each player can buy once. With `BuyToInventory`, the item picker opens right away, and the price is only charged once an item is picked.

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

- Unmodded clients can now see and use the Donation Altar. It is now the Deep Void Signal, without its beam, with its price shown in front of it.
- Each player sees rewards they could get on Lunar Buds on top of the altar, arranged by how many items the reward has.
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
- The Wandering CHEF only appears with the Alloyed Collective. With `CraftableRecipesOnly`, he only offers a recipe that someone can cook from their inventory and what the Bazaar offers, including one item from the Command Pedestal, and otherwise doesn't appear.
- At most 2 Scrappers spawn, instead of 4.
- Add Transcendent Cauldrons from Hallowed Concepts as a cauldron type, with weight 0 by default.
- With `BuyToInventory`, Equipment Terminals, the Command Pedestal and the Wandering CHEF only sell equipment into an empty equipment slot.
- Captain's hacking beacon only pays for one purchase of a Bazaar shop; afterwards it costs the usual price again.
- Remove the Bazaar decoration (`DecorateBazaar`) and the unused Green to Purple and Green to Yellow cauldron weights.
- The settings in each config section are now in a sensible order.

### Fixes

- Purchases and donations are only counted and charged after a successful payment, and donations only for a valid reward.
- Fix equipment rewards: they fill the free slots from Functional Couplers, and equipment that doesn't fit drops instead of disappearing.
- Fix the item selection of Void printers, and skip printer tiers without available items.
- Fix cauldrons without choices, and pay ShareSuite's cauldron refund only after a successful trade.
- With `BuyToInventory`, the Wandering CHEF checks that you have the ingredients before cooking.
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
