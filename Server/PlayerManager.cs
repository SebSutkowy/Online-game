using System.Collections.Generic;

namespace Server
{
    static class PlayerManager
    {
        private static Dictionary<int, Player> Players = new Dictionary<int, Player>();

        public static void Remove(int id)
        {
            Players.Remove(id);
        }

        public static void CreatePlayer(int playerId)
        {
            Players.Add(playerId, new Player());
        }

        public static void UpdatePlayer(int id, StatePayload statePayload)
        {
            if (!Players.ContainsKey(id))
                Players.Add(id, new Player());
            Players[id].UpdatePlayer(statePayload);
        }

        public static void AddInput(int id,  InputPayload inputPayload)
        {
            if (!Players.ContainsKey(id))
                return;
            Players[id].InputQueue.Enqueue(inputPayload);
        }

        public static void ProcessPlayerMovement()
        {
            foreach (var (playerId, player) in Players)
            {
                int bufferIndex = -1;
                while(player.InputQueue.Count > 0)
                {
                    InputPayload input = player.InputQueue.Dequeue();

                    bufferIndex = input.Tick;

                    StatePayload state = player.ProcessMovement(input);
                    player.StateBuffer[bufferIndex] = state;
                }
                if (bufferIndex != -1)
                {
                    string state = player.StateBuffer[bufferIndex].ToString(playerId);
                    Server.SendGlobalMessage(state);
                }
            }
        }

    }
}
