using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class ModelLayer {
    
    public static List<string> PlayerName = new List<string>();                   // playerName[i]   i-th player's name
    public static List<Vector3> PlayerPos = new List<Vector3>();                   // playerPos[i]   i-th player's position
    public static List<Vector3> PlayerRot = new List<Vector3>();                   // playerRot[i]   i-th player's rotation
    public static List<int> NowHealthPoint = new List<int>();                 // nowHealthPoint[i]   i-th player's now healthpoint
    public static List<int> TotHealthPotnt = new List<int>();                 // totHealthPoint[i]   i-th player's tot healthpoint
    public static List<float> PlayerRadius = new List<float>();                 // playerRadius[i]   i-th player's radius

    public static List<List<int>> SkillNextFrame = new List<List<int>>();     // skillNextFrame[i]   i-th player's skill next release frame
    public static List<List<int>> SkillTag = new List<List<int>>();                 // skillTag[i]   i-th player's skill use or not
    public static List<List<int>> SkillAnimation = new List<List<int>>();     // skillAnimation[i]   i-th player's skill animation state

    public static List<bool> IsDie = new List<bool>();                                 // IsDie[i]   i-th player die or not
    public static List<bool> IsWalking = new List<bool>();                         // IsWalking[i]   i-th player is walking or not
    public static List<List<bool>> IsSkill = new List<List<bool>>();                 // IsSkill[i]   i-th player is using skill[j] or not
    public static List<int> NowAliveIndex = new List<int>();                   // nowAliveIndex[i]   the set of the index of now alive player
    
    public static Queue<int> SkillQueue = new Queue<int>();                          // player release skill queue
    public static Dictionary<string, int> PlayerMap = new Dictionary<string, int>(); // player name  ->  player id

    public static List<Skill> AllSkillRelease = new List<Skill>();                         // all skill frame
    
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

    public static SkillConfig skills = Resources.Load("SkillConfig/Warrior") as SkillConfig;

    public static void CreateCharacter(Msg recvMsg) {
        int playerId = PlayerName.Count;
        NowAliveIndex.Add(playerId);
        PlayerMap.Add(recvMsg.Username, playerId);
        PlayerName.Add(recvMsg.Username);
        PlayerPos.Add(new Vector3(recvMsg.Posx, recvMsg.Posy, recvMsg.Posz));
        PlayerRot.Add(new Vector3(recvMsg.Rotx, recvMsg.Roty, recvMsg.Rotz));
        PlayerRadius.Add(0.5f);

        List<int> skillNextFrameTmp = new List<int>();
        SkillNextFrame.Add(skillNextFrameTmp);
        List<int> skillTagTmp = new List<int>();
        SkillTag.Add(skillTagTmp);
        List<int> skillAnimationTmp = new List<int>();
        SkillAnimation.Add(skillAnimationTmp);
        List<bool> IsSkillTmp = new List<bool>();
        IsSkill.Add(IsSkillTmp);

        for (int i = 0; i < skills.skill_config_list.Count; i++) {
            SkillNextFrame[playerId].Add(0);
            SkillTag[playerId].Add(0);
            SkillAnimation[playerId].Add(0);
            IsSkill[playerId].Add(false);
        }

        IsDie.Add(false);
        IsWalking.Add(false);
        NowHealthPoint.Add(100);
        TotHealthPotnt.Add(100);
    }
    
    public static void RefreshMessage(string nowPlayer) {
        msg.Optype = 0;
        int nowPlayerId = PlayerMap[nowPlayer];
        for (int i = 0; i < skills.skill_config_list.Count; i++) {
            SkillTag[nowPlayerId][i] = 0;
        }
    }

    public static void MovePosition(string username) {
        float horizontal = ETCInput.GetAxis("Horizontal");
        float vertical = -ETCInput.GetAxis("Vertical");
        GameObject gameObject = GameObject.Find(username);
        float degree = -90 - gameObject.transform.localEulerAngles.y;
        if (degree < 0) degree += 360;
        degree = degree * Mathf.PI / 180;

        if((msg.Optype & (1 << 3)) == 0) msg.Optype += (1 << 3);
        
        msg.Posz = vertical * Mathf.Sin(degree) + horizontal * Mathf.Cos(degree);
        msg.Posx = vertical * Mathf.Cos(degree) - horizontal * Mathf.Sin(degree);
        //Debug.Log(msg.Posz + "   " + msg.Posx);
    }
    
    public static void ReleaseSkill0(string nowPlayer) {
        int nowPlayerId = PlayerMap[nowPlayer];
        if (SkillTag[nowPlayerId][0] == 0) {
            //Debug.Log("skill = YES");
            SkillQueue.Enqueue(1 << 4);
            //msg.Optype += (1 << 4);
            SkillTag[nowPlayerId][0] = 1;
        }
    }

    public static void ReleaseSkill1(string nowPlayer) {
        int nowPlayerId = PlayerMap[nowPlayer];
        if (SkillTag[nowPlayerId][1] == 0) {
            //Debug.Log("skill = YES");
            SkillQueue.Enqueue(1 << 5);
            //msg.Optype += (1 << 5);
            SkillTag[nowPlayerId][1] = 1;
        }
    }

    public static void SendMessage() {
        //Debug.Log(msg.Optype + "   " + msg.Posx + "   " + msg.Posy + "   " + msg.Posz);
        //Debug.Log(msg.Optype);
        while (SkillQueue.Count != 0) {
            int skill = SkillQueue.Dequeue();
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

                        int playerId = PlayerMap[recv.Username];
                        //Debug.Log("username = " + recv.Username);
                        //Debug.Log("playerid = " + playerId);
                        //Debug.Log("playerid hearusername = " + playerName[playerId]);
                        //Debug.Log("username hereplayerid = " + playerMap[recv.Username]);
                        if ((recv.Optype & (1 << 3)) != 0) {  // Move
                            Vector3 move = new Vector3(recv.Posx, recv.Posy, recv.Posz);

                            PlayerPos[playerId] += move;
                            //Debug.Log(recv.Username + "   " + move);
                            //Vector3 trans = new Vector3(forward.x, forward.y, forward.z);
                            //Debug.Log(nowPos + "   " + trans);
                            //IsWalking[playerId] = (recv.Posx != 0) || (recv.Posz != 0);
                            IsWalking[playerId] = !Mathf.Approximately(recv.Posx, 0f) || !Mathf.Approximately(recv.Posz, 0f);
                            //Debug.Log(recv.Posx + "   " + recv.Posz + (!Mathf.Approximately(recv.Posx, 0f) || !Mathf.Approximately(recv.Posz, 0f)));
                        }


                        for (int j = 0; j < skills.skill_config_list.Count; j++) {
                            if (clientFrame < SkillAnimation[playerId][j]) {
                                IsSkill[playerId][j] = true;
                            } else {
                                IsSkill[playerId][j] = false;
                            }
                        }
                        bool nowState = false;
                        for (int j = 0; j < 2; j++) {
                            nowState |= IsSkill[playerId][j];
                        }

                        if (nowState == false) {
                            if (clientFrame >= SkillNextFrame[playerId][0]) {  // Skill0
                                //Debug.Log(recv.Username);
                                Skill_data skill0 = skills.skill_config_list[0];
                                if ((recv.Optype & (1 << 4)) != 0) {
                                    //List<int> skillFrame = skillAttackFrame[playerId];
                                    List<int> skillFrame = skill0.skill_attack_frame;
                                    foreach (int x in skillFrame) {
                                        //Skill newSkill = new Skill(playerId, clientFrame + x - 1, playerPos[playerId], skillAreaType[playerId], skillRadius[playerId], skillDamage[playerId]);
                                        Skill newSkill = new Skill(playerId, clientFrame + x - 1, PlayerPos[playerId], skill0.skill_area_type, skill0.skill_radius, skill0.skill_damage);
                                        AllSkillRelease.Add(newSkill);
                                    }
                                    //skillNextFrame[playerId] = clientFrame + skillCoolDown[playerId] - 1;
                                    SkillNextFrame[playerId][0] = clientFrame + skill0.skill_cool_down - 1;
                                    //skillAnimation[playerId] = clientFrame + skillTotFrame[playerId] - 1;
                                    SkillAnimation[playerId][0] = clientFrame + skill0.skill_tot_frame - 1;
                                    IsSkill[playerId][0] = true;
                                }
                            }
                            if (clientFrame >= SkillNextFrame[playerId][1]) { // Skill1
                                Skill_data skill1 = skills.skill_config_list[1];
                                if ((recv.Optype & (1 << 5)) != 0) {
                                    List<int> skillFrame = skill1.skill_attack_frame;
                                    /*foreach (int x in skillFrame) {
                                        Skill newSkill = new Skill(playerId, clientFrame + x - 1, PlayerPos[playerId], skill1.skill_area_type, skill1.skill_radius, skill1.skill_damage);
                                        AllSkillRelease.Add(newSkill);
                                    }*/
                                    SkillNextFrame[playerId][1] = clientFrame + skill1.skill_cool_down - 1;
                                    SkillAnimation[playerId][1] = clientFrame + skill1.skill_tot_frame - 1;
                                    IsSkill[playerId][1] = true;
                                }
                            }

                            for (int j = AllSkillRelease.Count - 1; j >= 0; j--) {
                                Skill nowSkill = AllSkillRelease[j];
                                if (nowSkill.attackFrame == clientFrame) {
                                    for (int k = 0; k < PlayerName.Count; k++) {
                                        int nowPlayerId = PlayerMap[PlayerName[k]];
                                        //Debug.Log("nowPlayerId = " + nowPlayerId + ", nowplayerName = " + playerName[nowPlayerId]);
                                        //Debug.Log("playerId = " + playerId + ", playerName = " + recv.Username);
                                        //Debug.Log("releasePosition = " + nowSkill.releasePosition + ", playerPosition = " + playerPos[nowPlayerId]);
                                        if (nowPlayerId != nowSkill.releasePlayerIndex && IsDie[nowPlayerId] == false) {
                                            //Debug.Log("dddddd");
                                            bool releaseTag = false;
                                            if (nowSkill.areaType == 1 && (nowSkill.releasePosition - PlayerPos[nowPlayerId]).sqrMagnitude <= nowSkill.radius) releaseTag = true;
                                            if (releaseTag == true) {
                                                NowHealthPoint[nowPlayerId] -= nowSkill.damage;
                                                if (NowHealthPoint[nowPlayerId] < 0) {
                                                    NowHealthPoint[nowPlayerId] = 0;
                                                    IsDie[nowPlayerId] = true;
                                                }
                                            }
                                        }
                                    }
                                    AllSkillRelease.Remove(AllSkillRelease[j]);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
