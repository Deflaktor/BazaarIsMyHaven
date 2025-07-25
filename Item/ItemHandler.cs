using RoR2;

namespace BazaarIsMyHome
{
    public class ItemHandler
    {
        public void GlobalEventManager_OnCrit(On.RoR2.GlobalEventManager.orig_OnCrit orig, GlobalEventManager self, CharacterBody body, DamageInfo damageInfo, CharacterMaster master, float procCoefficient, ProcChainMask procChainMask)
        {
            orig(self, body, damageInfo, master, procCoefficient, procChainMask);
            Inventory inventory = master.inventory;
            int itemCount = inventory.GetItemCount(JunkContent.Items.CooldownOnCrit);
            if (itemCount > 0)
            {
                //Util.PlaySound("Play_item_proc_crit_cooldown", body.gameObject);
                SkillLocator component = body.GetComponent<SkillLocator>();
                if (component)
                {
                    //ChatHelper.Send($"itemCount = {itemCount * 0.5}");
                    float dt = itemCount * -0.5f;
                    if (component.primary)
                    {
                        component.primary.RunRecharge(dt);
                    }
                    if (component.secondary)
                    {
                        component.secondary.RunRecharge(dt);
                    }
                    if (component.utility)
                    {
                        component.utility.RunRecharge(dt);
                    }
                    if (component.special)
                    {
                        component.special.RunRecharge(dt);
                    }
                }
            }
        }

        public void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            try
            {
                orig(self);
                if (!self && self.teamComponent.teamIndex == TeamIndex.Player)
                {
                    int count = self.inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger);
                    if (count > 0)
                    {
                        //self.maxHealth /= 10;
                        //self.maxHealth *= 10;
                        self.damage /= 0.04f;
                        //self.damage *= 1.1f;
                    }
                }
            }
            finally
            {

            }
        }
    }
}
