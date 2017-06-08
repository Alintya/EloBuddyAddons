using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Initiate();
        }

        private static bool _attacking;

        private const float DeltaT = .2f;

        private static Vector3 _lastWaypoint;

        private static GameObjectOrder _lastOrder;

        private static float _lastOrderTime;

        private static float _lastTime;

        private static AIHeroClient _player;

        private static readonly Random Random = new Random();

        private static Menu _mainMenu;


        private static void LoadMenu()
        {
            _mainMenu = MainMenu.AddMenu("FakeClicks", "FakeClicks");
            _mainMenu.AddGroupLabel("Intro");
            _mainMenu.Add("enable", new CheckBox("Enable"));
            _mainMenu.Add("clickMode", new ComboBox("Click Mode", new [] { "Evade compatible", "Cursor Position, No Evade" }));
        }

        private static void OnPostAttack(AttackableUnit unit, EventArgs args)
        {
            _attacking = false;

            if (unit.IsMe)
            {
                ////////////////////////////////////////////////////////////////
                Hud.ShowClick(ClickType.Move, RandomizePosition(unit.Direction));
            }
        }


        private static void OnPreAttack(AttackableUnit t, Orbwalker.PreAttackArgs args)
        {
            if (_mainMenu["clickMode"].Cast<ComboBox>().SelectedIndex == 1)
            {
                Hud.ShowClick(ClickType.Attack, RandomizePosition(args.Target.Position));
                _attacking = true;
            }
        }

        private static void OnCastSpell(Spellbook s, SpellbookCastSpellEventArgs args)
        {
            if (args.Target.Position.Distance(_player.Position) >= 5f)
            {
                Hud.ShowClick(ClickType.Attack, args.Target.Position);
            }
        }

        private static void DrawFake(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (sender.IsMe && 
                _lastTime + DeltaT < Game.Time && args.Path.LastOrDefault() != _lastWaypoint &&
                args.Path.LastOrDefault().Distance(_player.ServerPosition) >= 5f &&
                _mainMenu["enable"].Cast<CheckBox>().CurrentValue &&
                _mainMenu["clickMode"].Cast<ComboBox>().SelectedIndex == 1)
            {
                _lastWaypoint = args.Path.LastOrDefault();

                Hud.ShowClick(_attacking ? ClickType.Attack : ClickType.Move, Game.CursorPos);

                _lastTime = Game.Time;
            }
        }

        private static void OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (sender.IsMe &&
                (args.Order == GameObjectOrder.MoveTo || args.Order == GameObjectOrder.AttackUnit ||
                 args.Order == GameObjectOrder.AttackTo) &&
                _lastOrderTime + Random.NextFloat(DeltaT, DeltaT * 2) < Game.Time &&
                _mainMenu["enable"].Cast<CheckBox>().CurrentValue &&
                _mainMenu["clickMode"].Cast<ComboBox>().SelectedIndex == 0)
            {
                var vect = args.TargetPosition;
                vect.Z = _player.Position.Z;
                if (args.Order == GameObjectOrder.AttackUnit || args.Order == GameObjectOrder.AttackTo)
                {
                    Hud.ShowClick(ClickType.Attack, RandomizePosition(vect));
                }
                else
                {
                    Hud.ShowClick(ClickType.Move, vect);
                }

                _lastOrderTime = Game.Time;
            }
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            LoadMenu();
            Console.WriteLine("Menu loaded");
            _player = ObjectManager.Player;

            Obj_AI_Base.OnNewPath += DrawFake;
            Orbwalker.OnPreAttack += OnPreAttack;
            Spellbook.OnCastSpell += OnCastSpell;
            Orbwalker.OnPostAttack += OnPostAttack;
            Player.OnIssueOrder += OnIssueOrder;

            Hud.ShowClick(ClickType.Attack, Player.Instance.Position);

        }

        private static Vector3 RandomizePosition(Vector3 input)
        {
            if (Random.Next(2) == 0)
            {
                input.X += Random.Next(100);
            }
            else
            {
                input.Y += Random.Next(100);
            }

            return input;
        }

        public static void Initiate()
        {
            Loading.OnLoadingComplete += Game_OnGameLoad;
        }

    }
}
