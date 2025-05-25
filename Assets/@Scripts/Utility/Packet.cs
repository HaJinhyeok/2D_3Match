
using System;
using UnityEngine;

public enum PacketType : ushort
{
    PACKET_MATCH_REQUEST = 0,
    PACKET_MATCH_WAITING,
    PACKET_MATCH_COMPLETE,
    PACKET_MATCH_START,
    PACKET_MATCH_FINISH,
    PACKET_MATCH_RESULT,
    PACKET_MATCH_EXIT,

    PACKET_RESULT_WIN,
    PACKET_RESULT_LOSE,
    PACKET_RESULT_DRAW,

    PACKET_SWAP,
    PACKET_DESTROY,
    PACKET_GENERATE,
    PACKET_HIDE,

    PACKET_ERR_FULL,
    PACKET_ERR_DISCONNECTION,
}

public class PacketHeader
{
    public ushort size;
    public PacketType type;
}

public class PacketBuilder
{
    public static byte[] BuildPacketData(PacketType type, byte[] data = null)
    {
        ushort size = (ushort)(4 + (data?.Length ?? 0));
        byte[] buffer = new byte[size];
        Buffer.BlockCopy(BitConverter.GetBytes(size), 0, buffer, 0, 2);
        Buffer.BlockCopy(BitConverter.GetBytes((ushort)type), 0, buffer, 2, 2);
        if (data != null)
        {
            Buffer.BlockCopy(data, 0, buffer, 4, data.Length);
        }
        //if (type == PacketType.PACKET_GENERATE)
        //{
        //    Debug.Log($"Size: {size}, {BitConverter.ToString(data)}");
        //}
        return buffer;
    }
}
