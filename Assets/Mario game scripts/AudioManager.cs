using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // public static AudioManager Instance { get; private set; }
    
    // [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private AudioClip powerupSound;
    [SerializeField] private AudioClip deathSound;
    
    public AudioSource gameAudio;
    public AudioSource audioSource;

    // private void Awake()
    // {
    //     if (Instance == null)
    //     {
    //         Instance = this;
    //         DontDestroyOnLoad(gameObject);
    //         // audioSource = gameObject.AddComponent<AudioSource>();
    //         // gameAudio = gameObject.AddComponent<AudioSource>();
    //         // gameAudio.loop = true;
    //     }
    //     else
    //     {
    //         Destroy(gameObject);
    //     }
    // }

    void Start()
    {
        gameAudio.Play();
    }

    public void PlayCoinSound()
    {
        PlaySound(coinSound);
    }
    
    public void PlayPowerupSound()
    {
        PlaySound(powerupSound);
    }

    public void PlayDeathSound()
    {
        PlaySound(deathSound);
    }
    
    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
            Debug.Log("Playing sound: " + clip.name);
        }
    }

    // public void PlayGameMusic()
    // {
    //     if(gameAudio != null && gameMusic != null)
    //     {
    //         gameAudio.clip = gameMusic;
    //         gameAudio.Play();
    //     }
    // }
}