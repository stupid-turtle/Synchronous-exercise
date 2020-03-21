using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewLayer {

    static string username = PlayerPrefs.GetString("username", "default");

    public static void CreateCharacter(Msg msg, GameObject playerObject) {
        GameObject player = GameObject.Instantiate(playerObject);
        player.name = msg.Username;
        player.tag = "Player";
        player.transform.position = new Vector3(msg.Posx, msg.Posy, msg.Posz);
        player.transform.localEulerAngles = new Vector3(msg.Rotx, msg.Roty, msg.Rotz);
    }

    public static void MovePosition() {
        foreach (int id in ModelLayer.nowAliveIndex) {
            string username = ModelLayer.playerName[id];
            GameObject nowGameObject = GameObject.Find(username);
            nowGameObject.transform.position = ModelLayer.playerPos[id];
        }
    }

    public static void PlayerAnimation() {
        Queue<int> deleteQueue = new Queue<int>();
        foreach (int x in ModelLayer.nowAliveIndex) {
            if (ModelLayer.IsDie[x] == true) {
                deleteQueue.Enqueue(x);
                continue;
            } else {
                GameObject nowGameObjcet = GameObject.Find(ModelLayer.playerName[x]);
                Animator nowAnimator = nowGameObjcet.GetComponent<Animator>();
                //Debug.Log(nowGameObjcet.name + "   " + ModelLayer.IsWalking[x]);
                nowAnimator.SetBool("IsSkill1", ModelLayer.IsSkill1[x]);
                nowAnimator.SetBool("IsWalking", ModelLayer.IsWalking[x]);
            }
        }
        int del = 0;
        while (deleteQueue.Count != 0) {
            int top = deleteQueue.Dequeue();
            GameObject nowGameObject = GameObject.Find(ModelLayer.playerName[top]);
            Object.Destroy(nowGameObject);
            ModelLayer.nowAliveIndex.Remove(top);
            del++;
        }
    }
    
    public static void LateUpdate(Transform transform) {
        Transform follow = GameObject.Find(username).transform;
        transform.forward = follow.forward;
        Vector3 forward = follow.forward.normalized;
        transform.position = new Vector3(follow.position.x - forward.x * 3f, follow.position.y + 2f, follow.position.z - forward.z * 3f);
        transform.LookAt(new Vector3(follow.position.x + forward.x * 10f, follow.position.y, follow.position.z + forward.z * 10f));
    }
}
