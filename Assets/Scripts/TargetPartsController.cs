using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPartsController : MonoBehaviour {

    public GameObject[] triggerParts;
    bool moveTargetAway, bringToCenter;

    public void ChangeTriggersToColliders()
    {
        foreach(var part in triggerParts)
        {
            if(part != null)
                part.GetComponent<BoxCollider>().isTrigger = false;
        }
    }

    public void MoveTargetAway()
    {
        moveTargetAway = true;
    }
    public void BringToCenter()
    {
        bringToCenter = true;
    }

    private void FixedUpdate()
    {
        if(moveTargetAway)
        {
            transform.position = new Vector3(0, Mathf.Lerp(transform.position.y, -50f, Time.fixedDeltaTime), 7);
            if(transform.position.y <= -49.5f)
            {
                moveTargetAway = false;
                Destroy(gameObject);
            }
        }
        else if (bringToCenter)
        {
            transform.position = new Vector3(0, Mathf.Lerp(transform.position.y, 0, Time.fixedDeltaTime * 3), 7);
            if(transform.position.y <= 0.01f)
            {
                transform.position = new Vector3(0, 0, 7);
                bringToCenter = false;
                GameObject.FindGameObjectWithTag("GameController").GetComponent<UiController>().respawningNewTarget = false;
                GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().throwing = true;
                GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().EnemiesState(true);
                GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().MoveAngleFinder();
            }
        }
    }
}
