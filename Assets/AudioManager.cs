using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;
    [Header("Audio Clips")]
    public AudioClip lobby_theme;
    public AudioClip lv1_theme;
    public AudioClip game_over;
    public AudioClip lv_completed;
    public AudioClip jump;
    public AudioClip power_up;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        musicSource.clip = lobby_theme;
        musicSource.Play();
    }

    // Update is called once per frame
    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
