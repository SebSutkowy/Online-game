using System.Collections.Generic;

namespace Client
{
    internal class PlayerManager
    {
        public Dictionary<int, Player> Players { get; set; }

        public PlayerManager()
        {
            Players = new Dictionary<int, Player>();
        }

        public void Remove(int id)
        {
            Players.Remove(id);
        }

        public void UpdatePlayer(int id, StatePayload statePayload)
        {
            if (!Players.ContainsKey(id))
                Players.Add(id, new Player());
            Players[id].UpdatePlayer(statePayload);
        }

    }
}
