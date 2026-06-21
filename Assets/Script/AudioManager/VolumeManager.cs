using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    [Header("Sound Effect")]
    public Image sfxFill;
    public GameObject sfxMute;
    public Slider sfxVolume;

    [Header("Background Music")]
    public Image bgmFill;
    public GameObject bgmMute;
    public Slider bgmVolume;
    public float initialVolume;

    public void sfxChange(Slider slider)
    {
        sfxFill.fillAmount = slider.value;
        AudioManager.instance.sfxSource.volume = slider.value;
    }

    public void muteSfx()
    {
        sfxMute.SetActive(!AudioManager.instance.sfxSource.mute);
        AudioManager.instance.sfxSource.mute = !AudioManager.instance.sfxSource.mute;
    }

    public void bgmChange(Slider slider)
    {
        bgmFill.fillAmount = slider.value;
        AudioManager.instance.bgmSource.volume = slider.value * initialVolume;
    }

    public void muteBgm()
    {
        bgmMute.SetActive(!AudioManager.instance.bgmSource.mute);
        AudioManager.instance.bgmSource.mute = !AudioManager.instance.bgmSource.mute;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sfxFill.fillAmount = AudioManager.instance.sfxSource.volume;
        sfxMute.SetActive(AudioManager.instance.sfxSource.mute);
        sfxVolume.value = AudioManager.instance.sfxSource.volume;

        if(AudioManager.instance.bgmSource.volume > initialVolume)
        {
            AudioManager.instance.bgmSource.volume = initialVolume;
        }
        bgmFill.fillAmount = AudioManager.instance.bgmSource.volume / initialVolume;
        bgmMute.SetActive(AudioManager.instance.bgmSource.mute);
        bgmVolume.value = AudioManager.instance.bgmSource.volume / initialVolume;


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
