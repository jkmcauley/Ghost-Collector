// -----------------------------------------------------------------------------
//  Tiny Questers - Top-Down RPG Character Bundle Vol.1
//  TinyQuestersDemo.cs  (runtime)
//
//  A character preview demo that runs when you press Play.
//  Each motion is a real AnimationClip (.anim). The clip is played on the
//  stage SpriteRenderer through a PlayableGraph (AnimationClipPlayable), so no
//  per-frame sprite swapping and no AnimatorController asset are needed -
//  changing the motion just swaps the clip fed into the graph.
//  The character is rendered with a world-space SpriteRenderer viewed through an
//  orthographic Camera. Only the buttons / labels / dots are UI (Canvas).
//
//    ◀ ▶ : Change character (world slide transition)
//    ▲ ▼ : Change motion (clip) — the selected motion loops automatically
//    Auto : Automatically cycles motions (idle_down → idle_up → walk_down → …)
//    Bottom dots : Character indicator (click to jump)
//    Top : Character name + current motion
//    Space : Play / Pause
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Animations;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TinyQuesters.Demo
{
    public class TinyQuestersDemo : MonoBehaviour
    {
        [Header("Data (ScriptableObject)")]
        [Tooltip("Demo data asset")]
        public TinyQuestersDemoData data;

        [Header("Stage (world / SpriteRenderer)")]
        public Camera cam;
        [Tooltip("Stage slot A. Needs an Animator component (no controller); driven by a PlayableGraph.")]
        public SpriteRenderer rendererA;
        [Tooltip("Stage slot B (used for the slide transition). Needs an Animator component (no controller).")]
        public SpriteRenderer rendererB;
        [Tooltip("World slide distance. 0 = auto from the camera width.")]
        public float slideDistance = 0f;
        [Tooltip("Extra world-units of margin around the fitted character.")]
        public float fitMargin = 0.6f;

        [Header("Info Texts")]
        public Text categoryText;
        public Text nameText;
        public Text motionText;
        public Text countText;

        [Header("Nav Buttons")]
        public Button btnPrevChar;
        public Button btnNextChar;
        public Button btnPrevClip;
        public Button btnNextClip;
        public Button btnPlay;
        public Button btnAuto;
        public Animator btnAutoAnimator;
        public Animator btnPlayAnimator;

        [Header("Nav Animation Controllers")]
        public RuntimeAnimatorController autoOnController;
        public RuntimeAnimatorController autoOffController;
        public RuntimeAnimatorController playOnController;
        public RuntimeAnimatorController playOffController;

        [Header("Dots")]
        public GameObject dotPrefab;
        public RectTransform dotsRoot;
        public Sprite dotSprite;
        public Color dotOn  = new Color(0.66f, 0.55f, 0.98f);
        public Color dotOff = new Color(0.25f, 0.27f, 0.36f);

        [Header("Playback")]
        public float slideDuration = 0.45f;

        [Header("Auto Cycle")]
        public bool autoChange = true;            // default ON
        public float autoChangeInterval = 3f;     // seconds between motion changes

        int charIdx = 0, clipIdx = 0, active = 0;
        bool paused = false, sliding = false;
        float camBaseY = 0f;                      // camera stays put; characters are aligned to it
        bool playBusy = false, autoBusy = false;
        float autoTimer = 0f;
        static readonly int PressedHash = Animator.StringToHash("Pressed");

        SpriteRenderer[] slots;
        readonly Animator[] slotAnimators = new Animator[2];
        readonly PlayableGraph[] graphs = new PlayableGraph[2];
        readonly AnimationPlayableOutput[] outputs = new AnimationPlayableOutput[2];
        readonly AnimationClipPlayable[] clipPlayables = new AnimationClipPlayable[2];
        readonly AnimationClip[] slotClip = new AnimationClip[2];
        readonly List<Image> dots = new List<Image>();

        DemoCharacterData[] Characters => data != null ? data.characters : null;

        void Start()
        {
            if (data == null || data.characters == null || data.characters.Length == 0 || rendererA == null || rendererB == null)
            {
                Debug.LogWarning("[TinyQuestersDemo] Data/references are empty. Assign a TinyQuestersDemoData asset and Renderer A/B.");
                return;
            }

            slots = new[] { rendererA, rendererB };
            if (cam != null) camBaseY = cam.transform.position.y;
            slotAnimators[0] = GetSlotAnimator(rendererA);
            slotAnimators[1] = GetSlotAnimator(rendererB);

            if (btnPrevChar) btnPrevChar.onClick.AddListener(() => ChangeChar(-1));
            if (btnNextChar) btnNextChar.onClick.AddListener(() => ChangeChar(1));
            if (btnPrevClip) btnPrevClip.onClick.AddListener(() => ChangeClip(-1));
            if (btnNextClip) btnNextClip.onClick.AddListener(() => ChangeClip(1));
            if (btnPlay)     btnPlay.onClick.AddListener(TogglePause);
            if (btnAuto)     btnAuto.onClick.AddListener(ToggleAuto);
            if (btnPlayAnimator) btnPlayAnimator.runtimeAnimatorController = playOffController;
            if (btnAutoAnimator) btnAutoAnimator.runtimeAnimatorController = autoOffController;

            BuildDots();

            SetX(0, 0f);
            active = 0;
            slots[0].gameObject.SetActive(true);
            slots[1].gameObject.SetActive(false);
            SetupSlot(0, charIdx, clipIdx);   // activate first, so the graph's Evaluate can update the sprite

            FitActive();
            UpdateInfo();
            UpdateDots();
        }

        // Slots are driven by a PlayableGraph, so each needs an Animator (no controller).
        // Attach the Animator yourself in the scene; this only fetches it.
        Animator GetSlotAnimator(SpriteRenderer sr)
        {
            var a = sr.GetComponent<Animator>();
            if (a == null)
                Debug.LogWarning("[TinyQuestersDemo] Slot '" + sr.name + "' has no Animator. Add an Animator component (no controller needed) to each stage slot.", sr);
            return a;
        }

        void Update()
        {
            if (slots == null) return;

            LoopSlot(active);
            if (sliding) LoopSlot(1 - active);

            // Auto-cycle motions (idle_down → idle_up → walk_down → …)
            if (autoChange && !paused && !sliding)
            {
                autoTimer += Time.deltaTime;
                if (autoTimer >= autoChangeInterval)
                {
                    autoTimer = 0f;
                    ChangeClip(1);
                }
            }

            HandleKeyboard();
        }

        void HandleKeyboard()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.rightArrowKey.wasPressedThisFrame) ChangeChar(1);
            if (kb.leftArrowKey.wasPressedThisFrame)  ChangeChar(-1);
            if (kb.downArrowKey.wasPressedThisFrame)  ChangeClip(1);
            if (kb.upArrowKey.wasPressedThisFrame)    ChangeClip(-1);
            if (kb.spaceKey.wasPressedThisFrame)      TogglePause();
#else
            if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeChar(1);
            if (Input.GetKeyDown(KeyCode.LeftArrow))  ChangeChar(-1);
            if (Input.GetKeyDown(KeyCode.DownArrow))  ChangeClip(1);
            if (Input.GetKeyDown(KeyCode.UpArrow))    ChangeClip(-1);
            if (Input.GetKeyDown(KeyCode.Space))      TogglePause();
#endif
        }

        // ---- Data access ----
        DemoCharacterData CharAt(int c)
        {
            var chars = Characters;
            if (chars == null || c < 0 || c >= chars.Length) return null;
            return chars[c];
        }

        // ---- Clip playback (Playables) ----

        // Loop every motion: when the clip reaches its end, wrap the time back so
        // even non-looping clips (attack/hit/die) keep replaying on the stage.
        void LoopSlot(int i)
        {
            if (paused) return;
            if (!clipPlayables[i].IsValid()) return;
            var clip = slotClip[i];
            if (clip == null) return;
            float len = clip.length;
            if (len <= 0.0001f) return;
            double t = clipPlayables[i].GetTime();
            if (t >= len) clipPlayables[i].SetTime(t % len);
        }

        void EnsureGraph(int i)
        {
            if (graphs[i].IsValid()) return;
            var g = PlayableGraph.Create("TQDemoSlot" + i);
            g.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            outputs[i] = AnimationPlayableOutput.Create(g, "Animation", slotAnimators[i]);
            graphs[i] = g;
        }

        // Swap the clip fed into slot i's graph and start it at frame 0.
        void PlaySlotClip(int i, AnimationClip clip)
        {
            slotClip[i] = clip;
            if (clip == null || slotAnimators[i] == null)
            {
                // No clip to show: tear the slot down so no stale animation/sprite lingers.
                if (clipPlayables[i].IsValid()) clipPlayables[i].Destroy();
                StopSlot(i);
                if (slots != null && slots[i] != null) slots[i].sprite = null;
                return;
            }

            EnsureGraph(i);
            if (clipPlayables[i].IsValid()) clipPlayables[i].Destroy();

            var p = AnimationClipPlayable.Create(graphs[i], clip);
            p.SetApplyFootIK(false);
            clipPlayables[i] = p;
            outputs[i].SetSourcePlayable(p);

            p.SetTime(0);
            p.SetSpeed(paused ? 0 : 1);
            if (!graphs[i].IsPlaying()) graphs[i].Play();
            graphs[i].Evaluate(0f);   // snap to the first frame so the sprite/bounds are valid now
        }

        void SetupSlot(int i, int c, int cl)
        {
            var ch = CharAt(c);
            DemoClip dc = null;
            if (ch != null && ch.clips != null && ch.clips.Length > 0)
            {
                int k = Mathf.Clamp(cl, 0, ch.clips.Length - 1);
                dc = ch.clips[k];
            }
            PlaySlotClip(i, dc != null ? dc.clip : null);
        }

        void StopSlot(int i)
        {
            if (graphs[i].IsValid() && graphs[i].IsPlaying()) graphs[i].Stop();
        }

        // ---- Change motion ----
        public void ChangeClip(int delta)
        {
            if (sliding) return;
            autoTimer = 0f;
            var ch = CharAt(charIdx);
            if (ch == null || ch.clips == null || ch.clips.Length == 0) return;
            clipIdx = (clipIdx + delta + ch.clips.Length) % ch.clips.Length;
            var dc = ch.clips[clipIdx];
            PlaySlotClip(active, dc != null ? dc.clip : null);
            FitActive();
            UpdateInfo();
        }

        // ---- Change character (world slide) ----
        public void ChangeChar(int delta)
        {
            int n = Characters != null ? Characters.Length : 0;
            if (n == 0) return;
            int ni = (charIdx + delta + n) % n;
            GoTo(ni, delta > 0 ? 1 : -1);
        }

        public void GoTo(int ni, int dir)
        {
            if (sliding || ni == charIdx) return;
            autoTimer = 0f;
            StartCoroutine(SlideCo(ni, dir));
        }

        IEnumerator SlideCo(int ni, int dir)
        {
            sliding = true;
            int inSlot = 1 - active;
            float w = SlideDist();

            slots[inSlot].gameObject.SetActive(true);   // activate before Setup so the graph's Evaluate can update the sprite
            SetupSlot(inSlot, ni, 0);

            // Align both slots up front so the incoming character flies in at exactly
            // the height it will settle at - no vertical jump when it arrives.
            AlignY(inSlot);
            AlignY(active);

            // Zoom is the only thing that still differs per character; ease it across
            // the slide instead of snapping at the end.
            float sizeFrom = cam != null ? cam.orthographicSize : 0f;
            float sizeTo   = cam != null ? FitSize(inSlot) : 0f;

            SetX(inSlot, dir * w);
            SetX(active, 0f);

            float t = 0f;
            while (t < slideDuration)
            {
                // The slide runs regardless of pause, so you can browse characters while paused.
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / slideDuration));
                SetX(inSlot, Mathf.Lerp(dir * w, 0f, k));
                SetX(active, Mathf.Lerp(0f, -dir * w, k));
                if (cam != null) cam.orthographicSize = Mathf.Lerp(sizeFrom, sizeTo, k);
                yield return null;
            }

            SetX(inSlot, 0f);
            slots[active].gameObject.SetActive(false);
            StopSlot(active);

            active = inSlot;
            charIdx = ni;
            clipIdx = 0;
            sliding = false;

            FitActive();
            UpdateInfo();
            UpdateDots();
        }

        void SetX(int i, float x)
        {
            var p = slots[i].transform.localPosition;
            p.x = x;
            slots[i].transform.localPosition = p;
        }

        float SlideDist()
        {
            if (slideDistance > 0f) return slideDistance;
            if (cam != null && cam.orthographic) return cam.orthographicSize * cam.aspect * 2f + 2f;
            return 10f;
        }

        // ---- Fit the camera to the active character sprite (size) ----
        // The camera never moves vertically; each slot is raised/lowered instead so
        // every character sits at the same screen height - including while sliding in.
        void FitActive()
        {
            if (cam == null || slots == null) return;
            if (slots[active].sprite == null) return;

            cam.orthographicSize = FitSize(active);
            AlignY(active);
        }

        // Orthographic size that frames slot i's current sprite.
        float FitSize(int i)
        {
            var s = slots[i].sprite;
            if (cam == null || s == null) return cam != null ? cam.orthographicSize : 0f;

            float ex = s.bounds.extents.x;
            float ey = s.bounds.extents.y;
            float aspect = Mathf.Max(cam.aspect, 0.1f);
            return Mathf.Max(ey, ex / aspect) + fitMargin;
        }

        // Put the sprite's vertical center on the camera's center line
        // (the pivot is at the feet, so the offset differs per character).
        void AlignY(int i)
        {
            var sr = slots[i];
            var s = sr.sprite;
            if (s == null) return;
            var wp = sr.transform.position;
            wp.y = camBaseY - s.bounds.center.y;
            sr.transform.position = wp;
        }

        // ---- Info / dots ----
        void UpdateInfo()
        {
            var ch = CharAt(charIdx);
            if (ch == null) return;
            if (categoryText) categoryText.text = (ch.category ?? "").ToUpper();
            if (nameText)     nameText.text = ch.characterName;
            if (motionText)   motionText.text = Label(ch, clipIdx);
            int n = ch.clips != null ? ch.clips.Length : 0;
            if (countText)    countText.text = n > 0 ? ((clipIdx + 1) + " / " + n) : "0 / 0";
        }

        string Label(DemoCharacterData ch, int cl)
        {
            if (ch == null || ch.clips == null || cl < 0 || cl >= ch.clips.Length) return "";
            var dc = ch.clips[cl];
            if (dc == null) return "";
            if (!string.IsNullOrEmpty(dc.label)) return dc.label;
            if (dc.clip != null) return PrettyMotion(dc.clip.name, ch.characterName);
            return "";
        }

        // "TQ_Warrior_Idle_Down" → "Idle · Down"   (strips the "TQ_<Character>_" prefix)
        string PrettyMotion(string clipName, string charName)
        {
            string s = clipName;
            if (!string.IsNullOrEmpty(charName))
            {
                string tqPrefix = "TQ_" + charName + "_";
                if (s.StartsWith(tqPrefix)) s = s.Substring(tqPrefix.Length);
                else if (s.StartsWith(charName + "_")) s = s.Substring(charName.Length + 1);
            }
            if (s.StartsWith("TQ_"))
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

        void BuildDots()
        {
            if (dotsRoot == null) return;
            if (dotPrefab == null)
            {
                Debug.LogWarning("[TinyQuestersDemo] dotPrefab is not assigned; character dots will not appear.", this);
                return;
            }
            int count = Characters != null ? Characters.Length : 0;
            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(dotPrefab, dotsRoot);
                go.name = "Dot" + i;

                var img = go.GetComponent<Image>();
                if (img == null) img = go.GetComponentInChildren<Image>();
                if (img != null)
                {
                    if (dotSprite != null) img.sprite = dotSprite;
                    img.color = dotOff;
                    dots.Add(img);
                }

                var btn = go.GetComponent<Button>();
                if (btn == null) btn = go.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    btn.transition = Selectable.Transition.None;
                    int idx = i;
                    btn.onClick.AddListener(() => { if (idx != charIdx) GoTo(idx, idx > charIdx ? 1 : -1); });
                }
            }
        }

        void UpdateDots()
        {
            for (int i = 0; i < dots.Count; i++)
                dots[i].color = (i == charIdx) ? dotOn : dotOff;
        }

        public void TogglePause()
        {
            if (playBusy) return;
            paused = !paused;
            for (int i = 0; i < 2; i++)
                if (clipPlayables[i].IsValid()) clipPlayables[i].SetSpeed(paused ? 0 : 1);
            StartCoroutine(PressAndSwap(btnPlayAnimator, paused ? playOnController : playOffController, v => playBusy = v));
        }

        public void ToggleAuto()
        {
            if (autoBusy) return;
            autoChange = !autoChange;
            autoTimer = 0f;
            StartCoroutine(PressAndSwap(btnAutoAnimator, autoChange ? autoOffController : autoOnController, v => autoBusy = v));
        }

        // Plays the button Animator's "Pressed" clip from frame 0, waits for it to
        // finish, then swaps in the target controller and rebinds so the NEXT press
        // replays cleanly (fixes "works only once / doesn't reset").
        // NOTE: set the Button's Transition to "None" so it doesn't fight these calls.
        IEnumerator PressAndSwap(Animator anim, RuntimeAnimatorController next, System.Action<bool> setBusy)
        {
            if (anim == null) yield break;
            setBusy(true);

            anim.Play(PressedHash, 0, 0f);   // restart the Pressed clip from the beginning
            anim.Update(0f);

            yield return null;               // let the state info settle
            var info = anim.GetCurrentAnimatorStateInfo(0);
            float wait = info.shortNameHash == PressedHash ? info.length : 0f;
            if (wait > 0f) yield return new WaitForSeconds(wait);

            if (next != null) anim.runtimeAnimatorController = next;
            anim.Rebind();                   // reset so a future press starts fresh
            anim.Update(0f);

            setBusy(false);
        }

        void OnDestroy()
        {
            for (int i = 0; i < 2; i++)
                if (graphs[i].IsValid()) graphs[i].Destroy();
        }
    }
}

