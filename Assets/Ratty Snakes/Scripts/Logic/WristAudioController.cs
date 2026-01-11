using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class WristAudioController : MonoBehaviour
{
    [Header("Referencias")]
    public AudioMixer audioMixer; // Arrastra tu AudioMixer aquí

    [Header("Sliders UI")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Nombres Parámetros Mixer")]
    // Asegúrate de que coincidan con los "Exposed Parameters" del Mixer
    private string paramMaster = "MasterVol";
    private string paramMusic = "MusicVol";
    private string paramSFX = "SFXVol";

    void Start()
    {
        // 1. Inicializar los Sliders con el valor actual del volumen
        // (Para que estén sincronizados con lo que configuraste en el Main Menu)
        InitializeSlider(paramMaster, masterSlider);
        InitializeSlider(paramMusic, musicSlider);
        InitializeSlider(paramSFX, sfxSlider);

        // 2. Suscribirnos a los eventos de cambio
        // Esto equivale a arrastrar la función en el inspector "On Value Changed"
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    // --- FUNCIONES QUE CAMBIAN EL VOLUMEN ---

    public void SetMasterVolume(float sliderValue)
    {
        SetVolume(paramMaster, sliderValue);
    }

    public void SetMusicVolume(float sliderValue)
    {
        SetVolume(paramMusic, sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        SetVolume(paramSFX, sliderValue);
    }

    // --- LÓGICA INTERNA (MATEMÁTICAS) ---

    // Convierte el 0-1 del Slider a -80dB a 0dB del Mixer
    private void SetVolume(string parameterName, float sliderValue)
    {
        // Si el slider está al mínimo (0), ponemos -80dB para silenciar totalmente
        if (sliderValue <= 0.001f)
        {
            audioMixer.SetFloat(parameterName, -80f);
        }
        else
        {
            // Fórmula logarítmica para que el cambio de volumen se sienta natural al oído
            float dbValue = Mathf.Log10(sliderValue) * 20;
            audioMixer.SetFloat(parameterName, dbValue);
        }
    }

    // Lee los dB del mixer y los convierte a posición del slider 0-1
    private void InitializeSlider(string parameterName, Slider slider)
    {
        if (slider == null) return;

        float dbValue;
        bool result = audioMixer.GetFloat(parameterName, out dbValue);

        if (result)
        {
            // Operación inversa: De dB a Linear
            float linearValue = Mathf.Pow(10, dbValue / 20);
            slider.value = linearValue;
        }
        else
        {
            // Si falla, asumimos volumen máximo
            slider.value = 1f;
        }
    }
}