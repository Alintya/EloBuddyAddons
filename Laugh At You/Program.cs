using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using System.IO;

namespace LaughAtYou
{
    public static class Program
    {
        public static Menu _Menu;
        public static int LastEmoteSpam = 0;
        public static int MyKills = 0;
        public static int MyAssits = 0;
        public static int MyDeaths = 0;
        public static Random Random;
        public static string[] KnownDisrespectStarts = new[] { "", "gj ", "nice ", "wp ", "lol gj ", "nice 1 ", "gg ", "very wp ", "ggwp ", "sweet ", "ty ", "thx ", "wow nice ", "lol ", "wow ", "so good ", "heh ", "hah ", "haha ", "hahaha ", "hahahaha ", "u did well ", "you did well ", "loved it ", "loved that ", "love u ", "love you ", "ahaha ", "ahahaha ", "gg noobz ", "ez kill ", "get rekt bomji ebanii" };
        public static string[] KnownDisrespectEndings = new[] { "", " XD", " XDD", " XDDD", " XDDD", "XDDDD", " haha", " hahaha", " hahahaha", " ahaha", " ahahaha", " lol", " rofl", " roflmao" };
        public static SpellSlot FlashSlot = SpellSlot.Unknown;
        public static SpellSlot IgniteSlot = SpellSlot.Unknown;
        public static int LastDeathNetworkId = 0;
        public static int LastChat = 0;
        public static readonly string FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EloBuddy", "LaughAtYou");
        public const string FileName = "LaughChat.txt";
        public static string FullFilePath = Path.Combine(FilePath, FileName);
        public static Dictionary<int, int> DeathsHistory = new Dictionary<int, int>();
        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnGameLoad;
        }

        public static void OnGameLoad(EventArgs args)
        {
            _Menu = MainMenu.AddMenu("Laugh At You", "laughingmenu");
            _Menu.Add("mode", new ComboBox("Mode", 1, "Laugh", "Mastery Badge", "Both"));
            _Menu.Add("chatmode", new ComboBox("Chat Disrespect Mode", 0, "DISABLED", "CHAMPION NAME", "SUMMONER NAME", "CUSTOM CHAT ONLY"));
            _Menu.Add("usecustomchat", new CheckBox("Use Custom Messages", false));
            _Menu.Add("onkill", new CheckBox("After Kill"));
            _Menu.Add("onassist", new CheckBox("After Assist"));
            _Menu.Add("ondeath", new CheckBox("After Death", false));
            _Menu.Add("neardead", new CheckBox("Near Dead Bodies"));
            _Menu.Add("afterignite", new CheckBox("After Ignite"));
            _Menu.Add("afterflash", new CheckBox("After Flash", false));
            _Menu.Add("afterq", new CheckBox("After Q", false));
            _Menu.Add("afterw", new CheckBox("After W", false));
            _Menu.Add("aftere", new CheckBox("After E", false));
            _Menu.Add("afterr", new CheckBox("After R", false));
            _Menu.AddGroupLabel("Humanizer");
            _Menu.Add("delaybadge", new Slider("Delay for Laugh & Mastery Badge (Milisec)", 1000, 500, 2000));
            _Menu.Add("delaychat", new Slider("Delay for Chat (milisec)", 5000, 1000, 10000));
            _Menu.Add("chatincb", new CheckBox("No Chat in Combo"));
            _Menu.Add("chatinlc", new CheckBox("No Chat in Laneclear", false));
            _Menu.Add("chatinjc", new CheckBox("No Chat in Jungle Clear", false));
            _Menu.Add("chatinlh", new CheckBox("No Chat in Last Hit", false));
            _Menu.Add("chatinhr", new CheckBox("No Chat in Harass", false));
            Random = new Random();
            FlashSlot = ObjectManager.Player.GetSpellSlotFromName("summonerflash");
            IgniteSlot = ObjectManager.Player.GetSpellSlotFromName("summonerdot");
            Game.OnTick += OnTick;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;

            if (!File.Exists(FullFilePath))
            {
                Directory.CreateDirectory(FilePath);
                File.Create(FullFilePath);
                File.WriteAllLines(FilePath + "/Instruction.txt", new string[] { "Write your custom messages in LaughChat.txt, devided in each line.", "Eg:", "good job! but no", "u nnoooooobb" });
            }

            foreach (var en in EntityManager.Heroes.Enemies)
            {
                DeathsHistory.Add(en.NetworkId, en.Deaths);
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Slot == SpellSlot.Q && _Menu.Get<CheckBox>("afterq").CurrentValue)
                {
                    Core.DelayAction(DoEmote, Random.Next(_Menu.Get<Slider>("delaybadge").CurrentValue, _Menu.Get<Slider>("delaybadge").CurrentValue + 500));
                }
                if (args.Slot == SpellSlot.W && _Menu.Get<CheckBox>("afterw").CurrentValue)
                {
                    Core.DelayAction(DoEmote, Random.Next(_Menu.Get<Slider>("delaybadge").CurrentValue, _Menu.Get<Slider>("delaybadge").CurrentValue + 500));
                }
                if (args.Slot == SpellSlot.E && _Menu.Get<CheckBox>("aftere").CurrentValue)
                {
                    Core.DelayAction(DoEmote, Random.Next(_Menu.Get<Slider>("delaybadge").CurrentValue, _Menu.Get<Slider>("delaybadge").CurrentValue + 500));
                }
                if (args.Slot == SpellSlot.R && _Menu.Get<CheckBox>("afterr").CurrentValue)
                {
                    Core.DelayAction(DoEmote, Random.Next(_Menu.Get<Slider>("delaybadge").CurrentValue, _Menu.Get<Slider>("delaybadge").CurrentValue + 500));
                }
                if (IgniteSlot != SpellSlot.Unknown && args.Slot == IgniteSlot && _Menu.Get<CheckBox>("afterignite").CurrentValue)
                {
                    Core.DelayAction(DoEmote, Random.Next(_Menu.Get<Slider>("delaybadge").CurrentValue, _Menu.Get<Slider>("delaybadge").CurrentValue + 500));
                }
                if (FlashSlot != SpellSlot.Unknown && args.Slot == FlashSlot && _Menu.Get<CheckBox>("afterflash").CurrentValue)
                {
                    Core.DelayAction(DoEmote, Random.Next(_Menu.Get<Slider>("delaybadge").CurrentValue, _Menu.Get<Slider>("delaybadge").CurrentValue + 500));
                }
            }
        }

        private static void OnTick(EventArgs args)
        {
            if (ObjectManager.Player.ChampionsKilled > MyKills && _Menu.Get<CheckBox>("onkill").CurrentValue)
            {
                MyKills = ObjectManager.Player.ChampionsKilled;
                Core.DelayAction(DoEmote, Random.Next(_Menu.Get<Slider>("delaybadge").CurrentValue, _Menu.Get<Slider>("delaybadge").CurrentValue + 500));
            }
            if (ObjectManager.Player.Assists > MyAssits && _Menu.Get<CheckBox>("onassist").CurrentValue)
            {
                MyAssits = ObjectManager.Player.Assists;
                Core.DelayAction(DoEmote, Random.Next(_Menu.Get<Slider>("delaybadge").CurrentValue, _Menu.Get<Slider>("delaybadge").CurrentValue + 500));
            }
            if (ObjectManager.Player.Deaths > MyDeaths && _Menu.Get<CheckBox>("ondeath").CurrentValue)
            {
                MyDeaths = ObjectManager.Player.Deaths;
                Core.DelayAction(DoEmote, Random.Next(_Menu.Get<Slider>("delaybadge").CurrentValue, _Menu.Get<Slider>("delaybadge").CurrentValue + 500));
            }
            if (_Menu.Get<CheckBox>("neardead").CurrentValue &&
                ObjectManager.Get<Obj_AI_Base>()
                    .Any(h => h.IsEnemy && h.IsVisible && h.IsDead && ObjectManager.Player.Distance(h) < 300))
            {
                Core.DelayAction(DoEmote, Random.Next(_Menu.Get<Slider>("delaybadge").CurrentValue, _Menu.Get<Slider>("delaybadge").CurrentValue + 500));
            }
            if ((_Menu.Get<CheckBox>("chatincb").CurrentValue && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) || (_Menu.Get<CheckBox>("chatinlc").CurrentValue && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) || (_Menu.Get<CheckBox>("chatinjc").CurrentValue && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) || (_Menu.Get<CheckBox>("chatinhr").CurrentValue && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) || (_Menu.Get<CheckBox>("chatinlh").CurrentValue && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))) return;
            switch (_Menu.Get<ComboBox>("chatmode").CurrentValue)
            {
                case 0:
                    break;
                case 1:
                    foreach (var en in EntityManager.Heroes.Enemies)
                    {
                        if (DeathsHistory.FirstOrDefault(record => record.Key == en.NetworkId).Value < en.Deaths)
                        {
                            var championName = en.ChampionName.ToLower();
                            DeathsHistory.Remove(en.NetworkId);
                            DeathsHistory.Add(en.NetworkId, en.Deaths);
                            if (en.Distance(ObjectManager.Player) < 1000)
                            {
                                Core.DelayAction(() => DoChatDisrespect(championName), Random.Next(_Menu.Get<Slider>("delaychat").CurrentValue, _Menu.Get<Slider>("delaychat").CurrentValue + 500));
                            }
                        }
                    }
                    break;
                case 2:
                    foreach (var en in EntityManager.Heroes.Enemies)
                    {
                        if (DeathsHistory.FirstOrDefault(record => record.Key == en.NetworkId).Value < en.Deaths)
                        {
                            var name = en.Name.ToLower();
                            DeathsHistory.Remove(en.NetworkId);
                            DeathsHistory.Add(en.NetworkId, en.Deaths);
                            if (en.Distance(ObjectManager.Player) < 1000)
                            {
                                Core.DelayAction(() => DoChatDisrespect(name), Random.Next(_Menu.Get<Slider>("delaychat").CurrentValue, _Menu.Get<Slider>("delaychat").CurrentValue + 500));
                            }
                        }
                    }
                    break;
                case 3:
                    foreach (var en in EntityManager.Heroes.Enemies)
                    {
                        if (DeathsHistory.FirstOrDefault(record => record.Key == en.NetworkId).Value < en.Deaths && _Menu.Get<ComboBox>("chatmode").CurrentValue == 3 && _Menu.Get<CheckBox>("usecustomchat").CurrentValue && File.ReadLines(FullFilePath).Count() > 0)
                        {
                            DeathsHistory.Remove(en.NetworkId);
                            DeathsHistory.Add(en.NetworkId, en.Deaths);
                            if (en.Distance(ObjectManager.Player) < 1000 && Environment.TickCount - LastEmoteSpam > Random.Next(5000, 15000))
                            {
                                LastEmoteSpam = Environment.TickCount;
                                Core.DelayAction(() => Chat.Say("/all " + RandomLine(new StreamReader(FullFilePath))), Random.Next(_Menu.Get<Slider>("delaychat").CurrentValue, _Menu.Get<Slider>("delaychat").CurrentValue + 500));
                            }
                        }
                    }
                    
                    break;
            }
        }

        public static string RandomLine(StreamReader reader)
        {
            string chosen = null;
            int numberSeen = 0;
            var rng = new Random();
            string line;
            while (( line = reader.ReadLine()) != null)
    {
                if (rng.Next(++numberSeen) == 0)
                {
                    chosen = line;
                }
            }
            return chosen;
        }

        public static void DoEmote()
        {
            if (_Menu.Get<ComboBox>("mode").CurrentValue == 0)
            {
                if (Environment.TickCount - LastEmoteSpam > Random.Next(5000, 15000))
                {
                    LastEmoteSpam = Environment.TickCount;
                    Player.DoEmote(Emote.Laugh);
                }
            }
            if (_Menu.Get<ComboBox>("mode").CurrentValue == 1) Chat.Say("/masterybadge", "/1");
            if (_Menu.Get<ComboBox>("mode").CurrentValue == 2)
            {
                if (Environment.TickCount - LastEmoteSpam > Random.Next(5000, 15000))
                {
                    LastEmoteSpam = Environment.TickCount;
                    Player.DoEmote(Emote.Laugh);
                }
                Chat.Say("/masterybadge", "/1");
            }
        }

        public static void DoChatDisrespect(string theTarget)
        {
            if (Environment.TickCount - LastChat > Random.Next(5000, 20000))
            {
                LastChat = Environment.TickCount;
                switch (Random.Next(0, 19))
                {
                    case 0:
                        Chat.Say(string.Format("/all HAHA {0} that was a refreshing experience!", theTarget));
                        break;
                    case 1:
                        Chat.Say(string.Format("/all LOL {0} no match for me!", theTarget));
                        break;
                    case 2:
                        Chat.Say(string.Format("/all Fantastic performance right there {0}!", theTarget));
                        break;
                    case 3:
                        Chat.Say(string.Format("/all Can't touch this {0}", theTarget));
                        break;
                    case 4:
                        Chat.Say(string.Format("/all {0}, you have been reformed!", theTarget));
                        break;
                    case 5:
                        Chat.Say(string.Format("/all Completely smashed there {0}", theTarget));
                        break;
                    case 6:
                        Chat.Say(string.Format("/all haha pathetic {0}", theTarget));
                        break;
                    case 7:
                        Chat.Say(string.Format("/all true display of skill {0}", theTarget));
                        break;
                    case 8:
                        Chat.Say(string.Format("/all better luck next time {0}", theTarget));
                        break;
                    case 9:
                        Chat.Say(string.Format("/all Nice try for a monkey {0}", theTarget));
                        break;
                    case 10:
                        Chat.Say(string.Format("/all I see you've set aside this special time to humiliate yourself in public {0}", theTarget));
                        break;
                    case 11:
                        Chat.Say(string.Format("/all Who lit the fuse on your tampon {0}?", theTarget));
                        break;
                    case 12:
                        Chat.Say(string.Format("/all I like you {0}. You remind me of myself when I was young and stupid. ", theTarget));
                        break;
                    case 13:
                        Chat.Say(string.Format("/all {0}, I'll try being nicer if you'll try being more intelligent.", theTarget));
                        break;
                    case 14:
                        Chat.Say(string.Format("/all {0}, if you have something to say raise your hand... then place it over your mouth. ", theTarget));
                        break;
                    case 15:
                        Chat.Say(string.Format("/all Somewhere out there is a tree, tirelessly producing oxygen so you can breathe. I think you owe it an apology, {0}", theTarget));
                        break;
                    case 16:
                        Chat.Say(string.Format("/all Easy peasy lemon squeezy", theTarget));
                        break;
                    case 17:
                        Chat.Say(string.Format("/all suck it up", theTarget));
                        break;
                    case 18:
                        Chat.Say("/all " + KnownDisrespectStarts[Random.Next(0, KnownDisrespectStarts.Length - 1)] +
                         (Random.Next(1, 2) == 1 ? theTarget : "") +
                         KnownDisrespectEndings[Random.Next(0, KnownDisrespectEndings.Length - 1)]);
                        break;
                    case 19:
                        if (_Menu.Get<CheckBox>("usecustomchat").CurrentValue && File.ReadLines(FullFilePath).Count() > 0 && (_Menu.Get<ComboBox>("chatmode").CurrentValue == 1 || _Menu.Get<ComboBox>("chatmode").CurrentValue == 2))
                        {
                            foreach (var line in File.ReadLines(FullFilePath))
                            {
                                Chat.Say("/all " + line, theTarget);
                                LastChat = Environment.TickCount;
                            }
                        }
                        return;
                }
                return;
            }

        }
    }
}
