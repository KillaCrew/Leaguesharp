#region
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;

#endregion

namespace Kindred
{
    internal class Program
    {
        public const string ChampionName = "Kindred";

        public static List<Spell> SpellList = new List<Spell>();
        public static List<Obj_AI_Hero> Enemies = new List<Obj_AI_Hero>(), Allies = new List<Obj_AI_Hero>();

        public static SpellSlot Ignite;
        public static SpellSlot HealSlot;
        public static SpellSlot smiteSlot = SpellSlot.Unknown;

        public static Items.Item ManaPot = new Items.Item(2004, 0);
        public static Items.Item Botrk = new Items.Item(3153, 550f);
        public static Items.Item HealthPot = new Items.Item(2003, 0);
        public static Items.Item Youmuus = new Items.Item(3142, 650f);
        public static Items.Item Cutlass = new Items.Item(3144, 550f);

        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell SmiteSlot;

        public static List<SpellSlot> AUTO_LEVEL_SEQUENCE = new List<SpellSlot>() { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.Q, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.W, SpellSlot.Q, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.W, SpellSlot.E, SpellSlot.E, SpellSlot.R, SpellSlot.E, SpellSlot.E };

        public static Menu Config;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {

            if (Player.ChampionName != ChampionName)
                return;

            Notifications.AddNotification("KK - Killa Kindred Loaded", 7000);

            Q = new Spell(SpellSlot.Q, 340);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 800);

            //Ignite = Player.GetSpellSlot("summonerdot"); SOON

            Menu = new Menu("KK - Killa Kindred", Player.ChampionName, true);

            Menu OrbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(OrbwalkerMenu);

            Menu TargetSelectorMenu = Menu.AddSubMenu(new Menu("Target Selector", "TargetSelector"));
            TargetSelector.AddToMenu(TargetSelectorMenu);

            Menu ComboMenu = Menu.AddSubMenu(new Menu("Combo", "Combo"));
            ComboMenu.AddItem(new MenuItem("ComboUseQ", "Use Q").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboUseQAA", "Only Q for AA Reset").SetValue(false));
            ComboMenu.AddItem(new MenuItem("123", "---------------------------------------"));
            ComboMenu.AddItem(new MenuItem("ComboUseR", "Use Smart R").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboUseROnlyOnMe", "Ego Ult :]").SetValue(false));
            ComboMenu.AddItem(new MenuItem("SaveMana", "Save Mana for R").SetValue(true));
            ComboMenu.AddItem(new MenuItem("UltHPSlider", "Min % HP for R").SetValue(new Slider(10, 1, 100)));
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe))
                Menu.SubMenu("Use R On")
                    .AddItem(new MenuItem("allyruse" + hero.BaseSkinName, hero.BaseSkinName).SetValue(true));

            Menu HarassMenu = Menu.AddSubMenu(new Menu("Harass", "Harass"));
            HarassMenu.AddItem(new MenuItem("HarassUseQ", "Use Q").SetValue(true));
            HarassMenu.AddItem(new MenuItem("HarassUseE", "Use E").SetValue(true));
            HarassMenu.AddItem(new MenuItem("HarassManaManager", "Mana Manager (%)").SetValue(new Slider(70, 1, 100)));

            Menu JungleLaneClear = Menu.AddSubMenu(new Menu("Jungle/Lane Clear", "JungleLaneClear"));
            JungleLaneClear.AddItem(new MenuItem("JungleClearUseQ", "Use Jungleclear Q").SetValue(true));
            JungleLaneClear.AddItem(new MenuItem("JungleClearUseW", "Use Jungleclear W").SetValue(true));
            JungleLaneClear.AddItem(new MenuItem("JungleClearUseE", "Use Jungleclear E").SetValue(true));
            JungleLaneClear.AddItem(new MenuItem("JungleClearManager", "Jungleclear Mana Manager (%)").SetValue(new Slider(10, 1, 100)));
            JungleLaneClear.AddItem(new MenuItem("123", "---------------------------------------"));
            JungleLaneClear.AddItem(new MenuItem("LaneClearUseQ", "Use Laneclear Q").SetValue(true));
            JungleLaneClear.AddItem(new MenuItem("LaneClearManaManager", "Laneclear Mana Manager (%)").SetValue(new Slider(60, 1, 100)));

            Menu Activator = Menu.AddSubMenu(new Menu("Activator & Items", "Activator"));
            Activator.AddItem(new MenuItem("smitecombotype", "Smite Mode").SetValue(new StringList(new[] { "Combo", "Killsteal" })));
            Activator.AddItem(new MenuItem("botrkcombotype", "Botrk Mode").SetValue(new StringList(new[] { "Combo", "Killsteal" })));
            Activator.AddItem(new MenuItem("UseYomuus", "Use Yomuus in Combo").SetValue(true));
            //Activator.AddItem(new MenuItem("UseHeal", "Use Heal").SetValue(true));
            //Activator.AddItem(new MenuItem("UseBarrier", "Use Barrier").SetValue(true));
            Activator.AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));
            Activator.AddItem(new MenuItem("123", "More Summoners/Items Soon"));

            Menu FleeMenu = Menu.AddSubMenu(new Menu("Flee", "Flee"));
            FleeMenu.AddItem(new MenuItem("FleeUseQ", "Use Q").SetValue(true));
            FleeMenu.AddItem(new MenuItem("FleeUseE", "Use E").SetValue(true));
            FleeMenu.AddItem(new MenuItem("FleeKey", "Flee Key").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Press)));

            Menu MiscMenu = Menu.AddSubMenu(new Menu("Misc", "Misc"));
            MiscMenu.AddItem(new MenuItem("AutoPots", "Smart Auto Pots").SetValue(true));
            //MiscMenu.AddItem(new MenuItem("KS", "KS Mode").SetValue(false));
            //MiscMenu.AddItem(new MenuItem("DiveMode", "R Dive Logic").SetValue(true));
            MiscMenu.AddItem(new MenuItem("AntiGapCloserQ", "Anti Gapcloser Q").SetValue(true));
            MiscMenu.AddItem(new MenuItem("AntiGapCloserE", "Anti Gapcloser E").SetValue(true));
            MiscMenu.AddItem(new MenuItem("AutoBuyBlueTrinket", "Auto Buy Blue Trinket").SetValue(true));
            MiscMenu.AddItem(new MenuItem("bluelevel", "Buy Blue Trinket at Level:").SetValue(new Slider(6, 0, 18)));
            //MiscMenu.AddItem(new MenuItem("TrollMode", "Troll Mode (DONT ACTIVATE)").SetValue(false));
            //MiscMenu.AddItem(new MenuItem("AutoLevel", "Auto Level SOON").SetValue(true));

            Menu DrawingMenu = Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            DrawingMenu.AddItem(new MenuItem("DrawQ", "Draw Q Range").SetValue(true));
            DrawingMenu.AddItem(new MenuItem("DrawW", "Draw W Range").SetValue(true));
            DrawingMenu.AddItem(new MenuItem("DrawE", "Draw E Range").SetValue(true));
            DrawingMenu.AddItem(new MenuItem("DrawR", "Draw R Range").SetValue(true));
            DrawingMenu.AddItem(new MenuItem("DrawSmite", "Draw Smite Range").SetValue(false));
            DrawingMenu.AddItem(new MenuItem("DrawBotrk", "Draw Botrk Range").SetValue(true));
            DrawingMenu.AddItem(new MenuItem("DrawIgnite", "Draw Ignite Range").SetValue(false));

            Menu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Game_ProcessSpell;

        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var Target = (Obj_AI_Hero)gapcloser.Sender;

            if (Menu.Item("AntiGapCloserQ").GetValue<bool>() && Q.IsReady() && ObjectManager.Player.Mana > R.ManaCost + Q.ManaCost && ObjectManager.Player.Position.Extend(Game.CursorPos, Q.Range).CountEnemiesInRange(400) < 3)
            {
                if (Target.IsValidTarget(Q.Range))
                {
                    Q.Cast(Player.Position.Extend(Game.CursorPos, Q.Range), true);
                }

                if (Menu.Item("AntiGapcloserE").GetValue<bool>())
                {
                    if (Target.IsValidTarget(E.Range) && E.IsReady() && Player.Mana > R.ManaCost + E.ManaCost)
                    {
                        E.Cast(Target);
                    }
                }
            }

            return;
        }

        public static void Game_ProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy && sender is Obj_AI_Hero && args.Target.IsMe)
            {
                if (((Obj_AI_Hero)sender).ChampionName.ToLower() == "vayne" && args.Slot == SpellSlot.E)
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(sender);
                    }
                }
            }
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                if (Menu.Item("ComboUseQAA").GetValue<bool>() && unit.IsMe)
                    if (Player.TotalAttackDamage > target.Health) return;
            if (!Player.IsWindingUp && Q.IsReady())
                Q.Cast(Game.CursorPos);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Botrk
            if (Menu.Item("DrawBotrk").GetValue<bool>())
                if (Botrk.IsOwned() && Botrk.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Botrk.Range, Color.Cyan);

            //Ignite
            if (Menu.Item("DrawIgnite").GetValue<bool>())
                if (Ignite.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Botrk.Range, Color.Cyan);

            //Smite
            if (Menu.Item("DrawSmite").GetValue<bool>())
                if (smiteSlot.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 570, Color.Cyan);

            //Q
            if (Menu.Item("DrawQ").GetValue<bool>())
                if (Q.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Teal);

            //W
            if (Menu.Item("DrawW").GetValue<bool>())
                if (W.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.Teal);

            //E
            if (Menu.Item("DrawE").GetValue<bool>())
                if (E.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Teal);

            //R
            if (Menu.Item("DrawR").GetValue<bool>())
                if (R.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.LightBlue);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:

                    Combo();
                    UseItems();

                    if (Menu.Item("ComboUseR").GetValue<bool>() && Menu.Item("ComboUseROnlyOnMe").GetValue<bool>()) //Snacked from a Kayle Assembly. Dont know which tho :/
                    {
                        if (!Player.HasBuff("Recall") && !Player.InFountain() && R.IsReady())
                        {
                            if (Player.HealthPercent <= Menu.Item("UltHPSlider").GetValue<Slider>().Value && Player.CountEnemiesInRange(1000) > 0)
                            {
                                R.Cast(Player);
                            }
                        }
                    }

                    else if (Menu.Item("ComboUseR").GetValue<bool>() && !Menu.Item("ComboUseROnlyOnMe").GetValue<bool>())
                    {
                        foreach (var allyR in ObjectManager.Get<Obj_AI_Hero>().Where(allyR => allyR.IsAlly && !allyR.IsMe))
                        {
                            if (!allyR.InFountain() && !allyR.HasBuff("Recall") && R.IsReady())
                            {
                                if (allyR.HealthPercent < Menu.Item("UltHPSlider").GetValue<Slider>().Value && allyR.CountEnemiesInRange(1000) > 0 && Vector3.Distance(Player.ServerPosition, allyR.Position) < R.Range)
                                {
                                    if (Menu.Item("allyruse" + allyR.BaseSkinName) != null && Menu.Item("allyruse" + allyR.BaseSkinName).GetValue<bool>() == true)
                                    {
                                        R.Cast(allyR);
                                    }
                                }
                            }
                        }
                    }

                    break;

                case Orbwalking.OrbwalkingMode.Mixed:

                    if (Player.ManaPercent > Menu.Item("HarassManaManager").GetValue<Slider>().Value)
                    {
                        Harass();
                    }
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    {
                        LaneClear();
                        JungleClear();
                    }
                    break;

                case Orbwalking.OrbwalkingMode.None:

                    break;
            }

            if (Player.Level >= Menu.Item("bluelevel").GetValue<Slider>().Value && Menu.Item("AutoBuyBlueTrinket").GetValue<bool>())
            {
                Player.BuyItem(ItemId.Scrying_Orb_Trinket);
            }

            KS();
            AutoPots();
            SmiteCombo();
            setSmiteSlot();
            Flee();
            UseIgnite();
            //DiveLogic();
            //TrollMode();      Everything SOON
            //AutoLvL();
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(800, TargetSelector.DamageType.Physical);

            if (W.IsReady() && W.IsInRange(target))
            {
                if (Menu.Item("SaveMana").GetValue<bool>() && Player.Mana > W.ManaCost + R.ManaCost)
                {
                    W.Cast();
                }

                else
                {
                    W.Cast();
                }
            }

            QLogic();

            if (E.IsReady() && E.IsInRange(target) && !target.HasBuffOfType(BuffType.SpellImmunity) || !target.IsZombie || !target.HasBuffOfType(BuffType.SpellShield))
            {
                if (Menu.Item("SaveMana").GetValue<bool>() && Player.Mana > E.ManaCost + R.ManaCost)
                {
                    E.Cast(target);
                }

                else
                {
                    E.Cast(target);
                }
            }
        }

        private static void Harass()
        {
            var QEndpos = Player.ServerPosition.Extend(Game.CursorPos, 300);
            var target = TargetSelector.GetTarget(800, TargetSelector.DamageType.Physical);

            if (Menu.Item("HarassUseQ").GetValue<bool>())
            {
                if (Q.IsReady() && !QEndpos.UnderTurret(true) || QEndpos.CountEnemiesInRange(500) > 2)
                {
                    Q.Cast(Game.CursorPos);
                }
            }
        }

        private static void JungleClear()
        {
            var junglemob = MinionManager.GetMinions(Player.Position, 800, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health).FirstOrDefault();
            var junglemob2 = MinionManager.GetMinions(Player.Position, 800, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health).FirstOrDefault();

            if (Player.ManaPercent > Menu.Item("JungleClearManager").GetValue<Slider>().Value)
            {
                if (Menu.Item("JungleClearUseQ").GetValue<bool>())
                {
                    if (Q.IsReady() && junglemob != null)
                    {
                        Q.Cast(Game.CursorPos);
                    }
                }

                if (Menu.Item("JungleClearUseW").GetValue<bool>())
                {
                    if (W.IsReady())
                    {
                        W.Cast();
                    }
                }

                if (Menu.Item("JungleClearUseE").GetValue<bool>())
                {
                    if (E.IsReady() && junglemob2 != null)
                    {
                        E.Cast(junglemob2);
                    }
                }
            }
        }

        private static void LaneClear()
        {
            var MinionLane =
                      MinionManager.GetMinions(700, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health)
                          .FirstOrDefault();
            var QLane = Menu.Item("LaneClearUseQ").GetValue<bool>();

            if (Player.ManaPercent > Menu.Item("LaneClearManaManager").GetValue<Slider>().Value)
            {
                if (QLane)
                {
                    if (MinionLane != null)
                    {
                        Program.Q.Cast(Game.CursorPos, false);
                    }
                }
            }
        }

        private static void QLogic() //SharpShooter <3
        {
            var QEndpos = Player.ServerPosition.Extend(Game.CursorPos, 300);

            if (Menu.Item("ComboUseQ").GetValue<bool>() && !Menu.Item("ComboUseQAA").GetValue<bool>())
            {
                foreach (var enemy in QEndpos.GetEnemiesInRange(300))
                    if (enemy.IsMelee())
                        return;

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    if (QEndpos.UnderTurret(true) || QEndpos.CountEnemiesInRange(800) > 0 && QEndpos.CountAlliesInRange(800) < 1)
                        return;

                Q.Cast(Game.CursorPos);
            }
        }

        private static void Flee()
        {
            if (Menu.Item("FleeKey").GetValue<KeyBind>().Active)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                if (Menu.Item("FleeUseQ").GetValue<bool>() && Q.IsReady())
                {
                    Q.Cast(Game.CursorPos);
                }
            }

            if (Menu.Item("FleeKey").GetValue<KeyBind>().Active)
            {
                if (Menu.Item("FleeUseE").IsActive())
                {
                    Obj_AI_Hero target =
                        ObjectManager.Get<Obj_AI_Hero>().Where(
                            x => Vector3.Distance(Player.ServerPosition, x.Position) < E.Range && x.IsEnemy && x.IsTargetable && !x.IsDead)
                            .OrderByDescending(x => Vector3.Distance(Player.ServerPosition, x.Position))
                            .FirstOrDefault();
                    if (target != null)
                        E.Cast(target);
                }
            }
        }

        private static void KS()
        {
            //SOON
        }

        private static void UseItems()
        {
            //BOTRK
            var borkTarget = TargetSelector.GetTarget(Botrk.Range, TargetSelector.DamageType.Physical);
            var cutlasstarget = TargetSelector.GetTarget(Cutlass.Range, TargetSelector.DamageType.Magical);

            switch (Menu.Item("botrkcombotype").GetValue<StringList>().SelectedIndex)
            {
                case 0: //Normal
                    Game.Say("BOTRK");
                    if (Cutlass.IsOwned(Player))
                        if (cutlasstarget.IsValidTarget() && Cutlass.IsReady())
                        {
                            Cutlass.Cast(cutlasstarget);
                        }

                    if (Botrk.IsOwned(Player))
                        if (Botrk.IsInRange(borkTarget) && Botrk.IsReady())
                        {
                            Botrk.Cast(borkTarget);
                        }

                    break;

                case 1: //KS

                    if (Player.CalcDamage(borkTarget, Damage.DamageType.Physical, borkTarget.MaxHealth * 0.1) > borkTarget.Health) //ty Sebby <3
                    {
                        Botrk.Cast(borkTarget);
                    }

                    if (Player.CalcDamage(cutlasstarget, Damage.DamageType.Magical, 100) > cutlasstarget.Health)
                    {
                        Cutlass.Cast(cutlasstarget);
                    }

                    break;
            }

            if (Youmuus.IsReady() && Player.CountEnemiesInRange(Q.Range + 400) > 0 && Menu.Item("UseYoumuus").GetValue<bool>())
            {
                Youmuus.Cast();
            }
        }

        private static void UseIgnite()
        {
            var target = TargetSelector.GetTarget(550, TargetSelector.DamageType.True);

            if (Ignite.IsReady() && Menu.Item("UseIgnite").GetValue<bool>())
            {
                if (Player.CountEnemiesInRange(1000) >= Player.CountAlliesInRange(1000))
                {
                    if (target.IsValidTarget() && Player.GetSpellDamage(target, Ignite) > target.Health)
                    {
                        Player.Spellbook.CastSpell(Ignite, target);
                    }
                }
            }
        }

        private static void SmiteKS()
        {
            var target = TargetSelector.GetTarget(2000, TargetSelector.DamageType.Magical);

            if (SmiteSlot.GetDamage(target) > target.Health && Menu.Item("SmiteKS").GetValue<bool>() && target.IsValidTarget(570) && SmiteSlot.CanCast(target))
            {
                SmiteSlot.Cast(target);
            }
        }

        private static void SmiteCombo() //Stolen from blackt34 :P
        {
            switch (Menu.Item("smitecombotype").GetValue<StringList>().SelectedIndex)
            {
                case 0:

                    var target = TargetSelector.GetTarget(2000, TargetSelector.DamageType.Magical);  //Normal

                    if (target.IsValidTarget(570) && SmiteSlot.CanCast(target) && Orbwalker.ActiveMode.ToString() == "Combo")
                    {
                        SmiteSlot.Slot = smiteSlot;
                        Player.Spellbook.CastSpell(smiteSlot, target);
                    }
                    break;

                case 1:

                    var target2 = TargetSelector.GetTarget(2000, TargetSelector.DamageType.Magical); //KS

                    if (target2.IsValidTarget(570) && SmiteSlot.CanCast(target2) && Orbwalker.ActiveMode.ToString() == "Combo")
                    {
                        if (SmiteSlot.GetDamage(target2) > target2.Health)
                        {
                            Player.Spellbook.CastSpell(smiteSlot, target2);
                        }
                    }
                    break;
            }
        }

        /*private static void AutoLvL()
        {
            if (Menu.Item("AutoLevel").GetValue<bool>())
            {
                AutoLevel.Enable();
                AutoLevel.UpdateSequence(AUTO_LEVEL_SEQUENCE);
            }
            else
            {
                AutoLevel.Disable();
            }
        }*/

        public static void setSmiteSlot() //Also snacked from blackt34 :P ty mi friend
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteplayerganker", StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                SmiteSlot = new Spell(smiteSlot, 570);
                return;
            }
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteduel", StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                SmiteSlot = new Spell(smiteSlot, 570);
                return;
            }
        }

        private static void DiveLogic()
        {
            //SOON
        }

        private static void TrollMode()
        {
            //SOON
        }

        private static void AutoPots()
        {
            if (Menu.Item("AutoPots").GetValue<bool>() && Player.CountEnemiesInRange(1200) > 0 && !Player.InFountain()
                && !Player.HasBuff("Recall") && !Player.HasBuff("RegenerationPotion", true) && Player.HealthPercent < 60)
            {
                HealthPot.Cast();
            }

            if (Menu.Item("AutoPots").GetValue<bool>() && Player.Mana < Q.ManaCost + W.ManaCost + E.ManaCost + R.ManaCost
                && !Player.HasBuff("FlaskOfCrystalWater", true) && !Player.HasBuff("Recall") && Player.CountEnemiesInRange(1200) > 0 && !Player.InFountain())
            {
                ManaPot.Cast();
            }
        }
    }
}