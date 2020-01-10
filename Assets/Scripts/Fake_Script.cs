using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fake_Script : MonoBehaviour
{
        // desired_velocity = Vector3.Normalize(target - this.transform.position);

        //  steering = desired_velocity - velocity;
 
        //  velocity = Vector3.ClampMagnitude(velocity + steering , .05f);
        //  this.transform.position += velocity;
    public Vector3 target;
    public Vector3 prev;
    public Vector3 curr;
    public Vector3 velocity;
    public Vector3 desired_velocity;
    public Vector3 steering;
    public float arrival_radius;

    

    void Awake()
    {
        Physics2D.queriesStartInColliders = false;
        target = new Vector3(6.5f, this.transform.position.y, this.transform.position.z);
        velocity = new Vector3(1f, 0f, 0f);

        arrival_radius = 1f;
    }

    void Update()
    {

        //this is the basis of STEERING 
        desired_velocity = (target - this.transform.position);

        float distance = desired_velocity.magnitude;
            //Debug.Log(distance);

        if (distance < arrival_radius) {
            desired_velocity = Vector3.Normalize(desired_velocity) * (distance / arrival_radius);
        } 
        else {
            desired_velocity = Vector3.Normalize(desired_velocity);
        }

        steering = desired_velocity - velocity;
 
        velocity = Vector3.ClampMagnitude(velocity + steering , .05f);
        this.transform.position += velocity;

    }


}
