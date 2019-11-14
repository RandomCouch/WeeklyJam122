using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip _music;

    [SerializeField]
    private AudioClip _punchFX;

    [SerializeField]
    private AudioClip _hitFX;

    [SerializeField]
    private AudioClip _enemyHitFX;

    [SerializeField]
    private AudioClip _enemyDieFX;

    [SerializeField]
    private AudioSource _fxSource;

    public static AudioManager Instance;

    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    
    public void PlayPunchFX()
    {
        _fxSource.clip = _punchFX;
        _fxSource.Play();
    }

    public void PlayHitFX()
    {
        _fxSource.clip = _hitFX;
        _fxSource.Play();
    }

    public void PlayEnemyHitFX()
    {
        _fxSource.clip = _enemyHitFX;
        _fxSource.Play();
    }

    public void PlayEnemyDieFX()
    {
        _fxSource.clip = _enemyDieFX;
        _fxSource.Play();
    }
    
}
