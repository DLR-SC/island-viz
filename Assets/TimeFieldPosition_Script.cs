using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeFieldPosition_Script : MonoBehaviour
{
    public float thresholdAngleDegree = 20;
    public float angularRateDegSec = 20f;

    private float lowerThresholdAngleDegree = 5f;


    private Transform thisFieldPos;
    private Transform playerPos;

    private bool moving;

    // Start is called before the first frame update
    void Start()
    {

        thisFieldPos = gameObject.transform;
       GameObject player = GameObject.Find("VRCamera (eye)");

        if(player != null)
        {
            playerPos = player.transform;
        }
         moving = false;

        Vector2 thisFieldPos2D = new Vector2(playerPos.position.x, playerPos.position.z);
        float initialAngle = Vector2.SignedAngle(thisFieldPos2D, Vector2.right);
        
         thisFieldPos.localEulerAngles = new Vector3(0f, initialAngle, 0f);

        StartCoroutine(PositionController());
    }

    private IEnumerator PositionController()
    {
        while (true)
        {
            float deltaAngle = GetDeltaAngle();

            if(Mathf.Abs(deltaAngle) > thresholdAngleDegree && !moving)
            {
                moving = true;
            }
            if(Mathf.Abs(deltaAngle) < lowerThresholdAngleDegree && moving)
            {
                moving = false;
            }
            if (moving)
            {
                float oldAngle = thisFieldPos.localEulerAngles.y;
                float newAngle = oldAngle + Mathf.Sign(deltaAngle)*angularRateDegSec*0.1f;
                thisFieldPos.localEulerAngles = new Vector3(0, newAngle, 0);
            }
            yield return new WaitForSeconds(0.1f);
        }
       
        
    }

    private float GetDeltaAngle()
    {
        float thisAngle = thisFieldPos.localEulerAngles.y;

        Vector2 thisVector = new Vector2(Mathf.Cos(DegreeToRad(thisAngle)), -1* Mathf.Sin(DegreeToRad(thisAngle)));

        Vector2 playerVector = new Vector2(playerPos.position.x, playerPos.position.z);

        float res = Vector2.SignedAngle(playerVector, thisVector);
        return res;

    }


    private float DegreeToRad(float value)
    {
        return value / 180f * Mathf.PI;
    }

}
