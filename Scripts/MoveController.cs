using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class MoveController {
    public static void MovePosition() {
        Socket ClientSocket = GlobalController.instance.ClientSocket;
        float horizontal = 0;
        float vertical = 0;
        if (Input.GetKey(KeyCode.W) ) {  // Up
            horizontal += 1f;
        }
        if (Input.GetKey(KeyCode.S)) {  // Down
            horizontal -= 1f;
        }
        if (Input.GetKey(KeyCode.A)) {  // Left
            vertical -= 1f;
        }
        if (Input.GetKey(KeyCode.D)) {  // Right
            vertical += 1f;
        }
        Msg msg = new Msg {
            Optype = 3,
            Posz = horizontal,
            Posx = vertical
        };
        byte[] sendMsg = TransformController.Transform(msg);
        if (ClientSocket.Poll(-1, SelectMode.SelectWrite)) ClientSocket.Send(sendMsg);
        if (ClientSocket.Poll(-1, SelectMode.SelectRead)) {
            byte[] head = new byte[4];
            ClientSocket.Receive(head);
            int n = TransformController.BytesToInt(head, 0);
            for (int i = 0; i < n; i++) {
                if (ClientSocket.Poll(-1, SelectMode.SelectRead)) {
                    ClientSocket.Receive(head);
                    int bodyLength = TransformController.BytesToInt(head, 0);
                    byte[] body = new byte[bodyLength];
                    if (ClientSocket.Poll(-1, SelectMode.SelectRead)) {
                        ClientSocket.Receive(body);
                        Msg recv = TransformController.Deserialize<Msg>(body);
                        GameObject nowGameObject = GameObject.Find(recv.Username);
                        if (nowGameObject == null) continue;
                        Vector3 nowPos = nowGameObject.transform.position;
                        Vector3 trans = new Vector3(recv.Posx, recv.Posy, recv.Posz);
                        nowGameObject.transform.position = nowPos + trans;
                        //Debug.Log(nowPos + "   " + trans);
                    }
                }
            }
        }
    }
}
