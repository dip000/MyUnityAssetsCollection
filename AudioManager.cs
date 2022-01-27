using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {
	
	public Transform listenerTarget;

	string sceneName;

	public static AudioSource sfx2DSource;
	AudioSource musicSource;

	Transform audioListener;
	Transform playerT;

	public static SoundLibrary library;

	void Awake() {

		library = GetComponent<SoundLibrary> ();
		
		//Music Source
		GameObject newMusicSource = new GameObject ("Music source");
		musicSource = newMusicSource.AddComponent<AudioSource>();
		newMusicSource.transform.parent = transform;
		
		//2D SFX source
		GameObject newSfx2Dsource = new GameObject ("2D sfx source");
		sfx2DSource = newSfx2Dsource.AddComponent<AudioSource> ();
		newSfx2Dsource.transform.parent = transform;
		
		//Source reference location
		audioListener = FindObjectOfType<AudioListener> ().transform;
		audioListener.position = listenerTarget.position;
	}


	// MUSIC LOOP /////////////////////
	void Start() {
		Invoke ("MusicLoop", .2f);
	}

	void MusicLoop() {
		AudioClip clipToPlay = library.mainTheme;

		if (clipToPlay != null) {
			musicSource.clip = clipToPlay;
			musicSource.Play ();

			Invoke ("MusicLoop", clipToPlay.length);
		}
	}
	/////////////////////////////////////



	// SOUNDS ///////////////////////////
	public void PlaySound(AudioClip clip, Vector3 pos) {
		if (clip != null) {
			AudioSource.PlayClipAtPoint (clip, pos, 1.0f);
		}
	}

	public void PlaySound(string soundName, Vector3 pos) {
		PlaySound (library.GetClipFromName (soundName), pos);
	}

	public static void PlaySound2D(string soundName) {
		//sfx2DSource.clip = 
		sfx2DSource.PlayOneShot (library.GetClipFromName (soundName), 1.0f);
	}
	/////////////////////////////////////

}
