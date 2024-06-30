using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioSourceListClass", menuName = "ScriptableObjects/AudioSourceListClass")]
[System.Serializable]
public class AudioSourceListClass : ScriptableObject
{
    public List<AudioValue> audioValue;

    [System.Serializable]
    public class AudioValue
    {
        public AudioClip _Audioclip;
        public AudioMixerGroup _AudioMixerGroup;
        public bool _Mute;
        public bool _BypassEffects; 
        public bool _BypassListenerEffects; 
        public bool _BypassRevevbZones;
        public bool _PlayOnAwake;
        public bool _Loop;

        [SerializeField, Range(0, 256)]
        public int _Priority;

        [SerializeField, Range(0f, 1f)]
        public float _Volume;

        [SerializeField, Range(-3f, 3f)]
        public float _Pitch;

        [SerializeField, Range(-1f, 1f)]
        public float _StereoPan;

        [SerializeField, Range(0f, 1f)]
        public float _SpatialBlend;

        [SerializeField, Range(0f, 1.1f)]
        public float _ReverbZoneMix;

        [Header("3D Sound Setting")]
        [SerializeField, Range(0f, 5f)]
        public float _DopplerLevel;

        [SerializeField, Range(0, 360)]
        public float _Spread;

        public AudioRolloffMode _VolumeRolloff; // Logarithmic, Linear , Custom;

        [SerializeField, Range(0f, 1000000.00f)]
        public float _MinDistance;

        [SerializeField, Range(0.01f, 1000000.00f)]
        public float _MaxDistance;

        public AnimationCurve _RolloffCustomCurve;

        public AudioValue()
        {
            _Audioclip = null;
            _AudioMixerGroup = null;
            _Mute = false;
            _BypassEffects = false;
            _BypassListenerEffects = false;
            _BypassRevevbZones = false;
            _PlayOnAwake =true;
            _Loop = false;
            _Priority = 128;
            _Volume = 1f;
            _Pitch = 1f;
            _StereoPan = 0f;
            _SpatialBlend = 0f;
            _ReverbZoneMix = 0f;
            _DopplerLevel = 1f;
            _Spread = 0f;
            _VolumeRolloff = AudioRolloffMode.Logarithmic;
            _MinDistance = 1f;
            _MaxDistance = 500f;
            _RolloffCustomCurve = new AnimationCurve();
            _RolloffCustomCurve.AddKey(0.0f, 0.0f);
        }

        public AudioValue(AudioSource audioSource)
        {
            _Audioclip = audioSource.clip;
            _AudioMixerGroup = audioSource.outputAudioMixerGroup;
            _Mute = audioSource.mute;
            _BypassEffects = audioSource.bypassEffects;
            _BypassListenerEffects = audioSource.bypassListenerEffects;
            _BypassRevevbZones = audioSource.bypassReverbZones;
            _PlayOnAwake = audioSource.playOnAwake;
            _Loop = audioSource.loop;
            _Priority = audioSource.priority;
            _Volume = audioSource.volume;
            _Pitch = audioSource.pitch;
            _StereoPan = audioSource.panStereo;
            _SpatialBlend = audioSource.spatialBlend;
            _ReverbZoneMix = audioSource.reverbZoneMix;
            _DopplerLevel = audioSource.dopplerLevel;
            _Spread = audioSource.spread;
            _VolumeRolloff = audioSource.rolloffMode;
            _MinDistance = audioSource.minDistance;
            _MaxDistance = audioSource.maxDistance;
            _RolloffCustomCurve = audioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
        }

        public void SetAudioValue(AudioSource audioSource)
        {
            _Audioclip = audioSource.clip;
            _AudioMixerGroup = audioSource.outputAudioMixerGroup;
            _Mute = audioSource.mute;
            _BypassEffects = audioSource.bypassEffects;
            _BypassListenerEffects = audioSource.bypassListenerEffects;
            _BypassRevevbZones = audioSource.bypassReverbZones;
            _PlayOnAwake = audioSource.playOnAwake;
            _Loop = audioSource.loop;
            _Priority = audioSource.priority;
            _Volume = audioSource.volume;
            _Pitch = audioSource.pitch;
            _StereoPan = audioSource.panStereo;
            _SpatialBlend = audioSource.spatialBlend;
            _ReverbZoneMix = audioSource.reverbZoneMix;
            _DopplerLevel = audioSource.dopplerLevel;
            _Spread = audioSource.spread;
            _VolumeRolloff = audioSource.rolloffMode;
            _MinDistance = audioSource.minDistance;
            _MaxDistance = audioSource.maxDistance;
            _RolloffCustomCurve = audioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
        }

        public void GetAudioValue(AudioSource audioSource)
        {
            if(_Audioclip != null)
                audioSource.clip = _Audioclip;
            if (_AudioMixerGroup != null)
                audioSource.outputAudioMixerGroup = _AudioMixerGroup;
            audioSource.mute = _Mute;
            audioSource.bypassEffects = _BypassEffects;
            audioSource.bypassListenerEffects = _BypassListenerEffects;
            audioSource.bypassReverbZones = _BypassRevevbZones;
            audioSource.playOnAwake = _PlayOnAwake;
            audioSource.loop = _Loop;
            audioSource.priority = _Priority;
            audioSource.volume = _Volume;
            audioSource.pitch = _Pitch;
            audioSource.panStereo = _StereoPan;
            audioSource.spatialBlend = _SpatialBlend;
            audioSource.reverbZoneMix = _ReverbZoneMix;
            audioSource.dopplerLevel = _DopplerLevel;
            audioSource.spread = _Spread;
            audioSource.rolloffMode = _VolumeRolloff;
            audioSource.minDistance = _MinDistance;
            audioSource.maxDistance = _MaxDistance;
            audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, _RolloffCustomCurve);
        }
    }

    /*
    public AudioSource GetAudioSource(int num)
    {
        AudioSource audioSource = new();
        audioSource.clip = audioValue[num]._Audioclip;
        return audioSource;
    }
    */
    public AudioSourceListClass Clone()
    {
        return Instantiate(this);
    }
}
