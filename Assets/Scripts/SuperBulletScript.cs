using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperBulletScript : MonoBehaviour
{
    GameController gameController;
    Rigidbody rb;

    void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 100;
        rb.angularVelocity = new Vector3(0, 0, -30);
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "HitPoint":
                gameController.SuperBulletHit(transform.position);
                gameController.DetectTargetFullyPainted();
                Destroy(gameObject);
                break;
            case "Enemy":
                Destroy(collision.gameObject);
                rb.velocity = Vector3.zero;
                rb.AddForce(0, 0, 1000, ForceMode.Acceleration);
                break;
        }
    }
}
