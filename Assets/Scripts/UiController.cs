using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UiController : MonoBehaviour
{
    GameController gameController;

    public GameObject startUi, mainUi, levelCompleteUi, gameOverUi;

    public Image powerupBar, levelBarFill, levelBarFillBorder;

    bool filling;

    public TextMeshProUGUI currentLevel, nextLevel, highScoreText, diamondsNoText, lastScoreText;
    public TextMeshPro scoreText;
    int currentLevelNo, score, highScore, diamondsNo, tmp;

    [HideInInspector]
    public bool respawningNewTarget;
    [HideInInspector]
    public float powerupValue;

    private void Awake()
    {
        gameController = GetComponent<GameController>();
        UpdateLevelText();
    }

    private void Start()
    {
        UpdateLevelProgressBar(0, 0);
        
        score = PlayerPrefs.GetInt("score", 0);
        highScore = PlayerPrefs.GetInt("highScore", 0);
        diamondsNo = PlayerPrefs.GetInt("diamonds", 0);

        highScoreText.text = "Best\n" + highScore;
        diamondsNoText.text = diamondsNo.ToString();
        scoreText.text = score.ToString();

        ActivateMainUi(false);
        ActivateLevelCompleteUi(false);
        ActivateGameOverUi(false);
        ActivateStartUi(true);
    }


    public void ActivateStartUi(bool value)
    {
        if(startUi.activeInHierarchy != value)
            startUi.SetActive(value);
    }
    public void ActivateMainUi(bool value)
    {
        if (mainUi.activeInHierarchy != value)
            mainUi.SetActive(value);

        if (startUi.activeInHierarchy != false)
            startUi.SetActive(false);

        if (gameOverUi.activeInHierarchy != false)
            gameOverUi.SetActive(false);
    }
    public void ActivateLevelCompleteUi(bool value)
    {
        if (levelCompleteUi.activeInHierarchy != value)
            levelCompleteUi.SetActive(value);
        ActivateMainUi(false);
    }
    public void ActivateGameOverUi(bool value)
    {
        if (gameOverUi.activeInHierarchy != value)
            gameOverUi.SetActive(value);
        ActivateMainUi(false);
    }

    //NEW FUNCTION
    public void SetTargetNoForLevel()
    {
        int t = 0;
        if (currentLevelNo <= 3)
            t = 1;
        else if (currentLevelNo <= 6)
            t = 2;
        else t = 3;

        gameController.ChooseTargets(t);
    }

    private void Update()
    {
        if (powerupBar.fillAmount < 1 && !respawningNewTarget)
        {
            if (filling)
            {
                powerupValue = Mathf.Clamp01(powerupValue += Time.deltaTime / 3);
            }
            else
            {
                powerupValue = Mathf.Clamp01(powerupValue -= Time.deltaTime / 8);
            }
            powerupBar.fillAmount = powerupValue;
        }
        else if (powerupBar.fillAmount == 1)
        {
            PlayPowerupButtonAnim(true);
        }
    }
    
    public void NearMiss(GameObject g)
    {
        g.GetComponent<TextMeshPro>().text = "Near miss +" + currentLevelNo;
        UpdateScore(currentLevelNo);

        if (powerupValue == 1) return;

        if (powerupValue + 0.2f > 1)
            powerupValue = 1;
        else powerupValue += 0.2f;
        powerupBar.fillAmount = powerupValue;

    }
    
    public void UpdateLevelProgressBar(int totalPartsCount, int totalPartsPainted)
    {
        float f = 0;
        if(tmp == 0)
            tmp = totalPartsCount;
        try
        {
            f = (float)(totalPartsCount - totalPartsPainted) / totalPartsCount;
            UpdateScore(tmp - totalPartsPainted);
            tmp = totalPartsPainted;
        }
        catch (DivideByZeroException)
        {
            f = 0;
        }
        catch(Exception e)
        {
            print("Exception thrown: " + e);
        }
        levelBarFill.fillAmount = f;
        levelBarFillBorder.fillAmount = f;
        
    }

    void UpdateScore(int score)
    {
        this.score += score;
        scoreText.text = this.score.ToString();
    }

    public void UpdateLevelText()
    {
        currentLevelNo = PlayerPrefs.GetInt("currentLevel", 1);
        currentLevel.text = currentLevelNo.ToString();
        nextLevel.text = (currentLevelNo + 1).ToString();

        tmp = 0;
    }

    public void GoingToNextLevel()
    {
        PlayerPrefs.SetInt("currentLevel", currentLevelNo + 1);
        PlayerPrefs.SetInt("score", score);
        diamondsNo += 5;
        levelCompleteUi.GetComponent<Restarter>().SetDiamonds(diamondsNo);
        PlayerPrefs.SetInt("diamonds", diamondsNo);
        PlayerPrefs.Save();
    }

    public void GameOver()
    {
        if (score > highScore)
        {
            PlayerPrefs.SetInt("highScore", score);
        }
        PlayerPrefs.DeleteKey("score");

        lastScoreText.text = "Last score\n" + score;
        gameOverUi.SetActive(true);
        mainUi.SetActive(false);
        //ActivateGameOverUi(true);

        PlayerPrefs.Save();
    }

    public void FillPowerupBar(bool filling)
    {
        this.filling = filling;
    }

    public void ShootPowerup ()
    {
        if (powerupBar.fillAmount != 1) return;
        powerupValue = 0; powerupBar.fillAmount = 0;
        gameController.ShootSuperBullet();
        PlayPowerupButtonAnim(false);
    }

    public void PlayPowerupButtonAnim(bool filled)
    {
        if (filled) {
            try{ if (powerupBar.transform.parent.GetComponentInParent<Animator>().GetBool("btn_powerupFilled") == false)
                    powerupBar.transform.parent.GetComponentInParent<Animator>().SetBool("btn_powerupFilled", true);
            }catch (Exception) { }
        }
        else // if(buttonPressed)
        {
            powerupBar.transform.parent.GetComponentInParent<Animator>().SetBool("btn_powerupFilled", false);
        }
    }
}
