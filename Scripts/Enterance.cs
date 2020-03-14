using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Enterance : MonoBehaviour{

    public GameObject playerObject;
    // Start is called before the first frame update
    List<Msg> recvMsg;
    bool flag = false;
    float startTime;
    void Start(){
        string username = PlayerPrefs.GetString("username", "default");
        recvMsg = new List<Msg>();
    }

    // Update is called once per frame
    void Update(){
        if (recvMsg.Count <= 2 && flag == false) {
            recvMsg = LoadController.LoadCharacter();
            for (int i = 0; i < recvMsg.Count; i++) {
                Debug.Log(recvMsg[i].Username);
                if (GameObject.Find(recvMsg[i].Username) == null) {
                    //Debug.Log(recvMsg[i].Username);
                    CreateController.CreateCharacter(recvMsg[i], playerObject);
                }
            }
        }
        Debug.Log(recvMsg.Count + "   " + flag);
        if (recvMsg.Count == 2 && flag == false) {
            startTime = Time.time + 3f;
            flag = true;
        }
        if (flag == true && Time.time >= startTime) MoveController.MovePosition();
        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < player.Length; i++) {
            Debug.Log(player[i].name + "   " + player[i].transform.position);
        }
    }
}
