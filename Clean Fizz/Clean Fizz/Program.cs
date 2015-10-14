#region
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion
namespace CLEAN_FIZZ
{
    internal class Program
    {
        public const string ChampionName = "Fizz";
        static Orbwalking.Orbwalker Orbwalker;
        static Menu Menu;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("Clean Fizz (FREELO)", ObjectManager.Player.ChampionName, true);

            Menu OrbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(OrbwalkerMenu);

            Menu TargetSelectorMenu = Menu.AddSubMenu(new Menu("Target Selector", "TargetSelector"));
            TargetSelector.AddToMenu(TargetSelectorMenu);

            Menu.AddToMainMenu();
            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, Orbwalking.GetRealAutoAttackRange(ObjectManager.Player));
            E = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 1300);
            E.SetSkillshot(0.25f, 330, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 80, 1200, false, SkillshotType.SkillshotLine);

            Game.OnUpdate += Game_OnGameUpdate;

        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:

                    var Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

                    if (R.IsReady()) R.Cast(Target);
                    if (Q.IsReady()) Q.Cast(Target);
                    if (W.IsReady()) W.Cast();
                    if (E.IsReady()) E.Cast(Target);
                    break;
            }
        }
    }
}