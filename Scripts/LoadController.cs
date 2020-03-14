using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

public class LoadController {
    readonly static Socket ClientSocket = GlobalController.instance.ClientSocket;
    
    public static List<Msg> LoadCharacter() {
        Msg msg = new Msg {
            Optype = 2
        };
        List<Msg> recvMsg = new List<Msg>();
        byte[] sendMsg = TransformController.Transform(msg);
        if (ClientSocket.Poll(-1, SelectMode.SelectWrite)) {
            ClientSocket.Send(sendMsg);
            byte[] sz = new byte[4];
            if (ClientSocket.Poll(-1, SelectMode.SelectRead)) {
                ClientSocket.Receive(sz);
                int n = TransformController.BytesToInt(sz, 0);
                //Debug.Log("n = " + n);
                for (int i = 0; i < n; i++) {
                    byte[] head = new byte[4];
                    if (ClientSocket.Poll(-1, SelectMode.SelectRead)) {
                        ClientSocket.Receive(head);
                        //Debug.Log(head[0] + " " + head[1] + " " + head[2] + " " + head[3]);
                        int bodyLength = TransformController.BytesToInt(head, 0);
                        //Debug.Log("should   " + bodyLength);
                        byte[] body = new byte[bodyLength];
                        if (ClientSocket.Poll(-1, SelectMode.SelectRead)) {
                            ClientSocket.Receive(body);
                            //Debug.Log("real   " + body.Length);
                            Msg nowMsg = TransformController.Deserialize<Msg>(body);
                            recvMsg.Add(nowMsg);
                            //Debug.Log(nowMsg.Username);
                        } else {
                            //Debug.Log("cc");
                        }
                    }
                    else {
                        //Debug.Log("bb");
                    }
                }
            }
            else {
                //Debug.Log("aa");
            }
        }
        //Debug.Log("!!!!!!!!!!");
        return recvMsg;
    }
}
