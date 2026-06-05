using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    public AudioClip MainMenuMusic;
    public AudioClip BattleMusic;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (instance != this)
        {
            Destroy(gameObject);
        }

        if (instance == this)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        PlayMainMenuMusic();
    }

    public void PlaySfx(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayBattleMusic()
    {
        bgmSource.clip = BattleMusic;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayMainMenuMusic()
    {
        bgmSource.clip = MainMenuMusic;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
