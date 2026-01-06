using UnityEngine;
using UnityEngine.Audio; // Necesario para el Mixer
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [Header("Referencias")]
    public AudioMixer mainMixer;

    // Nombres exactos que pusiste en "Exposed Parameters"
    const string MIXER_MASTER = "MasterVol";
    const string MIXER_MUSIC = "MusicVol";
    const string MIXER_SFX = "SFXVol";

    // Llamaremos a estas funciones desde el evento "On Value Changed" del Slider

    public void SetMasterVolume(float sliderValue)
    {
        // Convertimos escala logarítmica (0.0001 a 1) a Decibelios (-80 a 0)
        mainMixer.SetFloat(MIXER_MASTER, Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20);
    }

    public void SetMusicVolume(float sliderValue)
    {
        mainMixer.SetFloat(MIXER_MUSIC, Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20);
    }

    public void SetSFXVolume(float sliderValue)
    {
        mainMixer.SetFloat(MIXER_SFX, Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20);
    }

    [Header("Test de Audio")]
    public AudioSource sfxTestAudioSource; // Arrastra el AudioSource con Output: SFX
    public AudioClip sonidoTest; // Tu "Quack"

    private float ultimoTiempoSonido = 0f;

    // Conecta esta función al evento del Slider también (además del SetSFXVolume)
    public void ProbarSonidoSFX()
    {
        // Solo permite que suene cada 0.15 segundos como máximo
        if (Time.time - ultimoTiempoSonido > 0.30f)
        {
            sfxTestAudioSource.PlayOneShot(sonidoTest);
            ultimoTiempoSonido = Time.time;
        }
    }
}