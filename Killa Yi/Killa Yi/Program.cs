#region Stuff
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


#endregion Stuff

namespace Killa_Karate_YI
{
    internal class Program
    {
        public const string ChampionName = "MasterYi";

        //Spells

        static Spell SmiteSlot;
        public static SpellSlot smiteSlot = SpellSlot.Unknown;
        public static Items.Item HealthPot = new Items.Item(2003, 0);
        public static Items.Item ManaPot = new Items.Item(2004, 0);
        public static Items.Item BOTRK = new Items.Item(3153, 550f);
        public static Items.Item Hydra = new Items.Item(3074, 440f);
        public static Items.Item Youmuus = new Items.Item(3142, 0);
        public static Items.Item HydraTitanic = new Items.Item(3748, 150f);
        static Orbwalking.Orbwalker Orbwalker;
        static Menu Menu;
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        //Menu
        public static Menu Config;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {

            if (Player.ChampionName != ChampionName)
                return;

            Notifications.AddNotification("Welcome to Master Yisus", 10000);
            Notifications.AddNotification("Press da Spacebar", 10000);

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            Menu = new Menu("MasterYi", Player.ChampionName, true);

            Menu OrbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(OrbwalkerMenu);

            Menu TargetSelectorMenu = Menu.AddSubMenu(new Menu("Target Selector", "TargetSelector"));
            TargetSelector.AddToMenu(TargetSelectorMenu);

            Menu ComboMenu = Menu.AddSubMenu(new Menu("Combo", "Combo"));
            ComboMenu.AddItem(new MenuItem("ComboUseSmite", "Use Smite").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboUseItems", "Use Items").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboUseR", "Use Smart R").SetValue(true));

            Menu HarassMenu = Menu.AddSubMenu(new Menu("Harass", "Harrash"));
            HarassMenu.AddItem(new MenuItem("HarassUseQ", "Use Q").SetValue(true));
            HarassMenu.AddItem(new MenuItem("HarassUseE", "Use E").SetValue(true));
            HarassMenu.AddItem(new MenuItem("HarassManaManager", "Mana Manager (%)").SetValue(new Slider(70, 1, 100)));

            Menu JungleLaneClear = Menu.AddSubMenu(new Menu("Jungle Clear", "Jungle Clear"));
            JungleLaneClear.AddItem(new MenuItem("JungleClearUseQ", "Use Jungleclear Q").SetValue(true));
            JungleLaneClear.AddItem(new MenuItem("JungleClearUseE", "Use JungleclearE").SetValue(true));
            JungleLaneClear.AddItem(new MenuItem("JungleClearManager", "Jungleclear Mana Manager (%)").SetValue(new Slider(10, 1, 100)));

            Menu LaneClear = Menu.AddSubMenu(new Menu("Lane Clear", "JungleLaneClear"));
            LaneClear.AddItem(new MenuItem("LaneClearUseQ", "Use Laneclear Q").SetValue(true));
            LaneClear.AddItem(new MenuItem("LaneClearUseE", "Use Laneclear E").SetValue(true));
            LaneClear.AddItem(new MenuItem("LaneClearManaManager", "Laneclear Mana Manager (%)").SetValue(new Slider(60, 1, 100)));

            /*
            Menu Activator = Menu.AddSubMenu(new Menu("Activator", "Activator"));
            Activator.AddItem(new MenuItem("UseHeal", "Use Heal").SetValue(true));
            Activator.AddItem(new MenuItem("UseBarrier", "Use Barrier").SetValue(true));
            Activator.AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));
            */

            /*Menu FleeMenu = Menu.AddSubMenu(new Menu("Flee", "Flee"));
            FleeMenu.AddItem(new MenuItem("FleeUseQ", "Use Q").SetValue(true));
            FleeMenu.AddItem(new MenuItem("FleeKey", "Flee Key").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Press)));
            */

            Menu MiscMenu = Menu.AddSubMenu(new Menu("Misc", "Misc"));
            MiscMenu.AddItem(new MenuItem("AutoPots", "Smart Auto Pots").SetValue(true));
            MiscMenu.AddItem(new MenuItem("AntiGapCloserQ", "Anti Gapcloser Q").SetValue(true));



            Menu DrawingMenu = Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            DrawingMenu.AddItem(new MenuItem("DrawQ", "Draw Q Range").SetValue(true));

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

            if (Menu.Item("AntiGapCloserQ").GetValue<bool>() && Q.IsReady() && ObjectManager.Player.Mana > R.ManaCost + Q.ManaCost)
            {
                if (Target.IsValidTarget(Q.Range))
                {
                    Q.Cast(Target);
                }
            }
        }

        public static void Game_ProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        { 
            if (((Obj_AI_Hero)sender).ChampionName.ToLower() == "vayne"
                && args.Slot == SpellSlot.E)
            {
                if (Q.IsReady())
                {
                    Q.Cast(sender);
                }
            }
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Menu.Item("UseItems").GetValue<bool>())
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    if (!Player.IsWindingUp && Hydra.IsReady() && Hydra.IsOwned() && Player.CountEnemiesInRange(300) > 0)
                        Hydra.Cast();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Render.Circle.DrawCircle(ObjectManager.Player.Position, Player.AttackRange + 150, Color.Green);
            if (Menu.Item("DrawQ").GetValue<bool>())
                if (Q.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Teal);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();

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
            }
            AutoPots();
            KS();
            Smite();
            setSmiteSlot();
            Flee();
            //AutoLvL();
        }

        private static void Smite()
        {

        }

        private static void KS()
        {
            var Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (Q.IsReady() && Q.GetDamage(Target) > Target.Health)
            {
                Q.Cast(Target);
            }
        }

        private static void Combo()
        {
            var Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            RLogic();

            if (Hydra.IsReady() && Hydra.IsOwned() && Player.CountEnemiesInRange(300) > 0 && Menu.Item("ComboUseItems").GetValue<bool>())
            {
                Hydra.Cast();
            }

            if (Youmuus.IsReady() && Player.CountEnemiesInRange(Q.Range) > 0 && Menu.Item("ComboUseItems").GetValue<bool>())
            {
                Youmuus.Cast();
            }

            QLogic();

            if (E.IsReady() && Player.CountEnemiesInRange(Q.Range) > 0)
            {
                E.Cast();
            }

            if (BOTRK.IsReady() && BOTRK.IsOwned() && Menu.Item("ComboUseItems").GetValue<bool>())
            {
                BOTRK.Cast(Target);
            }

            if (HydraTitanic.IsOwned() && HydraTitanic.IsReady() && Player.CountEnemiesInRange(150) > 0 && Menu.Item("ComboUseItems").GetValue<bool>())
            {
                HydraTitanic.Cast(Target);
            }
        }

        private static void Harass()
        {
            var Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (Q.IsReady() && Menu.Item("HarassUseQ").GetValue<bool>())
            {
                Q.Cast(Target);
            }
            if (E.IsReady() && Menu.Item("HarassUseE").GetValue<bool>())
            {
                E.Cast();
            }
        }

        private static void JungleClear()
        {

        }

        private static void LaneClear()
        {
            if (Player.ManaPercent > Menu.Item("LaneClearManaManager").GetValue<Slider>().Value)
            {
                var MinionN =
                      MinionManager.GetMinions(800, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health)
                          .FirstOrDefault();
                var QLane = Menu.Item("LaneClearUseQ").GetValue<bool>();
                if (QLane)
                {
                    if (MinionN != null)
                    {
                        Q.Cast(MinionN);
                    }
                }

                if (Menu.Item("LaneClearUseE").GetValue<bool>())
                {
                    E.Cast();
                }
            }
        }

        private static void Flee()
        {

        }

        private static void QLogic()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (Q.IsReady() && Player.CountEnemiesInRange(Q.Range) > 0 && Vector3.Distance(Player.ServerPosition, target.Position) > Player.AttackRange + 200)
            {
                Game.Say("Q1 RANGE");
                Q.Cast(target);
            }
            else if (Q.IsReady() && Player.CountEnemiesInRange(Q.Range) > 0 && Player.HealthPercent < target.HealthPercent)
            {
                Game.Say("Q2 under %");
                Q.Cast(target);
            }
        }

        private static void RLogic()
        {
            var target = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
            if (R.IsReady() && Player.CountEnemiesInRange(Q.Range) > 0 && Menu.Item("ComboUseR").GetValue<bool>() && target.Health > target.Health / 100 * 20 && Vector3.Distance(Player.ServerPosition, target.Position) <= 900)
            {
                R.Cast();
            }
            else
                return;
        }

        public static void setSmiteSlot()
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

        private static void AutoPots()
        {
            if (Menu.Item("AutoPots").GetValue<bool>() && Player.CountEnemiesInRange(1200) > 0 && !Player.InFountain()
                && !Player.HasBuff("Recall") && !Player.HasBuff("RegenerationPotion", true) && Player.HealthPercent < 60)
                HealthPot.Cast();


            if (Menu.Item("AutoPots").GetValue<bool>() && Player.Mana < Q.ManaCost + E.ManaCost + R.ManaCost
                && !Player.HasBuff("FlaskOfCrystalWater", true) && !Player.HasBuff("Recall") && Player.CountEnemiesInRange(1200) > 0 && !Player.InFountain())
                ManaPot.Cast();
        }
    }
}