using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Threading.Tasks;

namespace BearGryllsWarwick
{
    class Program
    {
        static Menu nmenu;
        static Spell Q, W, R;
        public static Items.Item Pot = new Items.Item(2003, 0);
        public static Items.Item MPot = new Items.Item(2004, 0);
        static readonly Obj_AI_Hero Player = ObjectManager.Player;
        static Orbwalking.Orbwalker Orbwalker;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Warwick")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 1250);
            R = new Spell(SpellSlot.R, 700);

            nmenu = new Menu("BearGryllsWarwick", "menu", true);

            var tsmenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsmenu);
            nmenu.AddSubMenu(tsmenu);

            {
                var orbwalkermenu = new Menu("Orbwalker", "Orbwalker");
                Orbwalker = new Orbwalking.Orbwalker(orbwalkermenu);
                nmenu.AddSubMenu(orbwalkermenu);

                var targetsmenu = new Menu("Target Selector", "TargetSelector");
                TargetSelector.AddToMenu(targetsmenu);
                nmenu.AddSubMenu(targetsmenu);

                var MainMenu = new Menu("Main Menu", "MainMenu");
                MainMenu.AddItem(new MenuItem("BearGryllsWarwick.MainMenu.Q", "Use Q Combo").SetValue(true));
                MainMenu.AddItem(new MenuItem("BearGryllsWarwick.MainMenu.W", "Use W Combo").SetValue(true));
                MainMenu.AddItem(new MenuItem("BearGryllsWarwick.MainMenu.R", "Use R Combo").SetValue(true));
                MainMenu.AddItem(new MenuItem("BearGryllsWarwick.MainMenu.abc", "-------------------"));
                MainMenu.AddItem(new MenuItem("BearGryllsWarwick.MainMenu.Qharass", "Use Q Harass").SetValue(true));
                MainMenu.AddItem(new MenuItem("BearGryllsWarwick.MainMenu.AutoPots", "Auto Pots").SetValue(true));
                MainMenu.AddItem(new MenuItem("BearGryllsWarwick.MainMenu.KS", "Smart KS").SetValue(false));
                MainMenu.AddItem(new MenuItem("BearGryllsWarwick.MainMenu.interrupt", "Interrupt with Ult").SetValue(false));
            }

            nmenu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;

        }

        static void Drawing_OnDraw(EventArgs args)
        {
            //Q
            if (nmenu.Item("BearGryllsWarwick.Drawing.Q").GetValue<bool>())
            {
                if (Q.IsReady()) { Render.Circle.DrawCircle(Player.ServerPosition, Q.Range, Color.HotPink); }
            }
            //R
            if (nmenu.Item("BearGryllsWarwick.Drawing.R").GetValue<bool>())
            {
                if (R.IsReady()) { Render.Circle.DrawCircle(Player.ServerPosition, R.Range, Color.HotPink); }
            }
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }

            AutoPots();
            KS();
        }

        static void Combo()
        {
            var target = TargetSelector.GetTarget(400, TargetSelector.DamageType.Physical);
            if (W.IsReady() && Player.CountEnemiesInRange(1250) > 0)
            {
                W.Cast();
            }

            if (R.IsReady())
            {
                if (R.GetDamage(target) > target.Health || Q.GetDamage(target) + R.GetDamage(target) > target.Health)
                {
                    RQCast();
                }
                else if (Player.CountAlliesInRange(1500) > Player.CountEnemiesInRange(1500))
                {
                    R.Cast(target);
                }
            }

            if (Q.IsReady() && Q.IsInRange(target))
            {
                Q.Cast(target);
            }
        }

        static void Harass()
        {
            var target = TargetSelector.GetTarget(400, TargetSelector.DamageType.Physical);
            if (Q.IsReady() && Player.Mana > Player.Mana / 100 * 60 && nmenu.Item("BearGryllsWarwick.MainMenu.Qharass").GetValue<bool>())
            {
                Q.Cast(target);
            }
        }

        static void UltLogic()
        {
            var target = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
            if (R.IsReady() && R.GetDamage(target) >= target.Health)
            {
                if (Q.IsReady() && R.IsReady())
                {
                    RQCast();
                }
            }
        }

        static void RQCast()
        {
            var target = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
            if (Q.IsReady())
            {
                Q.Cast(target);
            }
            if (R.IsReady())
            {
                R.Cast(target);
            } 
        }

        static void KS()
        {
            var target = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);
            if (Q.IsReady() && nmenu.Item("BearGryllsWarwick.MainMenu.KS").GetValue<bool>() && Q.GetDamage(target) > target.Health)
            {
                Q.Cast(target);
            }
        }

        static void OnInterruptableSpell(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (nmenu.Item("BearGryllsWarwick.MainMenu.interrupt").GetValue<bool>() && R.IsReady() && unit.IsValidTarget(R.Range))
                R.Cast();
        }

        public static void AutoPots()
        {
            if (nmenu.Item("BearGryllsWarwick.MainMenu.AutoPots").GetValue<bool>() && !ObjectManager.Player.InFountain() && !ObjectManager.Player.HasBuff("Recall") && Pot.IsReady() && !ObjectManager.Player.HasBuff("RegenerationPotion", true))
            {
                if (ObjectManager.Player.CountEnemiesInRange(1200) > 0 && ObjectManager.Player.Health < ObjectManager.Player.Health / 100 * 60)
                    Pot.Cast();
            }

            if (MPot.IsReady() && !ObjectManager.Player.HasBuff("FlaskOfCrystalWater", true) && nmenu.Item("BearGryllsWarwick.MainMenu.AutoPots").GetValue<bool>())
            {
                if (ObjectManager.Player.CountEnemiesInRange(1200) > 0 && !ObjectManager.Player.HasBuff("Recall") && Pot.IsReady() && ObjectManager.Player.Mana < R.Instance.ManaCost + Q.Instance.ManaCost + W.Instance.ManaCost)
                    MPot.Cast();
            }
        }
    }
}
