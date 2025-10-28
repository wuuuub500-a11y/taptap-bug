using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    EventInstance _musicInstance;
    [SerializeField] MusicState musicState;

    void Start()
    {
        _musicInstance = AudioManager.Instance.CreateInstance(FMODEvents.Instance.MUSIC_Chapter_1);
        AudioManager.Instance.UpdateLoopSound(_musicInstance, true);
    }

    public void ChangeMusicState()
    {
        AudioManager.Instance.SetState(musicState);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Plane_Fly_By, transform.position);
    }
}
