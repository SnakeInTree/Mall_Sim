using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Kees VandenBerg - 260725510 - COMP 521 A4

public class Advertiser : MonoBehaviour
{
    public GameObject curr_Shopper;
    public GameObject ShopParent;
    public GameObject Walls;
    public GameObject Flyer;
    public Sprite one;
    public Sprite two;

    public Spawner spawner;
    public Vector3 follow_velocity;
    public Vector3 behind;
    public float arrival_radius;
    public Vector3 velocity;
    public Vector3 maxVel;

    public Vector3 steering;
    private float wanderAngle;
    public List<CircleCollider2D> Collider_array;
    public float observation_dist;
    public float pitch_radius;
    public float ad_rate;
    public float probability;
    public bool flyer_wait;

    public bool wandering;
    private bool timingInRange;
    private bool timingOutOfRange;
    private int successful_pitches;
    private bool spawing_flyers;


    void Start()
    {
        //set booleans for pursuit mechanics 
        spawing_flyers = false;
        timingInRange = false;
        timingOutOfRange = false;
        
        successful_pitches =  0;
        
        wandering = true;

        wanderAngle = 0;
        velocity = new Vector3(1f, 0f, 0f);
        maxVel = new Vector3(.05f, 0f, 0f);
        behind = Vector3.zero;
        follow_velocity = Vector3.zero;
        arrival_radius = 1f;
    
        ShopParent = GameObject.Find("ShopParent");
        Walls = GameObject.Find("Walls");

        //find the spawner - this enforces separation
        spawner = GameObject.Find("Spawner").GetComponent<Spawner>();
        
        //list of circle colliders
        Collider_array = new List<CircleCollider2D>();

        //Get all other colliders in the scene 
        Collider_array.Add(GameObject.Find("t1").GetComponent<CircleCollider2D>());
        Collider_array.Add(GameObject.Find("t2").GetComponent<CircleCollider2D>());
        Collider_array.Add(GameObject.Find("t3").GetComponent<CircleCollider2D>());
        Collider_array.AddRange(GameObject.Find("PlanterMaster").GetComponentsInChildren<CircleCollider2D>());
        Collider_array.AddRange(ShopParent.GetComponentsInChildren<CircleCollider2D>());
        Collider_array.AddRange(Walls.GetComponentsInChildren<CircleCollider2D>());

        if (GameObject.Find("t4") != null) {
            Collider_array.Add(GameObject.Find("t4").GetComponent<CircleCollider2D>());
        }
        flyer_wait = false;

    }

    // Update is called once per frame
    void Update()
    {
        //check for changes in the advertiser attributes
        probability = spawner.ad_probability;
        ad_rate = spawner.ad_rate;
        observation_dist = spawner.observation_dist;
        pitch_radius = spawner.pitch_dist;

        //check to see if the flyered shoppers has been despawned - if so, return to wandering 
        if (curr_Shopper == null) {
            wandering = true;
            StopCoroutine("Out_of_Range");
            StopCoroutine("In_Range");
            timingOutOfRange = false;
            timingInRange = false;
            this.gameObject.GetComponent<SpriteRenderer>().color = new Color32 (0, 164, 41, 255);

        }

        //if we're wandering, check the distance to the nearest flyered shopper 
        if (wandering) {
            checkFlyeredDist();
        }

        //if we're pursuing 
        else { 
            //check if we're within the pitch radius
            if (distance(this.transform.position, curr_Shopper.transform.position) < pitch_radius) {
                
                //if switching from being OUT of pitch_radius to being IN pitch_radius, switch the timers
                if (timingOutOfRange) {
                    StopCoroutine("Out_of_Range");
                    timingOutOfRange = false;
                    StartCoroutine("In_Range");
                    timingInRange = true;
                }
            }
            //if we're OUT of range, switching from being IN pitch_radius to OUT of pitch_radius, switch the timers

            else {
                if (timingInRange) {
                    StopCoroutine("In_Range");
                    timingInRange = false;
                    StartCoroutine("Out_of_Range");
                    timingOutOfRange = true;
                }
            }
        }
        
        //if we are not in the flyer-cooldown period, check if we're within range of a store. If so, check if we can spawn a flyer.
        if (!flyer_wait) {
            if (checkStoreDist()) {
                StartCoroutine("SpawnFlyer");
            }
        }
        
        steering = Vector3.zero; // the null vector, meaning "zero force magnitude"
        
        //if wandering, apply that force
        if (wandering) {
            steering = steering + wander();
        }
        //if we're not wandering, we're pursuing! 
        else {
            steering = steering + follow();
        } 
        
        //collision and separation need to always be present
        steering += collision() *10f;
        steering += spawner.separation(this.gameObject) * 10f;
        
        steering = Vector3.ClampMagnitude(steering , .05f);
        velocity = Vector3.ClampMagnitude(velocity + steering , .05f);
        this.transform.position += velocity;
    }

    //collision - IDENTICAL TO THE METHOD IN PLAYER.CS
    private Vector3 collision() {

        Vector3 ahead = (this.transform.position + Vector3.Normalize(velocity) * 0.5f);
        Vector3 ahead2 = (this.transform.position + Vector3.Normalize(velocity));

        //we actually want this to return the pt of intersection of ahead and the nearest
        CircleCollider2D threat = findMostThreateningObstacle(ahead, ahead2);
        Vector3 avoidance = Vector3.zero;

        if (threat != null) {

            avoidance.x = ahead.x - threat.transform.position.x;                       
            avoidance.y = ahead.y - threat.transform.position.y;

            Vector3 res = Vector3.Normalize(avoidance) * 3f;

            return res;

        } 
        else {
            return Vector3.zero;
        }
        
    }

    //distance - IDENTICAL TO THE METHOD IN PLAYER.CS
    private float distance(Vector3 a, Vector3 b) {
        return Mathf.Sqrt((a.x - b.x) * (a.x - b.x)  + (a.y - b.y) * (a.y - b.y));
    }
   
    //findMostThreateningObstacle - IDENTICAL TO THE METHOD IN PLAYER.CS
    private CircleCollider2D findMostThreateningObstacle(Vector3 ahead, Vector3 ahead2) {

        CircleCollider2D threat = null;
        float dist = 200f;

        foreach (CircleCollider2D curr in Collider_array) {

            if (intersects(ahead, ahead2, curr)) {

                float curr_dist = distance(this.transform.position, curr.transform.position);
                if (curr_dist < dist && curr_dist < 1.85f) {
                    threat = curr;
                    dist = curr_dist;
                }    
            }
        }
        return threat;
    }

    //intersects - IDENTICAL TO THE METHOD IN PLAYER.CS
    private bool intersects(Vector3 ahead, Vector3 ahead2, CircleCollider2D circle) {

        return distance(circle.transform.position, ahead) <= circle.radius || distance(circle.transform.position, ahead2) <= circle.radius;
    
    }

    private Vector3 follow() {

        //get the negative velocity of the Flyered Shopper 
        follow_velocity = curr_Shopper.GetComponent<Player>().velocity * -1;
        follow_velocity = Vector3.Normalize(follow_velocity);
        
        //Calculate the behind vector to be just a step behind the target shopper 
        behind = curr_Shopper.transform.position + follow_velocity;
        
        //set this point as the target for the arrival method
        return arrival(behind);

    }

    //IDENTICAL TO THE METHOD IN PLAYER.CS
    private Vector3 arrival(Vector3 target) {
        Vector3 desired_velocity = (target - this.transform.position);
        float distance = desired_velocity.magnitude;
        if (distance < arrival_radius) {
            desired_velocity = Vector3.Normalize(desired_velocity) * (distance / arrival_radius);
        } 
        else {
            desired_velocity = Vector3.Normalize(desired_velocity);
        }
        Vector3 arrival = desired_velocity - velocity;
        return arrival;
 
    }

    private Vector3 wander() {
        
        //this is the direction we're heading in, times some constant to place it infront of the circle's velocity
        Vector3 CircleCenter = Vector3.Normalize(velocity) * 1f;

        //amt of displacement this causes 
        Vector3 Displacement = (new Vector3(1f, 0f, 0f)) * 0.5f;

        //get a new vector representing the angle placed on the displcement vector
        Vector3 alt_dis = setAngle(Displacement, wanderAngle);

        //change the wandernangle slightly to ensure diffrerent behavior on next call
        wanderAngle += Random.Range(0, 5);

        return CircleCenter + alt_dis;

    }

    //given a direction vector and a random angle, comute the angled dir vector
    public Vector3 setAngle(Vector3 dir, float angle) {

        float len = dir.magnitude;
        float x = Mathf.Cos(angle) * len;
        float y = Mathf.Sin(angle) * len;

        return new Vector3(x, y, dir.z);

    }

    //flyer timer
    IEnumerator SpawnFlyer()
    {
        
        //random float
        float res = Random.Range(0f, 1f);
        flyer_wait = true;

        //if the random is less than the flyer probability, spawn a flyer 
        if (res < probability && spawing_flyers) {
            GameObject flyer = Instantiate(Flyer, this.transform.position, Quaternion.identity);
            spawner.AddFlyer(flyer);
        }

        //wait the 'ad_rate' number of seconds before attempting to drop another flyer 
        yield return new WaitForSecondsRealtime(ad_rate);
        flyer_wait = false;
        spawing_flyers = true;
    }

    //this governs if we can drop a flyer, since we must be within 3f of a store to drop a flyer
    public bool checkStoreDist() {

        for (int i=0; i<10; i++) {
            if (distance(this.transform.position, ShopParent.transform.GetChild(i).transform.position) < 3f) {
                
                return true;
            }
        }
        return false;
    }

    //check the distance to all flyered shoppers - if we are within the observation radius, we switch to 'pursuit' mode, and set the 
    //position of the shopper to be our current target
    public bool checkFlyeredDist() {

        //for all flyered shoppers
        for (int i=0; i<spawner.flyeredShoppers.Count; i++) {
            //for all shoppers within the observation dist 
            if (distance(this.transform.position, spawner.flyeredShoppers[i].transform.position) < observation_dist ) {
                
                //if we're within the radius of this shopper, we want to follow them!
                //stop wandering
                curr_Shopper = spawner.flyeredShoppers[i];
                wandering = false;
                timingInRange = true;
                //start the in range timer
                StartCoroutine("In_Range");
                this.gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;

                return true;

            }
        }
        return false;
    }

    //timer for successful sales pitch - if the coroutine is not stopped, change the sprite (or despawn) to mark the successful pitch, and retunr to wandering. 
    IEnumerator In_Range()
    {
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSecondsRealtime(4);

        //After we have waited 5 seconds print the time again.
        //Debug.Log("Pitch Successful!");

        if (successful_pitches == 0) {
            this.gameObject.GetComponent<SpriteRenderer>().sprite = one;
            successful_pitches++;
        }

        else if (successful_pitches == 1) {
            this.gameObject.GetComponent<SpriteRenderer>().sprite = two;
            successful_pitches++;
        }

        else {
            spawner.removeAd(this.gameObject);
        }

        wandering = true;
        curr_Shopper = null;
        timingOutOfRange = false;
        timingInRange = false;
        this.gameObject.GetComponent<SpriteRenderer>().color = new Color (0, 164, 41, 255);


    }

    //Timer for the amt of time a advertiser can spend outside of the pitch range of a flyered shopper. If the coroutine is not stopped in 5 sec, 
    //return to wandering 
    IEnumerator Out_of_Range()
    {
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSecondsRealtime(5);

        //After we have waited 5 seconds print the time again.
        //Debug.Log("Pitch Failed");
        wandering = true;
        this.gameObject.GetComponent<SpriteRenderer>().color = new Color (0, 164, 41, 255);
        curr_Shopper = null;
        timingOutOfRange = false;
        timingInRange = false;
    }
}
