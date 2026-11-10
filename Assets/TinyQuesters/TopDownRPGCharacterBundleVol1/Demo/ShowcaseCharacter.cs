// -----------------------------------------------------------------------------
//  Tiny Questers - Top-Down RPG Character Bundle Vol.1
//  ShowcaseCharacter.cs  (runtime)
//
//  Plays every animation of a single character one after another
//  (idle → walk → attack → hit → die → …) instead of looping idle forever,
//  and writes the current motion name into a small label under the character.
//
//  Added to each character instance in the TQ_Showcase scene by
//  TinyQuestersBuilder. The cycle is started by ShowcaseController once the
//  user presses PLAY.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TinyQuesters.Demo
{
    [RequireComponent(typeof(Animator))]
    public class ShowcaseCharacter : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("Small world-space text under the name that shows the current motion.")]
        public TextMesh motionLabel;

        [Tooltip("Minimum seconds a looping clip (idle/walk/turn) stays on screen.")]
        public float minLoopTime = 2f;

        [Tooltip("Extra hold (seconds) after a one-shot clip (attack/hit/die) finishes.")]
        public float oneShotHold = 0.5f;

        Animator anim;
        AnimationClip[] clips;   // unique clips, ordered by motion rank (idle first)
        string[] states;         // animator state names (== clip names)
        int idx;
        Coroutine loop;

        void Awake()
        {
            anim = GetComponent<Animator>();
            BuildClipList();
        }

        void BuildClipList()
        {
            var rac = anim != null ? anim.runtimeAnimatorController : null;
            if (rac == null) { clips = new AnimationClip[0]; states = new string[0]; return; }

            var seen = new HashSet<string>();
            var list = new List<AnimationClip>();
            foreach (var c in rac.animationClips)
            {
                if (c == null || seen.Contains(c.name)) continue;
                seen.Add(c.name);
                list.Add(c);
            }
            list.Sort((a, b) => Rank(a.name).CompareTo(Rank(b.name)));

            clips = list.ToArray();
            states = new string[clips.Length];
            for (int i = 0; i < clips.Length; i++) states[i] = clips[i].name;
        }

        // Begin (or restart) cycling through this character's motions.
        public void Begin()
        {
            if (clips == null || clips.Length == 0) return;
            if (loop != null) StopCoroutine(loop);
            idx = 0;
            loop = StartCoroutine(Cycle());
        }

        public void StopCycle()
        {
            if (loop != null) { StopCoroutine(loop); loop = null; }
        }

        // Play the current motion and snap to its first frame.
        void PlayCurrent()
        {
            anim.Play(states[idx], 0, 0f);
            anim.Update(0f);                 // snap to the first frame right away
            SetLabel(clips[idx].name);
        }

        IEnumerator Cycle()
        {
            while (true)
            {
                PlayCurrent();

                float dur = DurationFor(clips[idx]);
                float t = 0f;
                while (t < dur) { t += Time.deltaTime; yield return null; }

                idx = (idx + 1) % clips.Length;
            }
        }

        // Looping clips play whole cycles up to a minimum on-screen time;
        // one-shot clips play once and hold briefly on the last frame.
        float DurationFor(AnimationClip clip)
        {
            float len = Mathf.Max(0.1f, clip.length);
            if (clip.isLooping)
            {
                int cycles = Mathf.Max(1, Mathf.CeilToInt(minLoopTime / len));
                return cycles * len;
            }
            return len + oneShotHold;
        }

        void SetLabel(string clipName)
        {
            if (motionLabel != null) motionLabel.text = PrettyMotion(clipName);
        }

        // "TQ_Warrior_Idle_Down" → "Idle · Down"   (strips the "TQ_<Character>_" prefix)
        string PrettyMotion(string clipName)
        {
            string s = clipName;
            string prefix = name + "_";                 // e.g. "TQ_Warrior_"
            if (s.StartsWith(prefix)) s = s.Substring(prefix.Length);
            else if (s.StartsWith("TQ_"))
            {
                int u = s.IndexOf('_', 3);
                if (u >= 0) s = s.Substring(u + 1);
            }

            var dirs = new HashSet<string> { "Down", "Up", "Left", "Right" };
            string dir = null;
            var actionParts = new List<string>();
            foreach (var part in s.Split('_'))
            {
                if (dirs.Contains(part)) dir = part;
                else if (part.Length > 0) actionParts.Add(part);
            }

            string action = string.Join(" ", actionParts);
            if (string.IsNullOrEmpty(action)) action = "Clip";
            return dir != null ? action + " · " + dir : action;
        }

        // idle → walk → attack → hit → die → effect → turn → other; then by direction.
        static int Rank(string clipName)
        {
            string b = clipName.ToLowerInvariant();
            int a = b.Contains("idle") ? 0 : b.Contains("walk") ? 1 : b.Contains("attack") ? 2 :
                    b.Contains("hit") ? 3 : (b.Contains("die") || b.Contains("death")) ? 4 :
                    b.Contains("effect") ? 5 : b.Contains("turn") ? 6 : 7;
            int d = b.EndsWith("down") ? 0 : b.EndsWith("up") ? 1 :
                    b.EndsWith("left") ? 2 : b.EndsWith("right") ? 3 : 4;
            return a * 10 + d;
        }

        private void NextMotion()
        {
            if (clips == null || clips.Length == 0) return;
            idx = (idx + 1) % clips.Length;
            // Restart the cycle so its timer resets from the clicked motion
            // (otherwise the still-running timer would double-advance the motion).
            if (loop != null) StopCoroutine(loop);
            loop = StartCoroutine(Cycle());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            NextMotion();
        }
    }
}
