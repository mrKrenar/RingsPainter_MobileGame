using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetCameraPositionAndRotation : MonoBehaviour
{
    private bool reset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void RessetPosition()
    {
        reset = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (reset)
        {
            transform.position = new Vector3(Mathf.Lerp(transform.position.y, 0f, Time.fixedDeltaTime), Mathf.Lerp(transform.position.y, 8.53f, Time.fixedDeltaTime), Mathf.Lerp(transform.position.y, -10f, Time.fixedDeltaTime));
            //transform.rotation = new Quaternion.euler(Mathf.Lerp(transform.position.y, 0f, Time.fixedDeltaTime), Mathf.Lerp(transform.position.y, 8.53f, Time.fixedDeltaTime), Mathf.Lerp(transform.position.y, -10f, Time.fixedDeltaTime));
            if(transform.position.y >= 8.4f)
            {
                transform.position = new Vector3(0f,8.53f, -10f);
                reset = false;
            }
        }
    }
}
