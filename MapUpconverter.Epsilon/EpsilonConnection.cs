using Epsilon.DTO;
using Google.Protobuf;
using NetMQ;
using NetMQ.Sockets;

namespace MapUpconverter.Epsilon
{
    public static class EpsilonConnection
    {
        private static readonly PublisherSocket Socket = new();

        public static void Connect()
        {
            Socket.Connect("tcp://127.0.0.1:38415");
        }

        public static void RequestMapTileUpdate(int mapID, List<(int TileID, int UpdateFlags)> tiles)
        {
            var requestMapOp = new RequestMapTileOperation();
            requestMapOp.MapID = mapID;
            foreach (var tile in tiles)
            {
                requestMapOp.MapTileID.Add(tile.TileID);
                requestMapOp.MapTileOpFlags.Add(tile.UpdateFlags);
            }

            byte[] topic = [(byte)'R', (byte)'O', (byte)'M', (byte)'2', 0x00];

            // combine topic and message
            byte[] message = requestMapOp.ToByteArray();
            byte[] combined = new byte[topic.Length + message.Length];
            topic.CopyTo(combined, 0);
            message.CopyTo(combined, topic.Length);

            Socket.SendFrame(combined);
        }
    }
}
