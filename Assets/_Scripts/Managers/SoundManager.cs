using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	[SerializeField]
	AudioSource effectsSource;

	[SerializeField]
	AudioSource musicSource;

	[SerializeField]
	AudioClip[] characterSounds;

	public Vector2 pitchRange = Vector2.zero;

	public static SoundManager SharedInstance;

	private void Awake()
	{
		if (SharedInstance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			SharedInstance = this;
		}

		//para que el manager sobreviva de escena a escena
		DontDestroyOnLoad(gameObject);
	}

	public void PlaySound(AudioClip clip, float pitch=1.0f)
	{
		effectsSource.pitch = pitch;
		effectsSource.Stop();
		effectsSource.clip = clip;
		effectsSource.Play();
	}

	public void PlayMusic(AudioClip clip)
	{
		musicSource.Stop();
		musicSource.clip = clip;
		musicSource.Play();
	}

	//el params haría referencia como apuntadores
	public void RandomCharacterSoundEffect()
	{
		RandomSoundEffect(characterSounds);
	}

	//el params haría referencia como apuntadores
	void RandomSoundEffect(params AudioClip[] clips)
	{
		int index = Random.Range(0, clips.Length);
		float pitch = Random.Range(pitchRange.x, pitchRange.y);

		//effectsSource.pitch = pitch;
		PlaySound(clips[0],pitch);
	}
}
