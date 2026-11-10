// -----------------------------------------------------------------------------
//  Tiny Questers - Top-Down RPG Character Bundle Vol.1
//  DemoCharacterData.cs  (runtime data)
//
//  One demo character = one ScriptableObject asset (e.g. Demo/Characters/Bat.asset).
//  Holds the character's motions.
// -----------------------------------------------------------------------------
using UnityEngine;

namespace TinyQuesters.Demo
{
    [System.Serializable]
    public class DemoClip
    {
        [Tooltip("Optional display label ('Idle · Down'). Left empty = derived from the clip name.")]
        public string label;
        [Tooltip("The animation clip (.anim) to play for this motion.")]
        public AnimationClip clip;
    }

    [CreateAssetMenu(fileName = "DemoCharacter", menuName = "Tiny Questers/Demo Character", order = 1)]
    public class DemoCharacterData : ScriptableObject
    {
        [Tooltip("Display name shown in the demo, e.g. 'Warrior'.")]
        public string characterName;
        [Tooltip("'Heroes' / 'NPC' / 'Monster'.")]
        public string category;
        [Tooltip("Motions for this character.")]
        public DemoClip[] clips;
    }
}
