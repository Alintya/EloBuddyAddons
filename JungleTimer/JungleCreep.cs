using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using SharpDX;

namespace JungleTimer
{
    class JungleCreep
    {
        public bool Dead { get; set; }
        public GameMapId MapID { get; set; }
        public Vector2 MinimapPosition { get { return Drawing.WorldToMinimap(this.Position); } }
        public string[] MobNames { get; set; }
        public int NextRespawnTime { get; set; }
        public List<string> ObjectsAlive { get; set; }
        public List<string> ObjectsDead { get; set; }
        public Vector3 Position { get; set; }
        public int RespawnTime { get; set; }
        public GameObjectTeam Team { get; set; }
        public JungleCreep(
                int respawnTime,    //msec
                Vector3 position,
                string[] mobNames,
                GameMapId mapID,
                GameObjectTeam team)
        {
            RespawnTime = respawnTime;
            Position = position;
            MobNames = mobNames;
            MapID = mapID;
            Team = team;

            ObjectsDead = new List<string>();
            ObjectsAlive = new List<string>();
        }
    }
}
