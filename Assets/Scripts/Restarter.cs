using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Restarter : MonoBehaviour
{
    public TextMeshProUGUI diamondsTxt;

    int diamonds, tmpDiamonds;

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetDiamonds(int diamonds)
    {
        this.diamonds = diamonds;
        tmpDiamonds = diamonds - 5;
        diamondsTxt.text = tmpDiamonds.ToString();
    }

    public void UpdateDiamondsText()
    {
        StartCoroutine(UpdateText());
    }

    IEnumerator UpdateText()
    {
        print("tmp, normal "+tmpDiamonds + " " + diamonds);
        while(tmpDiamonds < diamonds)
        {
            tmpDiamonds++;
            diamondsTxt.text = tmpDiamonds.ToString();
            print("tmp, real " + tmpDiamonds + " " + diamonds);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
