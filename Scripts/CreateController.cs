using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class CreateController {
    public static void CreateCharacter(Msg msg, GameObject playerObject) {
        GameObject player = GameObject.Instantiate(playerObject);
        player.name = msg.Username;
        player.tag = "Player";
        player.transform.position = new Vector3(msg.Posx, msg.Posy, msg.Posz);
        player.transform.localEulerAngles = new Vector3(msg.Rotx, msg.Roty, msg.Rotz);
    }
}
