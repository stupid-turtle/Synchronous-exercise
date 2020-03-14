using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowController : MonoBehaviour {

    Transform follow;
    string username;
    // Start is called before the first frame update
    void Start() {
        username = PlayerPrefs.GetString("username", "default");
        //Debug.Log(username);
    }

    // Update is called once per frame
    void Update() {
        
    }

    void LateUpdate() {
        follow = GameObject.Find(username).transform;
        transform.position = new Vector3(follow.position.x, follow.position.y + 8f, follow.position.z - 10f);
        transform.LookAt(follow);
    }
}
