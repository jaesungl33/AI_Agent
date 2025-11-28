using System;
using System.Collections.Generic;
using UnityEngine;
namespace GDO
{

    public enum NPCChatStates
    {
        Default = 0,
        Presenting1 = 1,
        Awkward = 2,
        Determined1 = 3,
        Determined2 = 4,
        Panic = 5,
        Startled = 6,
        Happy1 = 7,
        Happy2 = 8,
        Presenting2 = 9,
        Idea1 = 10,
        Bye1 = 11,
    }

    [Serializable]
    public class NPCAvatar
    {
        public NPCChatStates state;
        public Sprite sprite;
    }
    [Serializable]
    public class NpcData
    {
        public string npcId;
        public string npcName;
        public string npcBuildingOwnerPrefab;
        public Sprite smallAvatar;
        public Sprite defaultAvatar;
        public List<NPCAvatar> avatars;
    }
    [CreateAssetMenu(fileName = "NPCChatStatesData", menuName = "GDO/NPCChatStatesData", order = 1)]
    public class NPCChatStatesData : ScriptableObject
    {
        [SerializeField] private NpcData[] npcData;
        [SerializeField] public NpcData[] Data => npcData;
    }
}