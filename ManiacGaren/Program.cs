using System;
using System.Drawing;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

/*
    Features: 

    Auto Cancel E to kill with R
    Use Q, W, E on Combo 
    KS with R
    Show text if enemy is killable
    
     */
     
namespace ManiacGaren
{
    class Program
    {
        private static Spell.Active Q, W, E;
        private static Spell.Targeted R;
        private static Menu home;
        private static string canKill = "Not Killable";

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete; //Game Started?
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Garen") //Are we Garen?
            {
                return;
            }
            LoadSpells(); //Load QWER
            LoadMenu(); //Load Menu
            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Chat.Print("ManiacGaren successfuly loaded!. Enjoy :)");
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var color = Color.Red;
            foreach (var enemy in EntityManager.Heroes.Enemies) //Checks for every enemy
            {
                if (enemy.IsVisible && enemy.IsValidTarget() && !enemy.IsDead) //Are they visible, valid, and alive?
                {
                    if (enemy.Health < Q.GetSpellDamage(enemy) && Q.IsReady()) //Can we kill with Q?
                    {
                        canKill = "Kill with Q";
                        color = Color.Lime;
                    }
                    if (enemy.Health < E.GetSpellDamage(enemy) * 7 && E.IsReady()) //Can we kill with E?
                    {
                        canKill = "Kill with E";
                        color = Color.Lime;
                    }
                    if (enemy.Health < R.GetSpellDamage(enemy) && R.IsReady()) //Can we kill with R?
                    {
                        canKill = "Kill with R";
                        color = Color.Lime;
                    }
                    Drawing.DrawText(enemy.HPBarPosition.X, enemy.HPBarPosition.Y + 50, color, canKill, 25); //Show text under enemy HP Bar showing we can kill them
                }
                if (!enemy.IsVisible || enemy.IsDead) //Are they not visible or dead?
                {
                    canKill = "Not Killable";
                }
            }
        }

        private static void LoadMenu()
        {
            home = MainMenu.AddMenu("ManiacGaren", "ManiacGaren");
            home.Add("UseQ", new CheckBox("Use Q in Combo"));
            home.Add("UseW", new CheckBox("Use W in Combo")); 
            home.Add("UseE", new CheckBox("Use E in Combo"));
            home.Add("UseR", new CheckBox("Use R in Combo"));
            home.Add("CancelE", new CheckBox("Cancel E to kill with R"));
            home.Add("DrawKill", new CheckBox("Show text if can kill enemy"));
        }

        private static bool GetCheckbox(Menu menu, string value) //Gets value of a checkbox
        {
            return menu[value].Cast<CheckBox>().CurrentValue; 
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) //Is Spacebar being press?
            {
                Combo(); //Do Combo
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(R.Range + 100, DamageType.Mixed); //Get Target

            if (target != null) //Is target valid?
            {
                if (IsSpinning()) //Are we spinning?
                {
                    Orbwalker.DisableAttacking = true; //Disable AutoAttack
                }
                else
                {
                    Orbwalker.DisableAttacking = false; //Enable AutoAttack
                }
                if (GetCheckbox(home, "UseR") && R.IsReady()) //Can we use R?
                {
                    var dmg = Player.Instance.GetSpellDamage(target, SpellSlot.R); //R Damage
                    
                    if (dmg > target.Health && IsSpinning() && GetCheckbox(home, "CancelE")) //Can we kill but we are spinning?
                    {
                        E.Cast(); //Stop Spin
                        R.Cast(target); //Kill with R
                    }
                    if (dmg > target.Health && !IsSpinning()) //Can we kill and we are not spinning?
                    {
                        R.Cast(target); //Kill with R
                    }
                }

                if (GetCheckbox(home, "UseQ") && Q.IsReady()) //Can we use Q?
                {
                    Q.Cast(); //Use Q
                }

                if (GetCheckbox(home, "UseW") && W.IsReady() && Player.Instance.Distance(target.Position) <= 200) //Can we use W and we are in a short distance?
                {
                    W.Cast(); //Use W
                }

                if (GetCheckbox(home, "UseE") && E.IsInRange(target) && E.IsReady() && !IsQ() && !IsSpinning()) //Can we use E and we are in range, and we are not in Q, and we are not going to cancel E?
                {
                    E.Cast(); //Use E
                }
            }
        }

        private static void LoadSpells()
        {
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E, 300);
            R = new Spell.Targeted(SpellSlot.R, 400, DamageType.Magical);

            // To get information about spells, use the wikia.

            // In this case, we used: http://leagueoflegends.wikia.com/wiki/Garen
        }

        private static bool IsSpinning() //Check if we are spinning
        {
            return Player.Instance.HasBuff("GarenE");
        } 

        private static bool IsQ() //Checks if we have Q Buff
        {
            return Player.Instance.HasBuff("GarenQ");
        }
    }
}


// Hope you guys enjoyed this "Tutorial".

// I hope i managed to encourage you to make addons :D

// Best of lucks... BoliBerrys.