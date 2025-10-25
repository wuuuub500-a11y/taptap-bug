using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private List<EventInstance> _allEventInstances;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ensure this instance persists across scenes
        }
        else
        {
            Debug.LogError("Multiple instances of AudioManager detected!");
            Destroy(gameObject);
        }

        // Initialize the list to track active looped sounds (if not already initialized)
        if (_allEventInstances == null)
        {
            InitializeAllEventInstances();
        }
    }

    // Play a one-shot sound event at the specified world position
    public void PlayOneShot(EventReference soundEvent, Vector3 worldPosition)
    {
        RuntimeManager.PlayOneShot(soundEvent, worldPosition);
    }

    // Update the playback state of a looped sound event based on a boolean flag
    public void UpdateLoopSound(EventInstance eventInstance, bool shouldPlay)
    {
        if (shouldPlay)
        {
            PLAYBACK_STATE playbackState;
            eventInstance.getPlaybackState(out playbackState);
            if (playbackState != PLAYBACK_STATE.PLAYING)
            {
                eventInstance.start();
            }
        }
        else
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    // Create and return an EventInstance for a given sound event. Do this for looped sounds or sounds that need to be controlled.
    public EventInstance CreateInstance(EventReference soundEvent)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(soundEvent);
        _allEventInstances.Add(eventInstance);
        return eventInstance;
    }

    // Initialize the list to track active looped sounds
    public void InitializeAllEventInstances()
    {
        _allEventInstances = new List<EventInstance>();
    }

    // Clear and stop all active event instances
    public void ClearAllEventInstances()
    {
        foreach (var instance in _allEventInstances)
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
        }
    }

    // Set the parameter of a certain instance in the FMOD Studio system
    public void SetParameter(EventInstance eventInstance, string parameterName, float value, float maxValue = 1f, bool shouldChangeInstantly = false)
    {
        float targetValue = value / maxValue;
        eventInstance.setParameterByName(parameterName, targetValue, shouldChangeInstantly);
    }

    // Set the parameter of the FMOD Studio system (global parameter)
    public void SetParameter(string parameterName, float value, float maxValue = 1f, bool shouldChangeInstantly = false)
    {
        float targetValue = value / maxValue;
        RuntimeManager.StudioSystem.setParameterByName(parameterName, targetValue, shouldChangeInstantly);
    }

    // Get the current value of the parameter of a certain instance in the FMOD Studio system
    public float GetParameter(EventInstance eventInstance, string parameterName)
    {
        float value;
        eventInstance.getParameterByName(parameterName, out value);
        return value;
    }

    // Get the current value of the parameter of the FMOD Studio system (global parameter)
    public float GetParameter(string parameterName)
    {
        float value;
        RuntimeManager.StudioSystem.getParameterByName(parameterName, out value);
        return value;
    }

    // Enum representing different music states for background music
    //public void SetState(EventInstance eventInstance, MusicState musicState)
    //{
    //    eventInstance.setParameterByName("MusicState", (float) musicState);
    //}

    // Enum representing different material states for sound effects
    //public void SetState(EventInstance eventInstance, MaterialType materialType)
    //{
    //    eventInstance.setParameterByName("MaterialType", (float)materialType);
    //}

    // Set the volume of a specific bus in the FMOD Studio system
    public void SetBusVolume(string busPath, float volume)
    {
        Bus bus = RuntimeManager.GetBus(busPath);
        bus.setVolume(volume);
    }

    // Get the current volume of a specific bus in the FMOD Studio system
    public float GetBusVolume(string busPath)
    {
        float volume;
        Bus bus = RuntimeManager.GetBus(busPath);
        bus.getVolume(out volume);
        return volume;
    }
}

