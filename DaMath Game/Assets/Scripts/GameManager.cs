using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    /*public GameObject loadingScreen;
    public Slider loadingBar;*/
    // public static bool GameIsPaused;

    public GameObject pauseMenuUI;
    public GameObject optionPanel;
    public GameObject aiPanel;
    public GameObject aiUI;
    public GameObject gameModePanel;
    public GameObject backPanel;
    public GameObject unMuteSFX;
    public GameObject muteSFX;
    public Text p1ScoreText;
    public Text p2ScoreText;
    


    public Animator animPause;

    public static int algo;
    public static GameManager Instance
    {
        set;
        get;
    }
    public void ToggleSFX(bool muted)
    {
        if (muted)
        {
            MuteSFX();
        }
        else
        {
            UnmuteSFX();
        }

    }

    public void MuteSFX()
    {
        muteSFX.SetActive(false);
        unMuteSFX.SetActive(true);
        AudioManager.instance.MuteAudio();
        //FindObjectOfType<SFXManager>().MuteSFX(Master);
    }

    public void UnmuteSFX()
    {
        muteSFX.SetActive(true);
        unMuteSFX.SetActive(false);
        AudioManager.instance.UnmuteAudio();
        
    }
    public void ButtonSFX()
    {
        FindObjectOfType<AudioManager>().Play("Button");
    }

  
    public void PauseBtnSFX()
    {
        FindObjectOfType<AudioManager>().Play("PauseBtn");
    }

    public void KeyPressSFX()
    {
        FindObjectOfType<AudioManager>().Play("KeyPress");
    }

    public void StopBGMusic()
    {
        FindObjectOfType<AudioManager>().StopPlaying("ThemeBG");
    }

    public void PauseBGMusic()
    {
        FindObjectOfType<AudioManager>().StopPlaying("GameBGMusic");
        FindObjectOfType<AudioManager>().Play("PauseBGMusic");
    }
    public void GameBGMusic()
    {
        FindObjectOfType<AudioManager>().StopPlaying("PauseBGMusic");
        FindObjectOfType<AudioManager>().Play("GameBGMusic");
    }

    public void pauseBtn(bool gameIsPaused)
    {
        
        if (gameIsPaused)
        {
            pause();
            //FindObjectOfType<AudioManager>().StopPlaying("GameBGMusic");

        }
        else
        {
            resume();
            //FindObjectOfType<AudioManager>().Play("GameBGMusic");

        }
    }

    public void ThemeBGMusic()
    {
        FindObjectOfType<AudioManager>().StopPlaying("ThemeBG");
    }
    public void pause()
    {
        Time.timeScale = 0;
        PauseBGMusic();
        pauseMenuUI.SetActive(true);
    }

    public void resume()
    {
        Time.timeScale = 1f;
        GameBGMusic();
        StartCoroutine(PlayAndDisappear("Resume"));
    }

    public void StopGameBG()
    {
        FindObjectOfType<AudioManager>().StopPlaying("PauseBGMusic");
        FindObjectOfType<AudioManager>().StopPlaying("GameBGMusic");
    }

    private void Awake()
    {
        Time.timeScale = 1f;
        //GameIsPaused = false;

        //Instance = this;
        //hostMenu.SetActive(false);
        //connectMenu.SetActive(false);
        //DontDestroyOnLoad(gameObject);
    }

    public void BacktoMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void PlayBtn()
    {
        SceneManager.LoadScene(1);
        // StartCoroutine(LoadSceneAsynchronously(levelindex));
    }

    public void SameDeviceBtn()
    {
        SceneManager.LoadScene(4);
    }

    public void TutorialBtn()
    {
        SceneManager.LoadScene(6);
    }

    public void ExitBtn()
    {
        Application.Quit();
        Debug.Log("Quitted the game!");
    }

    public void Easy()
    {
        algo = 1;
        SceneManager.LoadScene("GameEasy");

    }
    public void Medium()
    {
        algo = 2;
        SceneManager.LoadScene("GameMedium");

    }
    public void Hard()
    {
        algo = 3;
        SceneManager.LoadScene("GameHard");

    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
        
    }

    public void MainMenu()
    {
        StopGameBG();
        FindObjectOfType<AudioManager>().Play("ThemeBG");
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        SceneManager.LoadScene(0);
        
    }
    IEnumerator PlayAndDisappear(string ani)
    {
        if (ani == "Resume")
        {
            animPause.SetTrigger("Closing");

            yield return new WaitForSeconds(1f);
            pauseMenuUI.SetActive(false);
        }

    }

    public void OptionPanelActivate()
    {
        optionPanel.SetActive(true);
    }

    public void OptionPanelDeActivate()
    {
        optionPanel.SetActive(false);
    }

    public void AIPanelActivate()
    {
        aiPanel.SetActive(true);
        gameModePanel.SetActive(false);
        backPanel.SetActive(false);
    }

    public void AIPanelDeactivate()
    {
        aiPanel.SetActive(false);
        backPanel.SetActive(true);
        gameModePanel.SetActive(true);
    }
    
}

