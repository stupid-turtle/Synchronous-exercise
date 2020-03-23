using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.UI;

public class Enterance : MonoBehaviour{

    public GameObject playerObject;
    public Button Skill0Button;
    public Text Skill0CD;
    public Button Skill1Button;
    public Text Skill1CD;

    List<Msg> recvMsg;
    bool flag = false;
    float startTime;
    string username;
    Camera mainCamera;
    GameObject player;
    int peopleNum;

    void Start(){
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        username = PlayerPrefs.GetString("username", "default");
        player = GameObject.Find(username);
        recvMsg = new List<Msg>();
        peopleNum = 2;
    }

    // Update is called once per frame
    void Update(){
        ///////////////////  Load Player
        if (recvMsg.Count <= peopleNum && flag == false) {
            recvMsg = LoadController.LoadCharacter();
            for (int i = 0; i < recvMsg.Count; i++) {
                //Debug.Log(recvMsg[i].Username);
                if (GameObject.Find(recvMsg[i].Username) == null) {
                    //Debug.Log(recvMsg[i].Username);
                    ModelLayer.CreateCharacter(recvMsg[i]);
                    ViewLayer.CreateCharacter(recvMsg[i], playerObject);
                }
            }
        }
        //Debug.Log(recvMsg.Count + "   " + flag);
        if (recvMsg.Count == peopleNum && flag == false) {
            for (int i = 0; i < peopleNum; i++) {
                Debug.Log("index = " + i + ", name = " + ModelLayer.PlayerName[i] + ", id = " + ModelLayer.PlayerMap[ModelLayer.PlayerName[i]]);
            }
            startTime = Time.time + 3f;
            flag = true;
        }

        ///////////////////  Player Operation
        if (flag == true && Time.time >= startTime) {
            ModelLayer.RefreshMessage(username);
            ModelLayer.MovePosition(username);
            //Debug.Log("1    " + ModelLayer.msg.Optype);
            Skill0Button.onClick.AddListener(
                delegate {
                    ModelLayer.ReleaseSkill0(username);
                }
            );
            Skill1Button.onClick.AddListener(
                delegate {
                    ModelLayer.ReleaseSkill1(username);
                }
            );
            
            //Debug.Log("2    " +  ModelLayer.msg.Optype + "   " + ModelLayer.skillQueue.Count);
            ModelLayer.SendMessage();
            //Debug.Log("3    " + ModelLayer.msg.Optype);
            ModelLayer.ReceiveMessage();
            int userId = ModelLayer.PlayerMap[username];

            int diff = ModelLayer.SkillNextFrame[userId][0] - ModelLayer.clientFrame;
            if (diff >= 0) {
                Skill0CD.text = (diff / 20).ToString();
            } else {
                Skill0CD.text = "";
            }
            diff = ModelLayer.SkillNextFrame[userId][1] - ModelLayer.clientFrame;
            if (diff >= 0) {
                Skill1CD.text = (diff / 20).ToString();
            } else {
                Skill1CD.text = "";
            }


            ViewLayer.MovePosition();
            ViewLayer.PlayerAnimation();
        }
    }

    void LateUpdate() {
        ViewLayer.LateUpdate(mainCamera.transform);
    }
    
}
