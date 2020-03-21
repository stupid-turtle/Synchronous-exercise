using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class ModelLayer {
    
    public static List<string> playerName = new List<string>();                   // playerName[i]   i-th player's name
    public static List<Vector3> playerPos = new List<Vector3>();                   // playerPos[i]   i-th player's position
    public static List<Vector3> playerRot = new List<Vector3>();                   // playerRot[i]   i-th player's rotation
    public static List<int> nowHealthPoint = new List<int>();                 // nowHealthPoint[i]   i-th player's now healthpoint
    public static List<int> totHealthPotnt = new List<int>();                 // totHealthPoint[i]   i-th player's tot healthpoint
    public static List<float> playerRadius = new List<float>();                 // playerRadius[i]   i-th player's radius

    public static List<int> skillNextFrame = new List<int>();                 // skillNextFrame[i]   i-th player's skill next release frame
    public static List<int> skillCoolDown = new List<int>();                   // skillCoolDown[i]   i-th player's skill cd
    public static List<int> skillDamage = new List<int>();                       // skillDamage[i]   i-th player's skill damage
    public static List<int> skillTag = new List<int>();                             // skillTag[i]   i-th player's skill use or not
    public static List<List<int>> skillAttackFrame = new List<List<int>>(); // skillAttackFrame[i]   i-th player's skill attack frame
    public static List<int> skillTotFrame = new List<int>();                   // skillTotFrame[i]   i-th player's skill tot frame
    public static List<float> skillRadius = new List<float>();                   // skillRadius[i]   i-th player's skill radius
    public static List<int> skillAreaType = new List<int>();                   // skillAreaType[i]   i-th player's skill area type
    public static List<int> skillAnimation = new List<int>();                 // skillAnimation[i]   i-th player's skill animation state

    public static List<bool> IsDie = new List<bool>();                                 // IsDie[i]   i-th player die or not
    public static List<bool> IsWalking = new List<bool>();                         // IsWalking[i]   i-th player is walking or not
    public static List<bool> IsSkill1 = new List<bool>();                           // IsSkill1[i]   i-th player is using skill1 or not
    public static List<int> nowAliveIndex = new List<int>();                   // nowAliveIndex[i]   the set of the index of now alive player
    
    public static Queue<int> skillQueue = new Queue<int>();                          // player release skill queue
    public static Dictionary<string, int> playerMap = new Dictionary<string, int>(); // player name  ->  player id

    public static List<Skill> allSkillRelease = new List<Skill>();                         // all skill frame
    
    public class Skill {
        public int releasePlayerIndex;
        public int attackFrame;
        public Vector3 releasePosition;
        public int areaType;
        public float radius;
        public int damage;
        public Skill(int _releasePlayerIndex, int _attackFrame, Vector3 _releasePosition, int _areaType, float _radius, int _damage) {
            releasePlayerIndex = _releasePlayerIndex;
            attackFrame = _attackFrame;
            releasePosition = _releasePosition;
            areaType = _areaType;
            radius = _radius;
            damage = _damage;
        }
    }

    class Player {
        string name;
        Vector3 position;
        Vector3 rotation;
        int nowHealthPoint;
        int totHealth;
    }


    public static Msg msg = new Msg();
    static Socket ClientSocket = GlobalController.instance.ClientSocket;

    public static int clientFrame;
    

    public static void CreateCharacter(Msg recvMsg) {
        nowAliveIndex.Add(playerName.Count);
        playerMap.Add(recvMsg.Username, playerName.Count);
        playerName.Add(recvMsg.Username);
        playerPos.Add(new Vector3(recvMsg.Posx, recvMsg.Posy, recvMsg.Posz));
        playerRot.Add(new Vector3(recvMsg.Rotx, recvMsg.Roty, recvMsg.Rotz));
        playerRadius.Add(0.5f);

        skillNextFrame.Add(0);
        skillCoolDown.Add(100);
        skillDamage.Add(10);
        skillTag.Add(0);
        skillTotFrame.Add(30);
        List<int> tmp = new List<int> {
            8
        };
        skillAttackFrame.Add(tmp);
        skillRadius.Add(2f);
        skillAreaType.Add(1);
        skillAnimation.Add(0);

        IsDie.Add(false);
        IsWalking.Add(false);
        IsSkill1.Add(false);
        nowHealthPoint.Add(100);
        totHealthPotnt.Add(100);
    }
    
    public static void RefreshMessage() {
        msg.Optype = 0;
        skillTag[0] = 0;
    }

    public static void MovePosition(string username) {
        float horizontal = ETCInput.GetAxis("Horizontal");
        float vertical = -ETCInput.GetAxis("Vertical");
        GameObject gameObject = GameObject.Find(username);
        float degree = -90 - gameObject.transform.localEulerAngles.y;
        if (degree < 0) degree += 360;
        degree = degree * Mathf.PI / 180;
        //Debug.Log(gameObject.name + "   " + gameObject.transform.localEulerAngles.y + "   " + degree);
        /*if (Input.GetKey(KeyCode.W)) {  // Up
            horizontal += forward.z;
            vertical += forward.x;
        }
        if (Input.GetKey(KeyCode.S)) {  // Down
            horizontal -= forward.z;
            vertical -= forward.x;
        }
        if (Input.GetKey(KeyCode.A)) {  // Left
            horizontal += forward.x;
            vertical -= forward.z;
        }
        if (Input.GetKey(KeyCode.D)) {  // Right
            horizontal -= forward.x;
            vertical += forward.z;
        }*/
        if((msg.Optype & (1 << 3)) == 0) msg.Optype += (1 << 3);
        
        msg.Posz = vertical * Mathf.Sin(degree) + horizontal * Mathf.Cos(degree);
        msg.Posx = vertical * Mathf.Cos(degree) - horizontal * Mathf.Sin(degree);
        //Debug.Log(msg.Posz + "   " + msg.Posx);
    }
    
    public static void ReleaseSkill1() {
        if (skillTag[0] == 0) {
            //Debug.Log("skill = YES");
            skillQueue.Enqueue(1 << 4);
            skillTag[0] = 1;
        }
    }

    public static void SendMessage() {
        //Debug.Log(msg.Optype + "   " + msg.Posx + "   " + msg.Posy + "   " + msg.Posz);
        //Debug.Log(msg.Optype);
        while (skillQueue.Count != 0) {
            int skill = skillQueue.Dequeue();
            //Debug.Log("skill = " + skill);
            msg.Optype += skill;
        }
        byte[] sendMsg = TransformController.Transform(msg);
        if (ClientSocket.Poll(-1, SelectMode.SelectWrite)) ClientSocket.Send(sendMsg);
        //if ((msg.Optype & (1 << 5)) == 0) msg.Optype += (1 << 5);
    }

    public static void ReceiveMessage() {
        if (ClientSocket.Poll(-1, SelectMode.SelectRead)) {
            byte[] head = new byte[4];
            ClientSocket.Receive(head);
            int n = TransformController.BytesToInt(head, 0);
            for (int i = 0; i < n; i++) {
                if (ClientSocket.Poll(-1, SelectMode.SelectRead)) {
                    ClientSocket.Receive(head);
                    //for (int j = 0; j < 4; j++) Debug.Log(j + "   " + head[j]);
                    int bodyLength = TransformController.BytesToInt(head, 0);
                    byte[] body = new byte[bodyLength];
                    if (ClientSocket.Poll(-1, SelectMode.SelectRead)) {
                        ClientSocket.Receive(body);
                        Msg recv = TransformController.Deserialize<Msg>(body);
                        clientFrame = recv.Frame;

                        //Debug.Log("Optype = " + recv.Optype);
                        //Debug.Log("Username = " + recv.Username);
                        //Debug.Log("clientFrame = " + clientFrame);

                        int playerId = playerMap[recv.Username];
                        //Debug.Log("username = " + recv.Username);
                        //Debug.Log("playerid = " + playerId);
                        //Debug.Log("playerid hearusername = " + playerName[playerId]);
                        //Debug.Log("username hereplayerid = " + playerMap[recv.Username]);
                        if ((recv.Optype & (1 << 3)) != 0) {  // Move
                            Vector3 move = new Vector3(recv.Posx, recv.Posy, recv.Posz);

                            playerPos[playerId] += move;
                            //Debug.Log(recv.Username + "   " + move);
                            //Vector3 trans = new Vector3(forward.x, forward.y, forward.z);
                            //Debug.Log(nowPos + "   " + trans);
                            //IsWalking[playerId] = (recv.Posx != 0) || (recv.Posz != 0);
                            IsWalking[playerId] = !Mathf.Approximately(recv.Posx, 0f) || !Mathf.Approximately(recv.Posz, 0f);
                            //Debug.Log(recv.Posx + "   " + recv.Posz + (!Mathf.Approximately(recv.Posx, 0f) || !Mathf.Approximately(recv.Posz, 0f)));
                        }
                        if (clientFrame >= skillNextFrame[playerId]) {  // Skill
                            //Debug.Log(recv.Username);
                            if ((recv.Optype & (1 << 4)) != 0) {
                                List<int> skillFrame = skillAttackFrame[playerId];
                                foreach (int x in skillFrame) {
                                    Skill newSkill = new Skill(playerId, clientFrame + x - 1, playerPos[playerId], skillAreaType[playerId], skillRadius[playerId], skillDamage[playerId]);
                                    allSkillRelease.Add(newSkill);
                                }
                                /*for (int j = 0; j < playerName.Count; j++) {
                                    int nowPlayerId = playerMap[playerName[j]];
                                    if (nowPlayerId != playerId && IsDie[nowPlayerId] == false) {
                                        nowHealthPoint[nowPlayerId] -= skillDamage[nowPlayerId];
                                        if (nowHealthPoint[nowPlayerId] < 0) {
                                            nowHealthPoint[nowPlayerId] = 0;
                                            IsDie[nowPlayerId] = true;
                                        }
                                        //Debug.Log(playerId + "   " + nowHealthPoint[playerId]);
                                    }
                                }*/
                                skillNextFrame[playerId] = clientFrame + skillCoolDown[playerId] - 1;
                                skillAnimation[playerId] = clientFrame + skillTotFrame[playerId] - 1;
                                IsSkill1[playerId] = true;
                            }
                        }
                        if (clientFrame < skillAnimation[playerId]) {
                            IsSkill1[playerId] = true;
                        } else {
                            IsSkill1[playerId] = false;
                        }
                        
                        for (int j = allSkillRelease.Count - 1; j >= 0; j--) {
                            Skill nowSkill = allSkillRelease[j];
                            if (nowSkill.attackFrame == clientFrame) {
                                for (int k = 0; k < playerName.Count; k++) {
                                    int nowPlayerId = playerMap[playerName[k]];
                                    //Debug.Log("nowPlayerId = " + nowPlayerId + ", nowplayerName = " + playerName[nowPlayerId]);
                                    //Debug.Log("playerId = " + playerId + ", playerName = " + recv.Username);
                                    //Debug.Log("releasePosition = " + nowSkill.releasePosition + ", playerPosition = " + playerPos[nowPlayerId]);
                                    if (nowPlayerId != nowSkill.releasePlayerIndex && IsDie[nowPlayerId] == false && (nowSkill.releasePosition - playerPos[nowPlayerId]).sqrMagnitude <= nowSkill.radius) {
                                        //Debug.Log("dddddd");
                                        nowHealthPoint[nowPlayerId] -= nowSkill.damage;
                                        if (nowHealthPoint[nowPlayerId] < 0) {
                                            nowHealthPoint[nowPlayerId] = 0;
                                            IsDie[nowPlayerId] = true;
                                        }
                                    }
                                }
                                allSkillRelease.Remove(allSkillRelease[j]);
                            }
                        }
                    }
                }
            }
        }
    }
}
