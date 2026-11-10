// -----------------------------------------------------------------------------
//  Tiny Questers - Top-Down RPG Character Bundle Vol.1
//  TinyQuestersDemoData.cs  (runtime data)
//
//  ScriptableObject listing the character assets shown in the demo, in order.
//  Each entry is its own DemoCharacterData asset (see Demo/Characters/).
// -----------------------------------------------------------------------------
using UnityEngine;

namespace TinyQuesters.Demo
{
    [CreateAssetMenu(fileName = "TinyQuestersDemoData", menuName = "Tiny Questers/Demo Data", order = 0)]
    public class TinyQuestersDemoData : ScriptableObject
    {
        [Tooltip("Character assets shown in the demo, in display order. Each is a DemoCharacter asset.")]
        public DemoCharacterData[] characters;
    }
}
