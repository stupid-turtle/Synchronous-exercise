using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using ProtoBuf;

public class TransformController {
    public static byte[] Transform(Msg msg) {
        byte[] body = Serialize<Msg>(msg);
        byte[] head = IntToBytes(body.Length);
        int bodyLength = body.Length;
        int headLength = head.Length;
        int len = headLength + bodyLength;
        byte[] sendMsg = new byte[len];
        head.CopyTo(sendMsg, 0);
        body.CopyTo(sendMsg, headLength);
        return sendMsg;
    }

    public static byte[] IntToBytes(int src) {
        byte[] ret = new byte[4];
        ret[0] = (byte)(src & 0xff);
        ret[1] = (byte)((src >> 8) & 0xff);
        ret[2] = (byte)((src >> 16) & 0xff);
        ret[3] = (byte)((src >> 24) & 0xff);
        return ret;
    }

    public static int BytesToInt(byte[] src, int offset) {
        int ret = 0;
        ret |= (int)(src[offset] & 0xff);
        ret |= (int)((src[offset + 1] & 0xff) << 8);
        ret |= (int)((src[offset + 2] & 0xff) << 16);
        ret |= (int)((src[offset + 3] & 0xff) << 24);
        return ret;
    }

    public static byte[] StringToBytes(string src) {
        byte[] ret = System.Text.Encoding.Default.GetBytes(src);
        return ret;
    }

    public static string BytesToString(byte[] src) {
        string ret = System.Text.Encoding.Default.GetString(src);
        return ret;
    }

    public static byte[] Serialize<T>(T obj) where T : IMessage {
        return obj.ToByteArray();
    }

    public static T Deserialize<T>(byte[] data) where T : class, IMessage, new() {
        T obj = new T();
        IMessage message = obj.Descriptor.Parser.ParseFrom(data);
        return message as T;
    }
}
