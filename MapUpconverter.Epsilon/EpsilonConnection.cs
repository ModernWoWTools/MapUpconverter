using Arbiter.Epsilon;
using Arbiter.CASC;
using Google.Protobuf;
using NetMQ;
using NetMQ.Sockets;
using System.Text;

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

            Socket.SendMultipartBytes(new List<byte[]> { Encoding.ASCII.GetBytes("ROM2"), requestMapOp.ToByteArray() });
        }

        public static void RequestCascReload()
        {
            var requestCascOp = new RequestCascReloadOperation();
            Socket.SendMultipartBytes(new List<byte[]> { Encoding.ASCII.GetBytes("RCR1"), requestCascOp.ToByteArray() });
        }
    }
}
