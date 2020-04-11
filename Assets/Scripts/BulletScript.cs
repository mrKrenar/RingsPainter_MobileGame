using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {

    GameController gameController;
    
	void Awake () {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        GetComponent<Rigidbody>().maxAngularVelocity = 100;
        GetComponent<Rigidbody>().angularVelocity = new Vector3(0,0,-30);
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "HitPoint":
                collision.gameObject.GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color;
                collision.gameObject.GetComponent<BoxCollider>().isTrigger = true;
                gameController.DetectTargetFullyPainted();
                gameController.InstantiateParticlesOnBulletHit(transform.position, GetComponent<Renderer>().material.color);
                Destroy(gameObject);
                break;
            case "BulletDestroyer":
                gameController.BulletMissedTarget();
                Destroy(gameObject);
                break;
            case "Enemy":
                gameController.GameOver(gameObject);
                break;
                //play target part animation and color splash particles at this.position before break
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Enemy":
                gameController.InstantiateNearMissText(transform.position);
                break;
        }
    }
}
