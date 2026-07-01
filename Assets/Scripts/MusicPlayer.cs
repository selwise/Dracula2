using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class MusicPlayer : MonoBehaviour
{
    [Header("Context")]
    public AdventureLoopController loopController;
    public bool useActiveActorRoom = true;
    public float contextRefreshSeconds = 0.25f;

    [Header("Music")]
    public AudioSource musicSource;
    public List<MusicTrack> musicTracks = new List<MusicTrack>();

    [Header("Looping Ambience")]
    public List<LoopingAmbience> loopingAmbience = new List<LoopingAmbience>();

    [Header("Random One Shots")]
    public AudioSource oneShotSource;
    public List<RandomOneShotGroup> randomOneShotGroups = new List<RandomOneShotGroup>();

    private AudioContext currentContext;
    private MusicTrack activeTrack;
    private float nextContextRefreshTime;

    private void Awake()
    {
        if (loopController == null)
        {
            loopController = FindAnyObjectByType<AdventureLoopController>();
        }

        musicSource = musicSource != null ? musicSource : FindReusableSource(true, "Music Source");
        oneShotSource = oneShotSource != null ? oneShotSource : AddSource("One Shot Source", false);
        ConfigureMusicSource();
        ConfigureOneShotSource();

        for (int i = 0; i < loopingAmbience.Count; i++)
        {
            LoopingAmbience layer = loopingAmbience[i];
            if (layer == null)
            {
                continue;
            }

            layer.source = layer.source != null
                ? layer.source
                : AddSource(string.IsNullOrWhiteSpace(layer.label) ? "Ambience Source" : layer.label + " Source", true);
            ConfigureLoopingSource(layer.source);
        }
    }

    private void OnEnable()
    {
        currentContext = ReadCurrentContext();
        nextContextRefreshTime = 0f;
        activeTrack = null;

        for (int i = 0; i < randomOneShotGroups.Count; i++)
        {
            RandomOneShotGroup group = randomOneShotGroups[i];
            if (group != null)
            {
                group.ResetSchedule(Time.time);
            }
        }
    }

    private void Update()
    {
        if (Time.time >= nextContextRefreshTime)
        {
            currentContext = ReadCurrentContext();
            nextContextRefreshTime = Time.time + Mathf.Max(0.05f, contextRefreshSeconds);
        }

        UpdateMusic();
        UpdateLoopingAmbience();
        UpdateRandomOneShots();
    }

    private void OnValidate()
    {
        contextRefreshSeconds = Mathf.Max(0.05f, contextRefreshSeconds);
        ClampTrackVolumes();
    }

    private AudioContext ReadCurrentContext()
    {
        Scene scene = SceneManager.GetActiveScene();
        string roomName = string.Empty;
        AdventurePhase phase = AdventurePhase.Day;
        bool hasPhase = false;

        if (loopController != null)
        {
            if (useActiveActorRoom && loopController.ActiveActor != null)
            {
                roomName = loopController.ActiveActor.roomName;
            }

            phase = loopController.State.Phase;
            hasPhase = true;
        }

        return new AudioContext(scene.name, scene.path, roomName, phase, hasPhase);
    }

    private void UpdateMusic()
    {
        MusicTrack nextTrack = SelectBestMusicTrack();
        if (nextTrack == null || nextTrack.clip == null)
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }

            activeTrack = null;
            return;
        }

        if (musicSource == null)
        {
            musicSource = FindReusableSource(true, "Music Source");
            ConfigureMusicSource();
        }

        float targetVolume = Mathf.Clamp01(nextTrack.volume);
        bool needsNewClip = activeTrack != nextTrack || musicSource.clip != nextTrack.clip;
        if (needsNewClip)
        {
            musicSource.clip = nextTrack.clip;
            musicSource.loop = nextTrack.loop;
            musicSource.volume = targetVolume;
            musicSource.Play();
            activeTrack = nextTrack;
            return;
        }

        musicSource.loop = nextTrack.loop;
        musicSource.volume = targetVolume;
        if (!musicSource.isPlaying && nextTrack.playWhenMatched)
        {
            musicSource.Play();
        }
    }

    private MusicTrack SelectBestMusicTrack()
    {
        MusicTrack bestTrack = null;
        int bestScore = int.MinValue;

        for (int i = 0; i < musicTracks.Count; i++)
        {
            MusicTrack track = musicTracks[i];
            if (track == null || !track.enabled || track.clip == null || !track.rule.Matches(currentContext))
            {
                continue;
            }

            int score = track.priority * 1000 + track.rule.SpecificityScore;
            if (score > bestScore)
            {
                bestScore = score;
                bestTrack = track;
            }
        }

        return bestTrack;
    }

    private void UpdateLoopingAmbience()
    {
        for (int i = 0; i < loopingAmbience.Count; i++)
        {
            LoopingAmbience layer = loopingAmbience[i];
            if (layer == null || layer.clip == null)
            {
                continue;
            }

            if (layer.source == null)
            {
                layer.source = AddSource(string.IsNullOrWhiteSpace(layer.label) ? "Ambience Source" : layer.label + " Source", true);
                ConfigureLoopingSource(layer.source);
            }

            bool shouldPlay = layer.enabled && layer.rule.Matches(currentContext);
            layer.source.clip = layer.clip;
            layer.source.loop = true;
            layer.source.volume = shouldPlay ? Mathf.Clamp01(layer.volume) : 0f;

            if (shouldPlay)
            {
                if (!layer.source.isPlaying)
                {
                    layer.source.Play();
                }
            }
            else if (layer.stopWhenUnmatched && layer.source.isPlaying)
            {
                layer.source.Stop();
            }
        }
    }

    private void UpdateRandomOneShots()
    {
        if (oneShotSource == null)
        {
            oneShotSource = AddSource("One Shot Source", false);
            ConfigureOneShotSource();
        }

        for (int i = 0; i < randomOneShotGroups.Count; i++)
        {
            RandomOneShotGroup group = randomOneShotGroups[i];
            if (group == null || !group.enabled || !group.rule.Matches(currentContext))
            {
                if (group != null)
                {
                    group.ResetSchedule(Time.time);
                }

                continue;
            }

            if (!group.HasPlayableClip)
            {
                continue;
            }

            if (group.nextPlayTime <= 0f)
            {
                group.ResetSchedule(Time.time);
            }

            if (Time.time < group.nextPlayTime)
            {
                continue;
            }

            RandomClipSlot slot = group.SelectRandomClip();
            if (slot == null || slot.clip == null)
            {
                group.ResetSchedule(Time.time);
                continue;
            }

            oneShotSource.PlayOneShot(slot.clip, Mathf.Clamp01(group.volume * slot.volume));
            group.ScheduleNext(Time.time, slot.clip.length);
        }
    }

    private AudioSource FindReusableSource(bool loop, string fallbackName)
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource candidate = sources[i];
            if (candidate == null)
            {
                continue;
            }

            if (loop && candidate.loop)
            {
                return candidate;
            }

            if (!loop && !candidate.loop)
            {
                return candidate;
            }
        }

        return AddSource(fallbackName, loop);
    }

    private AudioSource AddSource(string sourceName, bool loop)
    {
        AudioSource created = gameObject.AddComponent<AudioSource>();
        created.playOnAwake = false;
        created.loop = loop;
        created.spatialBlend = 0f;
        created.name = sourceName;
        return created;
    }

    private void ConfigureMusicSource()
    {
        if (musicSource == null)
        {
            return;
        }

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
    }

    private static void ConfigureLoopingSource(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
    }

    private void ConfigureOneShotSource()
    {
        if (oneShotSource == null)
        {
            return;
        }

        oneShotSource.playOnAwake = false;
        oneShotSource.loop = false;
        oneShotSource.spatialBlend = 0f;
    }

    private void ClampTrackVolumes()
    {
        for (int i = 0; i < musicTracks.Count; i++)
        {
            if (musicTracks[i] != null)
            {
                musicTracks[i].volume = Mathf.Clamp01(musicTracks[i].volume);
            }
        }

        for (int i = 0; i < loopingAmbience.Count; i++)
        {
            if (loopingAmbience[i] != null)
            {
                loopingAmbience[i].volume = Mathf.Clamp01(loopingAmbience[i].volume);
            }
        }

        for (int i = 0; i < randomOneShotGroups.Count; i++)
        {
            RandomOneShotGroup group = randomOneShotGroups[i];
            if (group == null)
            {
                continue;
            }

            group.volume = Mathf.Clamp01(group.volume);
            group.pauseRange.x = Mathf.Max(0f, group.pauseRange.x);
            group.pauseRange.y = Mathf.Max(group.pauseRange.x, group.pauseRange.y);

            for (int slotIndex = 0; slotIndex < group.clips.Count; slotIndex++)
            {
                if (group.clips[slotIndex] != null)
                {
                    group.clips[slotIndex].volume = Mathf.Clamp01(group.clips[slotIndex].volume);
                }
            }
        }
    }

    [Serializable]
    public sealed class MusicTrack
    {
        public string label;
        public bool enabled = true;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.45f;
        public bool loop = true;
        public bool playWhenMatched = true;
        public int priority;
        public AudioContextRule rule = new AudioContextRule();
    }

    [Serializable]
    public sealed class LoopingAmbience
    {
        public string label;
        public bool enabled = true;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.25f;
        public bool stopWhenUnmatched = true;
        public AudioSource source;
        public AudioContextRule rule = new AudioContextRule();
    }

    [Serializable]
    public sealed class RandomOneShotGroup
    {
        public string label;
        public bool enabled = true;
        [Range(0f, 1f)] public float volume = 0.75f;
        public Vector2 pauseRange = new Vector2(12f, 38f);
        public bool avoidImmediateRepeat = true;
        public List<RandomClipSlot> clips = new List<RandomClipSlot>();
        public AudioContextRule rule = new AudioContextRule();

        [NonSerialized] public float nextPlayTime;
        [NonSerialized] private int lastClipIndex = -1;

        public bool HasPlayableClip
        {
            get
            {
                for (int i = 0; i < clips.Count; i++)
                {
                    if (clips[i] != null && clips[i].clip != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void ResetSchedule(float now)
        {
            nextPlayTime = now + RandomPause();
        }

        public void ScheduleNext(float now, float clipLength)
        {
            nextPlayTime = now + Mathf.Max(0f, clipLength) + RandomPause();
        }

        public RandomClipSlot SelectRandomClip()
        {
            List<int> playable = new List<int>();
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i] != null && clips[i].clip != null)
                {
                    playable.Add(i);
                }
            }

            if (playable.Count == 0)
            {
                return null;
            }

            int selectedPlayableIndex = UnityEngine.Random.Range(0, playable.Count);
            int selectedIndex = playable[selectedPlayableIndex];

            if (avoidImmediateRepeat && playable.Count > 1 && selectedIndex == lastClipIndex)
            {
                selectedPlayableIndex = (selectedPlayableIndex + UnityEngine.Random.Range(1, playable.Count)) % playable.Count;
                selectedIndex = playable[selectedPlayableIndex];
            }

            lastClipIndex = selectedIndex;
            return clips[selectedIndex];
        }

        private float RandomPause()
        {
            float min = Mathf.Max(0f, pauseRange.x);
            float max = Mathf.Max(min, pauseRange.y);
            return UnityEngine.Random.Range(min, max);
        }
    }

    [Serializable]
    public sealed class RandomClipSlot
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Serializable]
    public sealed class AudioContextRule
    {
        [Tooltip("Blank means any scene. Matches either active scene name or scene path.")]
        public string sceneNameOrPath;
        [Tooltip("Blank means any room. Uses the active actor room when a loop controller is assigned.")]
        public string roomName;
        public bool restrictToPhase;
        public AdventurePhase phase = AdventurePhase.Day;

        public int SpecificityScore
        {
            get
            {
                int score = 0;
                if (!string.IsNullOrWhiteSpace(sceneNameOrPath))
                {
                    score += 10;
                }

                if (!string.IsNullOrWhiteSpace(roomName))
                {
                    score += 20;
                }

                if (restrictToPhase)
                {
                    score += 5;
                }

                return score;
            }
        }

        public bool Matches(AudioContext context)
        {
            if (!string.IsNullOrWhiteSpace(sceneNameOrPath)
                && !StringEquals(sceneNameOrPath, context.sceneName)
                && !StringEquals(sceneNameOrPath, context.scenePath))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(roomName) && !StringEquals(roomName, context.roomName))
            {
                return false;
            }

            if (restrictToPhase && (!context.hasPhase || context.phase != phase))
            {
                return false;
            }

            return true;
        }

        private static bool StringEquals(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }

    public readonly struct AudioContext
    {
        public readonly string sceneName;
        public readonly string scenePath;
        public readonly string roomName;
        public readonly AdventurePhase phase;
        public readonly bool hasPhase;

        public AudioContext(string sceneName, string scenePath, string roomName, AdventurePhase phase, bool hasPhase)
        {
            this.sceneName = sceneName ?? string.Empty;
            this.scenePath = scenePath ?? string.Empty;
            this.roomName = roomName ?? string.Empty;
            this.phase = phase;
            this.hasPhase = hasPhase;
        }
    }
}
