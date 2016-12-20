using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Enumerations;
using Color1 = System.Drawing.Color;

namespace JungleTimer
{
    class AutoSmite
    {
        public static double Damage = 0;

        public static Spell.Targeted Smite;

        public static Text SmiteStatus;

        public static Obj_AI_Base Monster;

        public static string[] SmiteNames =
        {
            "SummonerSmite", "S5_SummonerSmitePlayerGanker", "S5_SummonerSmiteDuel"
        };
        public static string[] MonstersNames =
        {
            "TT_Spiderboss", "TTNGolem", "TTNWolf", "TTNWraith",
            "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak",
            "SRU_Red", "SRU_Krug", "Sru_Crab", "SRU_Baron", "SRU_RiftHerald",
            "SRU_Dragon_Elder", "SRU_Dragon_Air", "SRU_Dragon_Earth",
            "SRU_Dragon_Fire", "SRU_Dragon_Water"
        };
        public static Menu RootMenu = Program.RootMenu, MobsMenu, DrawingsMenu;

        public static int Init()
        {
            Console.WriteLine("Smite loading");

            if (SmiteNames.Contains(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner1).Name))
            {
                Console.WriteLine("smite in slot 1");
                Smite = new Spell.Targeted(SpellSlot.Summoner1, (uint)570f);
            }
            if (SmiteNames.Contains(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner2).Name))
            {
                Console.WriteLine("Smite in slot 2");
                Smite = new Spell.Targeted(SpellSlot.Summoner2, (uint)570f);
            }

            if (Smite == null)
            {
                Console.WriteLine("no smite");
                return -1;
            }


            SmiteStatus = new Text("", new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, 9, System.Drawing.FontStyle.Bold));
            return 0;
        }

        public static void AddSmiteMenu(Menu root)
        {
            root.AddGroupLabel("AutoSmite Settings");
            root.AddSeparator();
            

            root.Add("active", new CheckBox("Enabled"));
            root.Add("activekey", new KeyBind("Enabled (Hold Key)", true, KeyBind.BindTypes.PressToggle));
            //if (SupportedChampions.Contains(ObjectManager.Player.ChampionName))
            //    RootMenu.Add("disable", new CheckBox("Enabled Champions skill usage (" + ObjectManager.Player.ChampionName + ")", true));
            root.AddSeparator();
            

            MobsMenu = root.AddSubMenu("Monsters", "Monsters");
            MobsMenu.AddGroupLabel("Monster Settings");
            MobsMenu.AddSeparator();
            MobsMenu.Add("killsmite", new CheckBox("KS Enemy with Smite"));
            MobsMenu.AddSeparator();

            if (Game.MapId == GameMapId.TwistedTreeline)
            {
                MobsMenu.Add("TT_Spiderboss", new CheckBox("Vilemaw Enabled"));
                MobsMenu.Add("TT_NGolem", new CheckBox("Golem Enabled"));
                MobsMenu.Add("TT_NWolf", new CheckBox("Wolf Enabled"));
                MobsMenu.Add("TT_NWraith", new CheckBox("Wraith Enabled"));
            }
            else
            {
                MobsMenu.Add("SRU_Baron", new CheckBox("Baron Enabled"));
                MobsMenu.Add("SRU_RiftHerald", new CheckBox("RiftHerald Enabled"));
                MobsMenu.Add("SRU_Blue", new CheckBox("Blue Enabled"));
                MobsMenu.Add("SRU_Red", new CheckBox("Red Enabled"));
                MobsMenu.Add("SRU_Gromp", new CheckBox("Gromp Enabled"));
                MobsMenu.Add("SRU_Murkwolf", new CheckBox("Murkwolf Enabled"));
                MobsMenu.Add("SRU_Krug", new CheckBox("Krug Enabled"));
                MobsMenu.Add("SRU_Razorbeak", new CheckBox("Razorbeak Enabled"));
                MobsMenu.Add("Sru_Crab", new CheckBox("Crab Enabled"));
                MobsMenu.Add("SRU_Dragon_Elder", new CheckBox("Dragon Elder Enabled"));
                MobsMenu.Add("SRU_Dragon_Air", new CheckBox("Dragon Air Enabled"));
                MobsMenu.Add("SRU_Dragon_Earth", new CheckBox("Dragon Earth Enabled"));
                MobsMenu.Add("SRU_Dragon_Fire", new CheckBox("Dragon Fire Enabled"));
                MobsMenu.Add("SRU_Dragon_Water", new CheckBox("Dragon Water Enabled"));
            }

            DrawingsMenu = root.AddSubMenu("Drawing", "drawing");
            DrawingsMenu.AddGroupLabel("Drawing Settings");
            DrawingsMenu.AddSeparator();
            DrawingsMenu.Add("draw", new CheckBox("Enabled"));
            DrawingsMenu.AddSeparator(10);
            DrawingsMenu.Add("smite", new CheckBox("Draw Smite Range"));
            DrawingsMenu.Add("drawTxt", new CheckBox("Draw Text"));
            DrawingsMenu.Add("killable", new CheckBox("Draw Circle on Killable Monster"));
        }

        internal static void Drawing_OnDraw(EventArgs args)
        {
            
            if (DrawingsMenu["draw"].Cast<CheckBox>().CurrentValue == false)
                return;
        
            if (DrawingsMenu["drawTxt"].Cast<CheckBox>().CurrentValue)
            {
                if (Program.RootMenu["active"].Cast<CheckBox>().CurrentValue || Program.RootMenu["activekey"].Cast<KeyBind>().CurrentValue)
                {
                    SmiteStatus.Position = Drawing.WorldToScreen(Player.Instance.Position) - new Vector2(40, -40);
                    SmiteStatus.Color = Color1.CadetBlue;
                    SmiteStatus.TextValue = "Smite : ON";
                    SmiteStatus.Draw();
                }
                else
                {
                    SmiteStatus.Position = Drawing.WorldToScreen(Player.Instance.Position) - new Vector2(40, -40);
                    SmiteStatus.Color = Color1.DarkRed;
                    SmiteStatus.TextValue = "Smite : OFF";
                    SmiteStatus.Draw();
                }
                
            }
            
            if (DrawingsMenu["smite"].Cast<CheckBox>().CurrentValue)
            {
                if (Smite.IsReady())
                    new Circle() { Color = Color1.CadetBlue, Radius = 500f + 20, BorderWidth = 2f }.Draw(ObjectManager.Player.Position);
                else
                    new Circle() { Color = Color1.DarkRed, Radius = 500f + 20, BorderWidth = 2f }.Draw(ObjectManager.Player.Position);
                    
            }
            
            if (DrawingsMenu["killable"].Cast<CheckBox>().CurrentValue)
            {
                Monster = GetNearest(ObjectManager.Player.ServerPosition);
                if (Monster != null)
                {
                    if (Monster.Health <= (Damage == 0 ? GetSmiteDamage() : Damage) && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < 900f)
                        Circle.Draw(Color.Purple, 100f, Monster.ServerPosition);
                }
            }
            
        }

        internal static void Game_OnUpdate(EventArgs args)
        {
            if (Program.RootMenu["active"].Cast<CheckBox>().CurrentValue || Program.RootMenu["activekey"].Cast<KeyBind>().CurrentValue)
            {
                //double SpellDamage = 0;
                Monster = GetNearest(ObjectManager.Player.ServerPosition);
                Console.WriteLine("Nearest Mob: " + Monster.Name + ", " + Monster.BaseSkinName);
                if (Monster != null && MobsMenu[Monster.BaseSkinName].Cast<CheckBox>().CurrentValue)
                {
                    if (Smite.IsReady() && Monster.Health <= GetSmiteDamage() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                    {
                        Smite.Cast(Monster);
                        Damage = 0;
                    }
                }
            }
        }

        public static int GetSmiteDamage()
        {
            int[] CalcSmiteDamage =
            {
                20 * ObjectManager.Player.Level + 370,
                30 * ObjectManager.Player.Level + 330,
                40 * ObjectManager.Player.Level + 240,
                50 * ObjectManager.Player.Level + 100
            };
            return CalcSmiteDamage.Max();
        }

        static double SmiteChampDamage()
        {
            if (Smite.Slot == Extensions.GetSpellSlotFromName(ObjectManager.Player, "s5_summonersmiteduel"))
            {
                var damage = new int[] { 54 + 6 * ObjectManager.Player.Level };
                return Player.CanUseSpell(Smite.Slot) == SpellState.Ready ? damage.Max() : 0;
            }

            if (Smite.Slot == Extensions.GetSpellSlotFromName(ObjectManager.Player, "s5_summonersmiteplayerganker"))
            {
                var damage = new int[] { 20 + 8 * ObjectManager.Player.Level };
                return Player.CanUseSpell(Smite.Slot) == SpellState.Ready ? damage.Max() : 0;
            }
            return 0;
        }

        public static Obj_AI_Minion GetNearest(Vector3 pos)
        {
            var mobs = ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValid && MonstersNames.Any(name => 
                minion.Name.ToLower().StartsWith(name.ToLower())) && !MonstersNames.Any(name => minion.Name.Contains("Mini")) && !MonstersNames.Any(name => 
                    minion.Name.Contains("Spawn")));
            var objAimobs = mobs as Obj_AI_Minion[] ?? mobs.ToArray();
            Obj_AI_Minion NearestMonster = objAimobs.FirstOrDefault();
            double? nearest = null;
            foreach (Obj_AI_Minion monster in objAimobs)
            {
                double distance = Vector3.Distance(pos, Monster.Position);
                if (nearest == null || nearest > distance)
                {
                    nearest = distance;
                    NearestMonster = monster;
                }
            }
            return NearestMonster;
        }
    }
}
