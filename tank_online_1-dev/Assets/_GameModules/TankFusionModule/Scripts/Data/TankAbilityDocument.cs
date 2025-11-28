// Date: 28 June 2025
// AI-Generated: This code structure was created with AI assistance

using UnityEngine;

[System.Serializable]
public class TankAbilityDocument
{
    public string abilityID; // Unique identifier for the skill
    public string abilityName; // Name of the skill
    public string description; // Description of the skill
    public float castTime; // Time taken to cast the skill in seconds
    public float duration; // Duration of the skill effect in seconds
    public float cooldown; // Cooldown time for the skill in seconds
    public CustomProperty[] customProperties; // Array of properties associated with the skill

    //contructer
    public TankAbilityDocument(string id, string name, string desc, float dur, float cd, CustomProperty[] props)
    {
        abilityID = id;
        abilityName = name;
        description = desc;
        duration = dur;
        cooldown = cd;
        customProperties = props;
    }
}
[System.Serializable]
public class CustomProperty
{
    public string propertyName;
    public float propertyValue;
}