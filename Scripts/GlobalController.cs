using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class GlobalController : MonoBehaviour {
    // Start is called before the first frame update

    public static GlobalController instance;
    
    public Socket ClientSocket;  // Client communication socket

    private void Awake() {
        if (instance == null) {
            DontDestroyOnLoad(gameObject);
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit() {
        ClientSocket.Close();
    }
}
