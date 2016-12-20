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
using Color1 = System.Drawing.Color;
using System.Drawing;

namespace JungleTimer
{
    class Program
    {
        public static Text txt;
        public static int Size;
        public static List<JungleCreep> JungleCreeps { get; set; }
        public static IEnumerable<JungleCreep> DeadJungleCreeps
        {
            get
            {
                return JungleCreeps.Where(x => x.Dead);
            }
        }

        private static AIHeroClient Player { get { return ObjectManager.Player; } }

        public static Menu RootMenu;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
//            EloBuddy.Hacks.RenderWatermark = false;
            
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Bootstrap.Init(null);
            LoadJungleCreeps();

            int smiteoff = AutoSmite.Init();

            RootMenu = MainMenu.AddMenu("JungleTimers", "jungletimers");
            RootMenu.AddGroupLabel("Timer Settings");
            RootMenu.Add("enabled", new CheckBox("Enabled", true));
            RootMenu.AddSeparator();
            RootMenu.Add("size", new Slider("Text Size", 6, 1, 20));

            RootMenu.AddSeparator();
            if (smiteoff == 0)
                AutoSmite.AddSmiteMenu(RootMenu);
            else
                Chat.Print("AutoSmite Unavailable");

            RootMenu.AddSeparator();
            RootMenu.AddLabel("Made by StormyZuse at EB.");

            Size = RootMenu["size"].Cast<Slider>().CurrentValue;
            txt = new Text("", new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, Size, System.Drawing.FontStyle.Bold));

//            Game.OnProcessPacket += Game_OnProcessPacket;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Drawing.OnEndScene += Drawing_OnEndScene;

            if(smiteoff == 0)
            {
                Game.OnUpdate += AutoSmite.Game_OnUpdate;
                Drawing.OnDraw += AutoSmite.Drawing_OnDraw;
            }
            //            Game.OnTick += Game_OnTick;
        }

        private static void Game_OnProcessPacket(GamePacketEventArgs args)
        {
            Console.WriteLine("OnProcessPacket called");
            Console.WriteLine(args.PacketData.ToString());
            Console.WriteLine("end");

            short header = BitConverter.ToInt16(args.PacketData, 0);
            int length = BitConverter.ToString(args.PacketData, 0).Length;
            int networkID = BitConverter.ToInt32(args.PacketData, 2);


            if (header == 0)
            {
                return;
            }

            //debug
            //foreach (var item in PossibleDragonList.ToList().Where(id => id == networkID))
            //{
            //    Console.WriteLine("Header: " + header + " lenght: " + length + " id: " + networkID);
            //}

            #region AutoFind Headers
/*
            if (_menu.Item("forcefindheaders").GetValue<bool>())
            {
                _menu.Item("headerOnPatienceChange" + GameVersion).SetValue<Slider>(new Slider(0, 0, 400));
                _menu.Item("headerOnAttack2" + GameVersion).SetValue<Slider>(new Slider(0, 0, 400));
                _menu.Item("headerOnMissileHit2" + GameVersion).SetValue<Slider>(new Slider(0, 0, 400));
                _menu.Item("headerOnDisengaged" + GameVersion).SetValue<Slider>(new Slider(0, 0, 400));
                _menu.Item("headerOnMonsterSkill" + GameVersion).SetValue<Slider>(new Slider(0, 0, 400));
                _menu.Item("headerOnCreateGromp" + GameVersion).SetValue<Slider>(new Slider(0, 0, 400));
                _menu.Item("headerOnCreateCampIcon" + GameVersion).SetValue<Slider>(new Slider(0, 0, 400));

                Packets.Patience.Header = 0;
                Packets.Attack.Header = 0;
                Packets.MissileHit.Header = 0;
                Packets.Disengaged.Header = 0;
                Packets.MonsterSkill.Header = 0;
                Packets.CreateGromp.Header = 0;
                Packets.CreateCampIcon.Header = 0;

                _menu.Item("forcefindheaders").SetValue<bool>(false);
            }

            if (_menu.Item("headerOnPatienceChange" + GameVersion).GetValue<Slider>().Value == 0 && length == Packets.Patience.Length && networkID > 0)
            {
                foreach (Obj_AI_Minion obj in ObjectManager.Get<Obj_AI_Minion>().Where(obj => obj.Team.ToString().Contains("Neutral") && obj.NetworkId == networkID))
                {
                    OnPatienceChangeList.Add(header);
                    if (OnPatienceChangeList.Count<int>(x => x == header) == 10)
                    {
                        _menu.Item("headerOnPatienceChange" + GameVersion).SetValue<Slider>(new Slider(header, 0, 400));
                        Packets.Patience.Header = header;
                        try
                        {
                            OnPatienceChangeList.Clear();
                        }
                        catch (Exception)
                        {
                            //ignored
                        }
                    }
                }
            }

            if (_menu.Item("headerOnAttack2" + GameVersion).GetValue<Slider>().Value == 0 && length == Packets.Attack.Length && networkID > 0)
            {
                foreach (Obj_AI_Minion obj in ObjectManager.Get<Obj_AI_Minion>().Where(obj => obj.NetworkId == networkID))
                {
                    OnAttackList.Add(header);
                    if (OnAttackList.Count<int>(x => x == header) == 10)
                    {
                        _menu.Item("headerOnAttack2" + GameVersion).SetValue<Slider>(new Slider(header, 0, 400));
                        Packets.Attack.Header = header;
                        try
                        {
                            OnAttackList.Clear();
                        }
                        catch (Exception)
                        {
                            //ignored
                        }
                    }
                }
            }

            if (_menu.Item("headerOnMissileHit2" + GameVersion).GetValue<Slider>().Value == 0 && length == Packets.MissileHit.Length && networkID > 0)
            {
                foreach (Obj_AI_Minion obj in ObjectManager.Get<Obj_AI_Minion>().Where(obj => obj.IsRanged && obj.NetworkId == networkID))
                {
                    MissileHitList.Add(header);
                    if (MissileHitList.Count<int>(x => x == header) == 10)
                    {
                        _menu.Item("headerOnMissileHit2" + GameVersion).SetValue<Slider>(new Slider(header, 0, 400));
                        Packets.MissileHit.Header = header;
                        try
                        {
                            MissileHitList.Clear();
                        }
                        catch (Exception)
                        {
                            //ignored
                        }
                    }
                }
            }

            if (_menu.Item("headerOnDisengaged" + GameVersion).GetValue<Slider>().Value == 0 && length == Packets.Disengaged.Length && networkID > 0)
            {
                foreach (Obj_AI_Minion obj in ObjectManager.Get<Obj_AI_Minion>().Where(obj => obj.Team.ToString().Contains("Neutral") && obj.NetworkId == networkID))
                {
                    _menu.Item("headerOnDisengaged" + GameVersion).SetValue<Slider>(new Slider(header, 0, 400));
                    Packets.Disengaged.Header = header;
                }
            }


            if (_menu.Item("headerOnMonsterSkill" + GameVersion).GetValue<Slider>().Value == 0 && length == Packets.MonsterSkill.Length && networkID > 0)
            {
                foreach (Obj_AI_Minion obj in ObjectManager.Get<Obj_AI_Minion>().Where(obj => obj.Name.Contains("Dragon") && obj.NetworkId == networkID))
                {
                    _menu.Item("headerOnMonsterSkill" + GameVersion).SetValue<Slider>(new Slider(header, 0, 400));
                    Packets.MonsterSkill.Header = header;
                }
            }

            if (_menu.Item("headerOnCreateGromp" + GameVersion).GetValue<Slider>().Value == 0 && (length == Packets.CreateGromp.Length || length == Packets.CreateGromp.Length2) && networkID > 0)
            {
                OnCreateGrompList.Add(new int[] { networkID, (int)header, length });
            }

            if (_menu.Item("headerOnCreateCampIcon" + GameVersion).GetValue<Slider>().Value == 0 && networkID == 0 &&
                (length == Packets.CreateCampIcon.Length || length == Packets.CreateCampIcon.Length2 || length == Packets.CreateCampIcon.Length3 || length == Packets.CreateCampIcon.Length4 || length == Packets.CreateCampIcon.Length5))
            {
                OnCreateCampIconList.Add(new int[] { (int)header, length });

                if ((OnCreateCampIconList.Count(item => item[0] == (int)header && item[1] == Packets.CreateCampIcon.Length) == 6) &&
                    (OnCreateCampIconList.Count(item => item[0] == (int)header && item[1] == Packets.CreateCampIcon.Length2) == 3) &&
                    (OnCreateCampIconList.Count(item => item[0] == (int)header && item[1] == Packets.CreateCampIcon.Length3) == 1) &&
                    (OnCreateCampIconList.Count(item => item[0] == (int)header && item[1] == Packets.CreateCampIcon.Length4) == 1) &&
                    (OnCreateCampIconList.Count(item => item[0] == (int)header && item[1] == Packets.CreateCampIcon.Length5) == 1))
                {
                    _menu.Item("headerOnCreateCampIcon" + GameVersion).SetValue<Slider>(new Slider(header, 0, 400));
                    Packets.CreateCampIcon.Header = header;
                    try
                    {
                        OnCreateCampIconList.Clear();
                    }
                    catch (Exception)
                    {
                        //ignored
                    }
                }
            }
*/
            #endregion

            #region Update States
/*
            bool isMob = false;

            foreach (var camp in Jungle.Camps.Where(camp => camp.MapType.ToString() == Game.MapId.ToString()))
            {
                //Do Stuff for each camp

                foreach (var mob in camp.Mobs.Where(mob => mob.NetworkId == networkID))
                {
                    //Do Stuff for each mob in a camp

                    isMob = true;

                    if (header == Packets.MonsterSkill.Header)
                    {
                        if (mob.Name.Contains("Crab"))
                        {
                            mob.State = 4;
                        }
                        else
                        {
                            mob.State = 1;
                        }
                        mob.LastChangeOnState = Environment.TickCount;
                    }

                    else if (header == Packets.Attack.Header)
                    {
                        mob.State = 1;
                        mob.LastChangeOnState = Environment.TickCount;
                    }

                    else if (header == Packets.Patience.Header && mob.State != 2)
                    {
                        if (!mob.Name.Contains("Dragon") && !mob.Name.Contains("Baron"))
                        {
                            mob.State = 1; // mob is aggro
                            mob.LastChangeOnState = Environment.TickCount;
                        }
                    }

                    else if (header == Packets.MissileHit.Header)
                    {
                        mob.State = 1;
                        mob.LastChangeOnState = Environment.TickCount;
                    }

                    else if (header == Packets.Disengaged.Header)
                    {
                        if (mob.Name.Contains("Crab"))
                        {
                            if (mob.State == 0) mob.State = 5;
                            else mob.State = 1;
                        }

                        if (!mob.Name.Contains("Crab") && !mob.Name.Contains("Spider"))
                        {
                            if (mob.State == 0) mob.State = 5;
                            else mob.State = 2;
                        }

                        mob.LastChangeOnState = Environment.TickCount;
                    }

                    if (mob.LastChangeOnState == Environment.TickCount && camp.Mobs.Count == 1)
                    {
                        camp.State = mob.State;
                        camp.LastChangeOnState = mob.LastChangeOnState;
                    }
                }
            }
*/
            #endregion

            #region Guess Dragon/Baron NetworkID
/*
            bool foundObj = false;

            foreach (var obj in ObjectManager.Get<GameObject>().ToList().Where(x => x.NetworkId == networkID))
            {
                foundObj = true;
            }

            //Find Baron NetworkID
            if (Game.MapId.ToString() == "SummonersRift" &&
                !isMob && !foundObj &&
                networkID != DragonCamp.Mobs[0].NetworkId &&
                networkID != BaronCamp.Mobs[0].NetworkId &&
                networkID > BiggestNetworkId
                )
            {
                if (Packets.MissileHit.Header == header && Packets.MissileHit.Length == length)
                {
                    PossibleBaronList.Add(new int[] { networkID, (int)header, length, Environment.TickCount });

                    if ((PossibleBaronList.Count(item => item[0] == networkID && item[1] == Packets.MonsterSkill.Header && item[2] == Packets.MonsterSkill.Length) >= 1) &&
                    (PossibleBaronList.Count(item => item[0] == networkID && item[1] == Packets.MonsterSkill.Header && item[2] == Packets.MonsterSkill.Length2) >= 1))
                    {
                        BaronCamp.Mobs[0].State = 1;
                        BaronCamp.Mobs[0].LastChangeOnState = Environment.TickCount;
                        BaronCamp.Mobs[0].NetworkId = networkID;
                    }

                }
                else if (Packets.MonsterSkill.Header == header && Packets.MonsterSkill.Length == length)
                {
                    PossibleBaronList.Add(new int[] { networkID, (int)header, length, Environment.TickCount });

                    if ((PossibleBaronList.Count(item => item[0] == networkID && item[1] == Packets.MissileHit.Header && item[2] == Packets.MissileHit.Length) >= 1) &&
                    (PossibleBaronList.Count(item => item[0] == networkID && item[1] == Packets.MonsterSkill.Header && item[2] == Packets.MonsterSkill.Length2) >= 1))
                    {
                        BaronCamp.Mobs[0].State = 1;
                        BaronCamp.Mobs[0].LastChangeOnState = Environment.TickCount;
                        BaronCamp.Mobs[0].NetworkId = networkID;
                    }
                }
                else if (Packets.MonsterSkill.Header == header && Packets.MonsterSkill.Length2 == length)
                {
                    PossibleBaronList.Add(new int[] { networkID, (int)header, length, Environment.TickCount });

                    if ((PossibleBaronList.Count(item => item[0] == networkID && item[1] == Packets.MissileHit.Header && item[2] == Packets.MissileHit.Length) >= 1) &&
                    (PossibleBaronList.Count(item => item[0] == networkID && item[1] == Packets.MonsterSkill.Header && item[2] == Packets.MonsterSkill.Length) >= 1))
                    {
                        BaronCamp.Mobs[0].State = 1;
                        BaronCamp.Mobs[0].LastChangeOnState = Environment.TickCount;
                        BaronCamp.Mobs[0].NetworkId = networkID;
                    }
                }
            }

            //Find Dragon NetworkID
            if (Environment.TickCount <= PossibleDragonTimer + 5000)
            {
                foreach (var id in PossibleDragonList.ToList().Where(id => id == networkID))
                {
                    try
                    {
                        PossibleDragonList.RemoveAll(x => x == networkID);
                    }
                    catch (Exception)
                    {
                        //ignored
                    }
                }
            }
            else
            {
                if (PossibleDragonList.Count() == 1)
                {
                    DragonCamp.Mobs[0].State = 1;
                    DragonCamp.Mobs[0].LastChangeOnState = Environment.TickCount;
                    DragonCamp.Mobs[0].NetworkId = PossibleDragonList[0];
                }
                try
                {
                    PossibleDragonList.Clear();
                }
                catch (Exception)
                {
                    //ignored
                }
            }



            if (header == Packets.MonsterSkill.Header &&
                Game.MapId.ToString() == "SummonersRift" &&
                !isMob && !foundObj &&
                networkID != DragonCamp.Mobs[0].NetworkId &&
                networkID != BaronCamp.Mobs[0].NetworkId &&
                networkID > BiggestNetworkId &&
                length == Packets.MonsterSkill.Length &&
                GuessDragonId == 1)
            {
//foreach (var obj in ObjectManager.Get<GameObject>().Where(x => x.NetworkId == networkID))
//                {
//                    Game.PrintChat("<font color=\"#FF0000\"> God Jungle Tracker (debug): Tell AlphaGod he forgot to consider: " + obj.Name + " - " + obj.SkinName + " - " + obj.CharData.BaseSkinName + " - Guess Dragon NetWorkID disabled</font>");
//                }
                if (!ObjectsList.Contains(networkID))
                {
                    PossibleDragonList.Add(networkID);
                    PossibleDragonTimer = Environment.TickCount;
                }
            }
*/
            #endregion  

            #region Gromp Created
/*
            if (header == Packets.CreateGromp.Header && Game.MapId.ToString() == "SummonersRift")  //Gromp Created
            {
                if (length == Packets.CreateGromp.Length)
                {
                    foreach (var camp in Jungle.Camps.Where(camp => camp.Name == "Gromp"))
                    {
                        foreach (var mob in camp.Mobs.Where(mob => mob.Name.Contains("SRU_Gromp13.1.1")))
                        {
                            mob.NetworkId = BitConverter.ToInt32(args.PacketData, 2);
                            mob.State = 3;
                            mob.LastChangeOnState = Environment.TickCount;
                            camp.State = mob.State;
                            camp.LastChangeOnState = mob.LastChangeOnState;
                        }
                    }

                    if (Game.ClockTime - 111f < 90 && ClockTimeAdjust == 0)
                    {
                        ClockTimeAdjust = Game.ClockTime - 111f;
                        DragonCamp.Mobs[0].State = 0;
                        DragonCamp.RespawnTime = Environment.TickCount + 39000;
                        DragonCamp.State = 0;
                        BiggestNetworkId = BitConverter.ToInt32(args.PacketData, 2);
                    }
                }
                else if (length == Packets.CreateGromp.Length2)
                {
                    foreach (var camp in Jungle.Camps.Where(camp => camp.Name == "Gromp"))
                    {
                        foreach (var mob in camp.Mobs.Where(mob => mob.Name.Contains("SRU_Gromp14.1.1")))
                        {
                            mob.NetworkId = BitConverter.ToInt32(args.PacketData, 2);
                            mob.State = 3;
                            mob.LastChangeOnState = Environment.TickCount;
                            camp.State = mob.State;
                            camp.LastChangeOnState = mob.LastChangeOnState;
                        }
                    }
                }
            }
*/
            #endregion


            #region ObjectsList
/*
            if (!ObjectsList.Contains(networkID) && (header != Packets.MonsterSkill.Header || length != Packets.MonsterSkill.Length))
            {
                ObjectsList.Add(networkID);
            }

*/
            #endregion
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (!RootMenu["enabled"].Cast<CheckBox>().CurrentValue)
                return;

            Size = RootMenu["size"].Cast<Slider>().CurrentValue;

            foreach (var camp in DeadJungleCreeps.Where(x => x.NextRespawnTime - Environment.TickCount > 0))
            {
                var timeSpan = TimeSpan.FromMilliseconds(camp.NextRespawnTime - Environment.TickCount);
                var text = timeSpan.ToString(@"m\:ss");

                txt.Position = new Vector2((int)camp.MinimapPosition.X - Size / 2, (int)camp.MinimapPosition.Y - Size / 2);
                txt.Color = Color1.White;
                txt.TextValue = text;
                txt.Draw();

            }
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {

            if (sender.Type != GameObjectType.obj_AI_Minion)
            {
                return;
            }

            var mob =
                JungleCreeps.FirstOrDefault(
                    x => x.MobNames.Select(y => y.ToLower()).Any(z => z.Equals(sender.Name.ToLower())));

            if (mob == null)
            {
                return;
            }

            mob.ObjectsDead.Add(sender.Name);
            mob.ObjectsAlive.Remove(sender.Name);

            if (mob.ObjectsDead.Count != mob.MobNames.Length)
            {
                return;
            }

            mob.Dead = true;
            mob.NextRespawnTime = Environment.TickCount + mob.RespawnTime - 3000;
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Type != GameObjectType.obj_AI_Minion)
            {
                return;
            }
/*
            if(!sender.Name.ToLower().StartsWith("minion"))
            {
                Console.WriteLine(sender.Name.ToLower() + " Team: " + sender.Team.ToString());
                
            }
*/                

            var mob =
                JungleCreeps.FirstOrDefault(
                    x => x.MobNames.Select(y => y.ToLower()).Any(z => z.Equals(sender.Name.ToLower())));

            if (mob == null)
            {
                return;
            }

            mob.ObjectsAlive.Add(sender.Name);
            mob.ObjectsDead.Remove(sender.Name);

            if (mob.ObjectsAlive.Count != mob.MobNames.Length)
            {
                return;
            }

            mob.Dead = false;
            mob.NextRespawnTime = 0;
        }
        private static void LoadJungleCreeps()
        {
            #region Init JungleCreep List
            JungleCreeps = new List<JungleCreep>
                              {
                                  new JungleCreep(
                                      75000,
                                      new Vector3(6078.15f, 6094.45f, -98.63f),
                                      new[] { "TT_NWolf3.1.1", "TT_NWolf23.1.2", "TT_NWolf23.1.3" },
                                      GameMapId.TwistedTreeline,
                                      GameObjectTeam.Order),
                                  new JungleCreep(
                                      150000,
                                      new Vector3(6943.41f, 5422.61f, 52.62f),
                                      new[] { "SRU_Razorbeak3.1.1", "SRU_RazorbeakMini3.1.2", "SRU_RazorbeakMini3.1.3",
                                          "SRU_RazorbeakMini3.1.4", "SRU_RazorbeakMini3.1.5", "SRU_RazorbeakMini3.1.6" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Order),
                                  new JungleCreep(
                                      150000,
                                      new Vector3(2164.34f, 8383.02f, 51.78f),
                                      new[] { "SRU_Gromp13.1.1" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Order),
                                  new JungleCreep(
                                      150000,
                                      new Vector3(8370.58f, 2718.15f, 51.09f),
                                      new[] { "SRU_Krug5.1.1", "SRU_KrugMini5.1.2"/*, "MiniKrugA", "MiniKrugB", 
                                          "MiniKrugA", "MiniKrugB", "MiniKrugA", "MiniKrugB", "MiniKrugA", "MiniKrugB"*/ },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Order),
                                  new JungleCreep(
                                      180000,
                                      new Vector3(4285.04f, 9597.52f, -67.6f),
                                      new[] { "SRU_Crab16.1.1" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Neutral),
                                  new JungleCreep(
                                      150000,
                                      new Vector3(6476.17f, 12142.51f, 56.48f),
                                      new[] { "SRU_Krug11.1.1", "SRU_KrugMini11.1.2"/*, "MiniKrugA", "MiniKrugB", "MiniKrugA",
                                          "MiniKrugB", "MiniKrugA", "MiniKrugB", "MiniKrugA", "MiniKrugB"*/ },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Chaos),
                                  new JungleCreep(
                                      75000,
                                      new Vector3(11025.95f, 5805.61f, -107.19f),
                                      new[] { "TT_NWraith4.1.1", "TT_NWraith24.1.2", "TT_NWraith24.1.3" },
                                      GameMapId.TwistedTreeline,
                                      GameObjectTeam.Chaos),
                                  new JungleCreep(
                                      150000,
                                      new Vector3(10983.83f, 8328.73f, 62.22f),
                                      new[] { "SRU_Murkwolf8.1.1", "SRU_MurkwolfMini8.1.2", "SRU_MurkwolfMini8.1.3" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Chaos),
                                  new JungleCreep(
                                      150000,
                                      new Vector3(12671.83f, 6306.6f, 51.71f),
                                      new[] { "SRU_Gromp14.1.1" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Chaos),
                                  new JungleCreep(
                                      360000,
                                      new Vector3(7738.3f, 10079.78f, -61.6f),
                                      new[] { "TT_Spiderboss8.1.1" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Neutral),
                                  new JungleCreep(
                                      300000,
                                      new Vector3(3800.99f, 7883.53f, 52.18f),
                                      new[] { "SRU_Blue1.1.1"/*, "SRU_BlueMini1.1.2", "SRU_BlueMini21.1.3" */},
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Order),
                                  new JungleCreep(
                                      75000,
                                      new Vector3(4373.14f, 5842.84f, -107.14f),
                                      new[] { "TT_NWraith1.1.1", "TT_NWraith21.1.2", "TT_NWraith21.1.3" },
                                      GameMapId.TwistedTreeline,
                                      GameObjectTeam.Order),
                                  new JungleCreep(
                                      300000,
                                      new Vector3(4993.14f, 10491.92f, -71.24f),
                                      new[] { "SRU_RiftHerald" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Neutral),
                                  new JungleCreep(
                                      75000,
                                      new Vector3(5106.94f, 7985.9f, -108.38f),
                                      new[] { "TT_NGolem2.1.1", "TT_NGolem22.1.2" },
                                      GameMapId.TwistedTreeline,
                                      GameObjectTeam.Order),
                                  new JungleCreep(
                                      150000,
                                      new Vector3(7852.38f, 9562.62f, 52.3f),
                                      new[] { "SRU_Razorbeak9.1.1", "SRU_RazorbeakMini9.1.2", "SRU_RazorbeakMini9.1.3",
                                          "SRU_RazorbeakMini9.1.4", "SRU_RazorbeakMini9.1.5", "SRU_RazorbeakMini9.1.6" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Chaos),
                                  new JungleCreep(
                                      300000,
                                      new Vector3(10984.11f, 6960.31f, 51.72f),
                                      new[] { "SRU_Blue7.1.1"/*, "SRU_BlueMini7.1.2", "SRU_BlueMini27.1.3" */},
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Chaos),
                                  new JungleCreep(
                                      180000,
                                      new Vector3(10647.7f, 5144.68f, -62.81f),
                                      new[] { "SRU_Crab15.1.1" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Neutral),
                                  new JungleCreep(
                                      75000,
                                      new Vector3(9294.02f, 6085.41f, -96.7f),
                                      new[] { "TT_NWolf6.1.1", "TT_NWolf26.1.2", "TT_NWolf26.1.3" },
                                      GameMapId.TwistedTreeline,
                                      GameObjectTeam.Chaos),
                                  new JungleCreep(
                                      420000,
                                      new Vector3(4993.14f, 10491.92f, -71.24f),
                                      new[] { "SRU_Baron12.1.1" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Neutral),
                                  new JungleCreep(
                                      150000,
                                      new Vector3(3849.95f, 6504.36f, 52.46f),
                                      new[] { "SRU_Murkwolf2.1.1", "SRU_MurkwolfMini2.1.2", "SRU_MurkwolfMini2.1.3" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Order),
                                  new JungleCreep(
                                      300000,
                                      new Vector3(7813.07f, 4051.33f, 53.81f),
                                      new[] { "SRU_Red4.1.1"/*, "SRU_RedMini4.1.2", "SRU_RedMini4.1.3" */},
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Order),
                                  new JungleCreep(
                                      360000,
                                      new Vector3(9813.83f, 4360.19f, -71.24f),
                                      new[] { "SRU_Dragon_Air6.1.1" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Neutral),
                                  new JungleCreep(
                                      360000,
                                      new Vector3(9813.83f, 4360.19f, -71.24f),
                                      new[] { "SRU_Dragon_Earth6.4.1" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Neutral),
                                  new JungleCreep(
                                      360000,
                                      new Vector3(9813.83f, 4360.19f, -71.24f),
                                      new[] { "SRU_Dragon_Fire6.2.1" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Neutral),
                                  new JungleCreep(
                                      360000,
                                      new Vector3(9813.83f, 4360.19f, -71.24f),
                                      new[] { "SRU_Dragon_Water6.3.1" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Neutral),
                                  new JungleCreep(
                                      360000,
                                      new Vector3(9813.83f, 4360.19f, -71.24f),
                                      new[] { "SRU_Dragon_Elder6.5.1" },
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Neutral),
                                  new JungleCreep(
                                      300000,
                                      new Vector3(7139.29f, 10779.34f, 56.38f),
                                      new[] { "SRU_Red10.1.1"/*, "SRU_RedMini10.1.2", "SRU_RedMini10.1.3" */},
                                      GameMapId.SummonersRift,
                                      GameObjectTeam.Chaos),
                                  new JungleCreep(
                                      75000,
                                      new Vector3(10276.81f, 8037.54f, -108.92f),
                                      new[] { "TT_NGolem5.1.1", "TT_NGolem25.1.2" },
                                      GameMapId.TwistedTreeline,
                                      GameObjectTeam.Chaos)
                              };
            #endregion
            JungleCreeps = JungleCreeps.Where(x => x.MapID == Game.MapId).ToList();
        }

        private static void Game_OnTick(EventArgs args)
        {
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    break;
                case Orbwalker.ActiveModes.Flee:
                    break;
                case Orbwalker.ActiveModes.Harass:
                    break;
                case Orbwalker.ActiveModes.JungleClear:
                    break;
                case Orbwalker.ActiveModes.LaneClear:
                    break;
                case Orbwalker.ActiveModes.LastHit:
                    break;
                case Orbwalker.ActiveModes.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

        }

        private static void Combo()
        {

        }

        private static void Flee()
        {

        }

        private static void Harass()
        {

        }

        private static void Farm()
        {

        }
    }
}
