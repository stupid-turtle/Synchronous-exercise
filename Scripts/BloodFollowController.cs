using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BloodFollowController : MonoBehaviour{
    
    private Camera mainCamera;
    float height;
    string username;
    int playerId;
    public Texture2D bloodBackground;
    public Texture2D bloodFill;
    int totHealthPoint;
    int nowHealthPoint;

    // Start is called before the first frame update
    void Start() {
        mainCamera = Camera.main;
        username = gameObject.name;
        playerId = ModelLayer.PlayerMap[username];
        float size_y = GetComponent<Renderer>().bounds.size.y;
        height = size_y;
    }

    void Update() {
        totHealthPoint = ModelLayer.TotHealthPotnt[playerId];
        nowHealthPoint = ModelLayer.NowHealthPoint[playerId];
        mainCamera = Camera.main;
        float size_y = GetComponent<Collider>().bounds.size.y;
        float scal_y = transform.localScale.y;
        height = size_y;
    }

    void OnGUI() {
        Vector3 worldPosition = new Vector3(transform.position.x, transform.position.y + height, transform.position.z);
        Vector2 position = mainCamera.WorldToScreenPoint(worldPosition);
        position = new Vector2(position.x, Screen.height - position.y);
        Vector2 bloodSize = GUI.skin.label.CalcSize(new GUIContent(bloodFill));
        float bloodWidth = bloodFill.width * nowHealthPoint / totHealthPoint;
        GUI.DrawTexture(new Rect(position.x - (bloodSize.x / 2), position.y - bloodSize.y, bloodSize.x, bloodSize.y), bloodBackground);
        GUI.DrawTexture(new Rect(position.x - (bloodSize.x / 2), position.y - bloodSize.y, bloodWidth, bloodSize.y), bloodFill);

        Vector2 nameSize = GUI.skin.label.CalcSize(new GUIContent(username));
        GUI.color = Color.black;
        GUI.Label(new Rect(position.x - (nameSize.x / 2), position.y - nameSize.y - bloodSize.y, nameSize.x, nameSize.y), username);
    }
}
