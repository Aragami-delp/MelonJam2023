using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.DefaultInputActions;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private List<SceneLevel> _levelNames;
    [SerializeField] private List<AudioClipReference> _audioClipReference;
    [SerializeField] private AudioSource _audioSourcePrefab;

    private Dictionary<AUDIOTYPE, AudioSource> _audioSources = new Dictionary<AUDIOTYPE, AudioSource>();

    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);

        CreateAudioSources();
        PlaySound(AUDIOTYPE.MUSIC);
    }

    [ContextMenu("ReloadScene")]
    public void ReloadScene()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(LEVEL nextLevel = LEVEL.NEXT)
    {
        if (nextLevel == LEVEL.NEXT)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        foreach (SceneLevel sceneLevel in _levelNames)
        {
            if (sceneLevel.Level == nextLevel)
            {
                try
                {
                    SceneManager.LoadScene(sceneLevel.SceneName);
                }
                catch
                {
                    Debug.LogError("Couldn't load level: " + nextLevel.ToString());
                }
            }
        }
    }

    public void CloseGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void CreateAudioSources()
    {
        foreach (AudioClipReference audioReference in _audioClipReference)
        {
            if (audioReference.Reference != null)
            {
                AudioSource newAudioSource = Instantiate(_audioSourcePrefab, this.transform);
                newAudioSource.clip = audioReference.Reference;
                newAudioSource.gameObject.name = audioReference.Reference?.name ?? "No Audio";
                _audioSources.Add(audioReference.Audio, newAudioSource);
            }
        }
    }

    public void PlaySound(AUDIOTYPE audioType)
    {
        try
        {
            try
            {
                if (!_audioSources[audioType].isPlaying)
                {
                    _audioSources[audioType].Play();
                }
            }
            catch (KeyNotFoundException)
            {
                Debug.LogWarning("Couldn't find audio: " + audioType.ToString());
            }
        }
        catch (Exception)
        {
            Debug.LogError("Couldn't play audio: " + audioType.ToString());
        }
    }
}
