using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    UiController ui;

    public GameObject bullet, superBulletPrefab, superBulletHitParticles, bulletHitParticles, backgroundParticles, deltaAngleFinder, bulletDestroyer, bulletHitAnim, nearMissAnim;
    public GameObject[] targets, enemies;
    GameObject[] targetParts;
    public Color[] targetPartColors;
    public Transform spawnPosition;
    public float throwForce, fireRate;
    public RuntimeAnimatorController[] animators;

    GameObject player, enemy, target, currentTargetPart;
    int targetNoForThisLevel, targetNoSpawned, targetInstantiated, bulletsShot, partsPainted, totalPartsPainted, partsCount, totalPartsCount, gamesPlayed, tmpGamesPlayed, partAnimationNo, lastEnemy;
    int tmpTarget1, tmpTarget2, tmpTarget3;
    Color paintColor1, paintColor2;
    bool rotate, firstColor;
    private float angleToRotate, lastShot;

    [HideInInspector]
    public bool throwing, allowedFromSuperball = true;

    private void Awake()
    {
        ui = GetComponent<UiController>();
        Time.timeScale = 1;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        //PlayerPrefs.DeleteKey("partAnimationNo");
        //PlayerPrefs.DeleteAll();
        lastEnemy = PlayerPrefs.GetInt("lastEnemy", 0);
        tmpGamesPlayed = PlayerPrefs.GetInt("tmpGamesPlayed");
        partAnimationNo = PlayerPrefs.GetInt("partAnimationNo", 0);
        player = GameObject.FindGameObjectWithTag("Player");
        UpdateGamesPlayed();
        targetInstantiated = PlayerPrefs.GetInt("target", 0); //UnityEngine.Random.Range(0, animators.Length);
        if(targetInstantiated >= targets.Length)
            targetInstantiated = 0;
        InstantiateNewTargetForCurrentLevel();
        ui.SetTargetNoForLevel();
        throwing = false;
        
        //backgroundParticles.transform.GetChild(PlayerPrefs.GetInt("backParticles", 0)).gameObject.SetActive(true);
    }

    void Update()
    {
#if UNITY_EDITOR
        //Works For devices with mouse
        if (Input.GetMouseButton(0))
        {
            // Check if the mouse was clicked over a UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
        }
#endif

#if UNITY_ANDROID
        //works for devices with touch
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
            {
                // Check if finger is over a UI element
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    return;
                }
            }
        }
#endif

#if UNITY_IOS
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
            {
                // Check if finger is over a UI element
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    return;
                }
            }
        }
#endif
        //Auto-Fires $fireRate/seconds$ bullets while touching screen
        if (Input.GetKey(KeyCode.Mouse0) && Time.time - lastShot > 1 / fireRate && throwing && allowedFromSuperball)
        {
            lastShot = Time.time;
            ThrowBullet();
        }
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            ui.ActivateMainUi(true);

            ui.FillPowerupBar(true);
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            ui.FillPowerupBar(false);
        }
    }

    public void SetAnimatorsOfTargetParts()
    {
        try
        {
            bool b = true;
            int nrPartAnim = UnityEngine.Random.Range(0, targets.Length - 1);
            //if (partAnimationNo + targetNoSpawned >= animators.Length)
            //    partAnimationNo = (partAnimationNo + targetNoSpawned - animators.Length - 1);
            //else partAnimationNo++;
            
            foreach (var item in targetParts)
            {
                if (item.GetComponentInParent<Animator>() == null) item.transform.parent.gameObject.AddComponent<Animator>();
                item.transform.parent.gameObject.GetComponent<Animator>().runtimeAnimatorController = animators[partAnimationNo]; 
                if(b)
                item.transform.parent.gameObject.GetComponent<Animator>().SetTrigger("tr_1");
                else item.transform.parent.gameObject.GetComponent<Animator>().SetTrigger("tr_2");
                b = !b;
            }

        }
        catch (Exception e) { print(e); }
    }
    
        //NEW FUNCTION
    public void ChooseTargets(int targetsNo)
    {
        targetNoForThisLevel = targetsNo;
        totalPartsPainted = totalPartsCount = targetsNo * partsCount;
        
        //int tgt = PlayerPrefs.GetInt("lastTarget", 0);
        //target = targets[tgt];
        //if ( tgt == targets.Length - 1)
        //{
        //    PlayerPrefs.SetInt("lastTarget", 0);
        //}
        //else
        //{
        //    PlayerPrefs.SetInt("lastTarget", tgt + 1);
        //}
        

        //PlayerPrefs.Save();
    }

    private void InstantiateNewTarget()
    {
        if (tmpGamesPlayed + 2 >= targetPartColors.Length)
            tmpGamesPlayed = 0;
        else tmpGamesPlayed += 2;

        int colorAtIndex = tmpGamesPlayed;
        paintColor1 = targetPartColors[colorAtIndex];
        paintColor2 = colorAtIndex % 2 == 1 ? targetPartColors[colorAtIndex - 1] : targetPartColors[colorAtIndex + 1];
        partsCount = partsPainted = target.transform.childCount;
        targetParts = new GameObject[partsCount];
        for (int i = 0; i < partsCount; i++)
        {
            targetParts[i] = target.transform.GetChild(i).GetChild(0).gameObject;
        }
        SetAnimatorsOfTargetParts();
        currentTargetPart = targetParts[targetParts.Length - 1];

        try{
            enemy.GetComponent<Animator>().SetTrigger("tr_destroyEnemy");
            Destroy(enemy, 1f);}catch (Exception) {
        }

        if (lastEnemy >= enemies.Length - 1)
        {
            ChooseATarget();
        }
        else enemy = Instantiate(enemies[lastEnemy++]); //from array list in some order
        
        //EnemiesState(false);
    }

    void ChooseATarget()
    {
        if (PlayerPrefs.GetInt("chooseNew", 0) == 0)
        {
            tmpTarget1 = UnityEngine.Random.Range(5, enemies.Length);
            do
            {
                tmpTarget2 = UnityEngine.Random.Range(5, enemies.Length);
            } while (tmpTarget1 == tmpTarget2);
            do
            {
                tmpTarget3 = UnityEngine.Random.Range(5, enemies.Length);
            } while (tmpTarget3 == tmpTarget2 || tmpTarget3 == tmpTarget1);

            PlayerPrefs.SetInt("tmpTarget1", tmpTarget1);
            PlayerPrefs.SetInt("tmpTarget2", tmpTarget2);
            PlayerPrefs.SetInt("tmpTarget3", tmpTarget3);
            PlayerPrefs.SetInt("chooseNew", 1);

            PlayerPrefs.Save();
        } else if (PlayerPrefs.GetInt("chooseNew", 0) == 1)
        {
            tmpTarget1 = PlayerPrefs.GetInt("tmpTarget1");
            tmpTarget2 = PlayerPrefs.GetInt("tmpTarget2");
            tmpTarget3 = PlayerPrefs.GetInt("tmpTarget3");
        }
        switch (targetNoSpawned)
        {
            case 0:
                enemy = Instantiate(enemies[tmpTarget1]);
                print("first saved enemy");
                break;
            case 1:
                enemy = Instantiate(enemies[tmpTarget2]);
                print("second saved enemy");
                break;
            case 2:
                enemy = Instantiate(enemies[tmpTarget3]);
                print("third saved enemy");
                break;
        }
    }

    public void MoveAngleFinder()
    {
        bulletDestroyer.transform.position = deltaAngleFinder.transform.position = target.transform.position;
        deltaAngleFinder.transform.LookAt(currentTargetPart.transform);
    }

    public void EnemiesState(bool isItActive)
    {
        if(isItActive)
            enemy.GetComponent<Animator>().SetTrigger("tr_playEnemy");
    }

    Color GetCurrentPaintColor()
    {
        Color c = firstColor ? paintColor1 : paintColor2;
        firstColor = !firstColor;
        return c;
    }

    void ThrowBullet()
    {
        Color thisBulletColor = GetCurrentPaintColor();
        player.GetComponent<Animator>().SetBool("shot", true);
        bulletsShot++;
        GameObject currentBullet = Instantiate(bullet, spawnPosition.position, Quaternion.identity);
        currentBullet.GetComponent<Renderer>().material.color = thisBulletColor;
        currentBullet.GetComponent<Rigidbody>().AddForce(0, 0, throwForce * 100, ForceMode.Acceleration);
        currentBullet.transform.GetChild(0).GetComponent<TrailRenderer>().startColor = thisBulletColor;
        currentBullet.transform.GetChild(0).GetComponent<TrailRenderer>().endColor = new Color(thisBulletColor.r, thisBulletColor.g, thisBulletColor.b, thisBulletColor.a / 2);
        currentBullet.transform.GetChild(0).transform.GetChild(0).GetComponent<TrailRenderer>().startColor = thisBulletColor;
        currentBullet.transform.GetChild(0).transform.GetChild(0).GetComponent<TrailRenderer>().endColor = new Color(thisBulletColor.r, thisBulletColor.g, thisBulletColor.b, thisBulletColor.a / 2);
        currentBullet.GetComponent<Light>().color = thisBulletColor;
        if (bulletsShot >= 5)
            target.GetComponent<TargetPartsController>().ChangeTriggersToColliders();
        if (bulletsShot >= partsCount)
            throwing = false;
    }

    public void BulletMissedTarget()
    {
        bulletsShot--;
        if (!throwing) throwing = true;
    }

    public void DetectTargetFullyPainted()
    {
        UpdateNextTargetParts();
        ui.UpdateLevelProgressBar(totalPartsCount, totalPartsPainted);
        if (partsPainted < 1)//nese jon ngjyros krejt
        {
            //restartButton.SetActive(true);
            if (gamesPlayed < targets.Length)
            {
                PlayerPrefs.SetInt("games played", gamesPlayed + 1);
                PlayerPrefs.Save();
            }
            UpdateGamesPlayed();
            StartCoroutine(TargetPainted(3));
            //targetPainted = true;
        }
    }

    private void UpdateNextTargetParts()
    {
        try
        {
            currentTargetPart = targetParts[--partsPainted - 1]; totalPartsPainted--;
            deltaAngleFinder.transform.LookAt(targetParts[partsPainted].transform.position);
            StopCoroutine(Rotate(angleToRotate, 1 / fireRate));
            angleToRotate = Mathf.DeltaAngle(deltaAngleFinder.transform.rotation.eulerAngles.y, 180f);
            StartCoroutine(Rotate(angleToRotate, 1 / fireRate));
        }
        catch (IndexOutOfRangeException)
        {
            print("Reached end of target parts. currentTargetPart now is null");
            currentTargetPart = null; totalPartsPainted--; partsPainted--;
        }
        catch (Exception e)
        {
            print("Caught general exception. No further actions needed. Eception: " + e);
        }
    }

    public void ShootSuperBullet()
    {
        if (!throwing)
        {
            ui.powerupBar.fillAmount = 1;
            ui.powerupValue = 1;
            return;
        }
        Color c = new Color(209, 64, 16, 255);
        allowedFromSuperball = false;
        player.GetComponent<Animator>().SetBool("shot", true);
        GameObject superBullet = Instantiate(superBulletPrefab, spawnPosition.position, Quaternion.identity);
        superBullet.GetComponent<Rigidbody>().AddForce(0, 0, throwForce * 100, ForceMode.Acceleration);
        Time.timeScale = 0.2f;
        Camera.main.GetComponent<Animator>().SetBool("bo_superShoot", true);
        
        bulletsShot += 6;
        if (bulletsShot <= 10)
            target.GetComponent<TargetPartsController>().ChangeTriggersToColliders();
    }

    public void SuperBulletHit(Vector3 superBulletHitPosition)
    {
        Camera.main.GetComponent<Animator>().SetBool("bo_superShoot", false);
        Camera.main.GetComponent<ResetCameraPositionAndRotation>().RessetPosition();
        Time.timeScale = 1f;
        print("total painted " + totalPartsPainted + " painted " + partsPainted);
        if (partsPainted != partsCount)
        {
            targetParts[partsPainted].GetComponent<Renderer>().material.color = GetCurrentPaintColor();
            targetParts[partsPainted].GetComponent<Collider>().isTrigger = true;
        }
        else
        {
            targetParts[0].GetComponent<Renderer>().material.color = GetCurrentPaintColor();
            targetParts[0].GetComponent<Collider>().isTrigger = true;
        }

        if (partsPainted > 5)
        {
            for (int i = 0; i < 5; i++)
            {
                PaintCurrentTargetPart();
                UpdateNextTargetParts();
            }
        }
        else
        {
            int tmpPartsPainted = partsPainted;
            for (int i = 1; i < tmpPartsPainted; i++)
            {
                print("parts painted " + partsPainted);
                PaintCurrentTargetPart(); 
                UpdateNextTargetParts();
            }
            if (partsPainted % 2 == 1) GetCurrentPaintColor();
            target.GetComponent<TargetPartsController>().triggerParts[0].GetComponent<Renderer>().material.color = GetCurrentPaintColor();
            target.GetComponent<TargetPartsController>().triggerParts[0].GetComponent<Collider>().isTrigger = true;
            //partsPainted--;
            //totalPartsPainted--;
        }
        InstantiateSuperBulletHitParticles(superBulletHitPosition);
        allowedFromSuperball = true;   
        print("total painted " + totalPartsPainted + " painted " + partsPainted);
    }
    void InstantiateSuperBulletHitParticles( Vector3 superBulletHitPosition)
    {
        var particles = Instantiate(superBulletHitParticles, superBulletHitPosition, Quaternion.Euler(180, 0, 0)).GetComponent<ParticleSystem>();
        //var particles2 = particles.GetComponent<ParticleSystem>().main;
        
        particles.Play();

        Destroy(particles.gameObject, .5f);
    }


    public void PaintCurrentTargetPart()
    {
        currentTargetPart.GetComponent<Renderer>().material.color = GetCurrentPaintColor();
        currentTargetPart.GetComponent<Collider>().isTrigger = true;
    }

    public void RotateTarget(bool rotate)
    {
        //if(this.rotate != rotate)
        //    this.rotate = rotate;
    }

    public void InstantiateParticlesOnBulletHit(Vector3 hitPos, Color particlesColor)
    {
        //bulletHitParticles = GetComponent<ParticleSystem>();
        var particles = Instantiate(bulletHitParticles, hitPos, Quaternion.Euler(180, 0, 0)).GetComponent<ParticleSystem>();
        var particles2 = particles.GetComponent<ParticleSystem>().main;

        particles2.startColor = particlesColor;
        particles.Play();

        Destroy(particles.gameObject, .5f);

        GameObject hitAnim = Instantiate(bulletHitAnim, new Vector3(hitPos.x, hitPos.y + 0.2f, hitPos.z), Quaternion.identity);
        hitAnim.GetComponentInChildren<SpriteRenderer>().color = particlesColor;
        Destroy(hitAnim, 1f);
    }

    public void InstantiateNearMissText(Vector3 position)
    {
        GameObject g = Instantiate(nearMissAnim, position, Quaternion.identity);
        ui.NearMiss(g);
        Destroy(g, 1f);
    }

    void DestroyAllBullets(GameObject currentBullet)
    {
        try{Destroy(GameObject.FindGameObjectWithTag("SuperBullet"));} catch (Exception) { }

        foreach (var item in GameObject.FindGameObjectsWithTag("Bullet"))
        {
            if (item != currentBullet)
            {
                Destroy(item);
                bulletsShot--;
            } else
            {
                item.GetComponent<Rigidbody>().velocity = Vector3.zero;
                item.transform.position = new Vector3(0, 0, item.transform.position.z - 0.2f);
            }
        }
    }

    public void GameOver(GameObject currentBullet)
    {
        if(Camera.main.GetComponent<Animator>().GetBool("bo_superShoot"))
            Camera.main.GetComponent<Animator>().SetBool("bo_superShoot", false);
        Time.timeScale = 1f;

        throwing = false;
        enemy.GetComponent<Animator>().enabled = false;
        DestroyAllBullets(currentBullet);
        ui.GameOver();
        //Restart();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateGamesPlayed()
    {
        gamesPlayed = PlayerPrefs.GetInt("games played", 1);
    }

    private IEnumerator Rotate(float angles, float duration)
    {
        Quaternion startRotation = target.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(new Vector3(0, angles, 0)) * startRotation;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            target.transform.rotation = Quaternion.Lerp(startRotation, endRotation, t / duration);
            yield return null;
        }
        target.transform.rotation = endRotation;
    }

    void InstantiateNewTargetForCurrentLevel()
    {
        try { target.GetComponent<TargetPartsController>().MoveTargetAway(); } catch (NullReferenceException) { print("Can't move away null ring"); }
        target = Instantiate(targets[targetInstantiated], new Vector3(0, 10, 7), Quaternion.Euler(0, 180, 0));
        target.GetComponent<TargetPartsController>().BringToCenter();
        ui.respawningNewTarget = true;
        InstantiateNewTarget();
    }

    private IEnumerator TargetPainted(float secondsToWait)
    {
        throwing = false;
        targetNoSpawned++;
        if (targetNoSpawned < targetNoForThisLevel)
        {
            bulletsShot = 0; partsPainted = partsCount;
            InstantiateNewTargetForCurrentLevel();
            StopAllCoroutines(); // return;
        }
        else
        {
            print("level complete\nreload scene"); // check other lines


            //PlayerPrefs.SetInt("partAnimationNo", partAnimationNo);
            //PlayerPrefs.Save();
            //print("last part anim no: "+PlayerPrefs.GetInt("partAnimationNo"));

            //int particles = PlayerPrefs.GetInt("backParticles", 0);
            //if (particles + 1 >= backgroundParticles.transform.childCount)
            //    PlayerPrefs.SetInt("backParticles", 0);
            //else PlayerPrefs.SetInt("backParticles", particles + 1);

            PlayerPrefs.SetInt("chooseNew", 0);
            PlayerPrefs.SetInt("target", targetInstantiated + 1);
            if(PlayerPrefs.GetInt("lastEnemy", 0) < enemies.Length - 1)
                PlayerPrefs.SetInt("lastEnemy", lastEnemy);
            PlayerPrefs.SetInt("tmpGamesPlayed", tmpGamesPlayed);
            ui.GoingToNextLevel();
            ui.ActivateLevelCompleteUi(true);
            //REPLACE WITH GameOverAnim, which at end calls Restart():
            PlayerPrefs.Save();
            yield return new WaitForSeconds(3);
            
            //Restart();

        }
    }
}
