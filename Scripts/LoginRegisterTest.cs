using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using System.Text;
using System;
using Google.Protobuf;
using UnityEngine.SceneManagement;

public class LoginRegisterTest : MonoBehaviour {

    const int BUFFSIZE = 128;

    public InputField user_name;
    public InputField user_pwd;
    private Button LoginButton;
    Msg msg = new Msg();
    public Socket ClientSocket;
    public IPEndPoint ServerEndPoint;

    public void OnLoginClick() {

        string username = user_name.text;
        string userpwd = user_pwd.text;

        msg.Optype = (1 << 0);
        msg.Username = username;
        msg.Userpwd = userpwd;
        
        //Controller.SetIPEndPoint(new IPEndPoint(IPAddress.Parse("118.89.189.165"), 3360));
        //Controller.SetSocket(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        ServerEndPoint = new IPEndPoint(IPAddress.Parse("118.89.189.165"), 3360);  // Connect to server IP and port
        ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Client communication socket

        try {
            ClientSocket.Connect(ServerEndPoint);
            //Controller.GetSocket().Connect(Controller.GetIPEndPoint());
            Debug.Log("Successfully connected to the server");
        } catch (Exception e) {
            Debug.Log(e.Message);
        }

        ClientSocket.Blocking = false;
        byte[] sendMsg = TransformController.Transform(msg);
        if(ClientSocket.Poll(-1, SelectMode.SelectWrite)) ClientSocket.Send(sendMsg);
        if (ClientSocket.Poll(-1, SelectMode.SelectRead)) {
            ClientSocket.Receive(sendMsg);
            if (sendMsg[0] == '2') {
                Debug.Log("login success");
                // save user name
                PlayerPrefs.SetString("username", username);
                // save socket
                GlobalController.instance.ClientSocket = ClientSocket;
                // load new scene
                SceneManager.LoadScene("DN_LV2_2_4");
                //SceneManager.LoadScene("PlayScene");
            } else {
                if (sendMsg[0] == '1') {
                    Debug.Log("user not found");
                } else if (sendMsg[0] == '3') {
                    Debug.Log("user already login");
                } else if (sendMsg[0] == '4') {
                    Debug.Log("password wrong");
                }
                ClientSocket.Close();
                //Controller.GetSocket().Close();
            }
        }
    }

    public void OnRegisterClick() {
        string username = user_name.text;
        string userpwd = user_pwd.text;

        msg.Optype = (1 << 1);
        msg.Username = username;
        msg.Userpwd = userpwd;

        //Controller.SetIPEndPoint(new IPEndPoint(IPAddress.Parse("118.89.189.165"), 3360));
        //Controller.SetSocket(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        ServerEndPoint = new IPEndPoint(IPAddress.Parse("118.89.189.165"), 3360);  // Connect to server IP and port
        ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Client communication socket

        try {
            ClientSocket.Connect(ServerEndPoint);
            //Controller.GetSocket().Connect(Controller.GetIPEndPoint());
            Debug.Log("Successfully connected to the server");
        } catch (Exception e) {
            Debug.Log(e.Message);
        }

        byte[] sendMsg = TransformController.Transform(msg);
        if (ClientSocket.Poll(-1, SelectMode.SelectWrite)) ClientSocket.Send(sendMsg);
        if (ClientSocket.Poll(-1, SelectMode.SelectRead)) {
            ClientSocket.Receive(sendMsg);
            //Controller.GetSocket().Send(msg);
            //Controller.GetSocket().Receive(msg);
            if (sendMsg[0] == '1') {
                Debug.Log("user already exist");
            } else if (sendMsg[0] == '2') {
                Debug.Log("register success");
            }
            ClientSocket.Close();
            //Controller.GetSocket().Close();
        }
    }
}
