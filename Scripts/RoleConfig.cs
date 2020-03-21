using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class role_data {
    public float posx;
    public float posy;
    public float posz;
    public float rotx;
    public float roty;
    public float rotz;
}

[CreateAssetMenu(menuName = "Editor/RoleConfig")]
[Serializable]
public class RoleConfig : ScriptableObject {
    // Start is called before the first frame update
    public List<role_data> role_config_list = new List<role_data>();
}
