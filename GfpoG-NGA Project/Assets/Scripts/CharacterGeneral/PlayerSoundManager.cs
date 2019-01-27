using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip m_JumpClip;
    [SerializeField] private AudioClip m_LandClip;
    [SerializeField] private AudioClip m_DeathClip;

    private AudioSource m_AudioSource;                    // the audio source for playing audio

    private void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        CharacterController2D.OnDeathEvent.AddListener(delegate { DeathSound(); });
        CharacterController2D.OnJumpEvent.AddListener(delegate { JumpSound(); });
        CharacterController2D.OnLandEvent.AddListener(delegate { LandSound(); });
    }

    private void Play()
    {
        m_AudioSource.Play();
    }

    private void JumpSound()
    {
        m_AudioSource.clip = m_JumpClip;
        Play();
    }

    private void LandSound()
    {
        m_AudioSource.clip = m_LandClip;
        Play();
    }

    private void DeathSound()
    {
        m_AudioSource.clip = m_DeathClip;
        Play();
    }
}
