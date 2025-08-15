using System.Collections.Generic;

namespace Server
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

        public void AddInput(int id,  InputPayload inputPayload)
        {
            if (!Players.ContainsKey(id))
                return;
            Players[id].InputQueue.Enqueue(inputPayload);
        }

        public void ProcessPlayerMovement(int id, InputPayload inputPayload)
        {
            if (!Players.ContainsKey(id))
                return;
            Players[id].ProcessMovement(inputPayload);
        }

    }
}
