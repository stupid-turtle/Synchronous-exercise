using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class Skill_data {
    public string skill_name;
    public int skill_cool_down;
    public int skill_damage;
    public List<int> skill_attack_frame;
    public int skill_tot_frame;
    public int skill_type;
    public int skill_area_type;
    public float skill_radius;
}

[CreateAssetMenu(menuName = "Editor/SkillConfig")]
[Serializable]
public class SkillConfig : ScriptableObject {
    
    public List<Skill_data> skill_config_list = new List<Skill_data>();
}
