using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Kees VandenBerg - 260725510 - COMP 521 A4

//Main Shopper Class 
public class Player : MonoBehaviour
{
    private Vector3 target;
    public Vector3 velocity;

    //total steering force
    private Vector3 steering;

    //max velocity
    private Vector3 maxVel;

    
    private Spawner spawner;

    private float arrival_radius;
    public List<CircleCollider2D> Collider_array;
    public GameObject ShopParent;
    public bool shopping;
    public int shop_index;
    public bool halt;
    public bool chair;
    public bool chair_wait;
    public bool flyered;


    void Start()
    {
        //find the positions of shops
        ShopParent = GameObject.Find("ShopParent");

        //find the spawner - this enforces separation
        spawner = GameObject.Find("Spawner").GetComponent<Spawner>();
        
        //list of circle colliders
        Collider_array = new List<CircleCollider2D>();

        //Get all other colliders in the scene 
        Collider_array.Add(GameObject.Find("t1").GetComponent<CircleCollider2D>());
        Collider_array.Add(GameObject.Find("t2").GetComponent<CircleCollider2D>());
        Collider_array.Add(GameObject.Find("t3").GetComponent<CircleCollider2D>());
        Collider_array.AddRange(GameObject.Find("PlanterMaster").GetComponentsInChildren<CircleCollider2D>());

        if (GameObject.Find("t4") != null) {
            Collider_array.Add(GameObject.Find("t4").GetComponent<CircleCollider2D>());
        } 

        //coin flip to find if they're going shopping!
        float is_shopping = Random.Range(0f, 1f);

        shopping = (is_shopping > 0.5f) ? true : false;
        
        //this controls if we are actually in the vicinity of the store 

        //if we are shopping, pick the center of a random store to be the target 
        if (shopping) {

            //pick one of the stores 
            shop_index = spawner.getShop();
            float rand_x = Random.Range(0f, 1.5f);
            float rand_y = Random.Range(0f, 0.75f);
            float pos_x = Random.Range(0, 1);
            float pos_y = Random.Range(0, 1);

            Vector3 shop_pos = ShopParent.transform.GetChild(shop_index).transform.position;

            //make this random position the new target
            target = shop_pos;

        }
        else {
            target = new Vector3(9f, this.transform.position.y, this.transform.position.z);
        }

        velocity = new Vector3(1f, 0f, 0f);
        maxVel = new Vector3(.05f, 0f, 0f);

        //controls the state of the Shopper
        arrival_radius = 1f;
        halt = false;
        chair_wait = false;
        chair = false;
        flyered = false;

    }

    void Update()
    {
        //see if we can pickup a flyer - if so, delete it from Game Boards and start the Flyered CoRoutine    
        GameObject fly = spawner.collect_flyer(this.gameObject);

        if (fly != null) {
            spawner.DropFlyer(fly);
            StartCoroutine("Flyered");
        }

        //if not stopped in store, waiting at chair or otherwise flyered, 
        if (!halt && !chair_wait && !flyered) {

            //if we're 'shopping' currently , see if we're within range of a shop
            if (distance(this.transform.position, target) < 0.5f && shopping) {
                //if we are within the range of our target, begin to wait
                
                //start the coroutine to pause at shop
                StartCoroutine("Shop_Action");
                shopping = false;

                //make it so another Shopper can get the shop
                spawner.returnShop(shop_index);
                chair = true;

                //pick a random chair to target
                target = spawner.chairlist[Random.Range(0, spawner.chairlist.Count)].transform.position;
                //target = new Vector3(tar.x +0.1f, tar.y + 0.3f, tar.z);
                
                //Debug.Log("chair :" + target);
            }

            //if we're looking for a chair and within 0.01f of it
            else if (distance(this.transform.position, target) < 0.01f && chair) {

                //start coroutine to wait at chair
                StartCoroutine("Chair_Action");
                chair = false;

                //set target of right side of the screen 
                target = new Vector3(9f, Random.Range(-2.5f, 2.5f), this.transform.position.z);
            }


            //otherwise, target is the right wall - check if we reached it. 
            else if (distance(this.transform.position, target) < 0.2f && !chair && !shopping) {
                spawner.remove(this.gameObject);
            }

            //if not at ANY goals, apply steering forces to get us to a goal
            else {
                steering = Vector3.zero;

                //arrival to guide us toward a target
                steering += arrival();
                
                //this is the criteria to GET steering forces
                if (chair && distance(this.transform.position, target) < 1f) {
                    //skip the steering forces if this close 
                }
                else{
                    //if not looking for chair and close to it, activate all collision forces
                    steering += collision() *10f;
                }
                //separate from all other actions
                steering += spawner.separation(this.gameObject);

                //clamp magnitude to avoid massive velocity spikes
                steering = Vector3.ClampMagnitude(steering , .04f);
                velocity = Vector3.ClampMagnitude(velocity + steering , .04f);
                this.transform.position += velocity;
            }
        }
    }

    //collision steering beahvior formula - send 2 vectors ahead and see if they intersect with collider
    private Vector3 collision() {

        //vectors ahead with slight offset 
        Vector3 ahead = (this.transform.position + Vector3.Normalize(velocity) * 0.5f);
        Vector3 ahead2 = (this.transform.position + Vector3.Normalize(velocity));

        //we actually want this to return the pt of intersection of ahead and the nearest
        CircleCollider2D threat = findMostThreateningObstacle(ahead, ahead2);
        Vector3 avoidance = Vector3.zero;

        if (threat != null) {
            
            //calculate the force needed to avoid the nearest obstacle
            avoidance.x = ahead.x - threat.transform.position.x;                       
            avoidance.y = ahead.y - threat.transform.position.y;

            //get the direction of this force and return it
            Vector3 res = Vector3.Normalize(avoidance) * 3f;

            return res;

        } 
        else {
            return Vector3.zero;
        }
        
    }

    //Vector distance formula
    private float distance(Vector3 a, Vector3 b) {
        return Mathf.Sqrt((a.x - b.x) * (a.x - b.x)  + (a.y - b.y) * (a.y - b.y));
    }

    //scan through the list of obstacles in the Colliderlist, and find the closest one
    private CircleCollider2D findMostThreateningObstacle(Vector3 ahead, Vector3 ahead2) {

        CircleCollider2D threat = null;
        float dist = 200f;

        //go through collider list 
        foreach (CircleCollider2D curr in Collider_array) {

            //if the pos vectors intersect the circle collider 
            if (intersects(ahead, ahead2, curr)) {

                float curr_dist = distance(this.transform.position, curr.transform.position);

                //if the threat is within the collision distance of 1.85, make it the nearest threat
                if (curr_dist < dist && curr_dist < 1.85f) {
                    threat = curr;
                    dist = curr_dist;
                }    
            }
        }
        return threat;
    }

    //check if 2 vectors intersect the radius of a circlecollider 
    private bool intersects(Vector3 ahead, Vector3 ahead2, CircleCollider2D circle) {

        return distance(circle.transform.position, ahead) <= circle.radius || distance(circle.transform.position, ahead2) <= circle.radius;
    
    }

    //ARRIVAL STEERING BEHAVIOR
    private Vector3 arrival() {
        
        Vector3 desired_velocity = (target - this.transform.position);

        //distance vector btwn shopper and target
        float distance = desired_velocity.magnitude;

        //if within the arrival rad, reduce the speed 
        if (distance < arrival_radius) {
            desired_velocity = Vector3.Normalize(desired_velocity) * (distance / arrival_radius);
        } 
        //otherwise, move towards the target normally
        else {
            desired_velocity = Vector3.Normalize(desired_velocity);
        }

        Vector3 arrival = desired_velocity - velocity;
        return new Vector3(arrival.x, arrival.y, 0);
 
    }

    //action to wait in shop for 1 sec 

    IEnumerator Shop_Action()
    {
        halt = true; 
        yield return new WaitForSecondsRealtime(1);
        halt = false;
    }

    //action to wait in chair for 2 sec
    IEnumerator Chair_Action()
    {
        chair_wait = true; 
        yield return new WaitForSecondsRealtime(3);
        chair_wait = false;
    }

    //play on Flyer pickup
    IEnumerator Flyered()
    {
        flyered = true; 

        //add Shopper to the flyered list - this allows advertisers to target it 
        spawner.AddFlyeredShopper(this.gameObject);
        this.gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
        yield return new WaitForSecondsRealtime(2);
        flyered = false;
        spawner.RemoveFlyeredShopper(this.gameObject);
    }

    //OnDestroy remove the Shopper from the collision list 
    void onDestroy() {
        spawner.RemoveFlyeredShopper(this.gameObject);
    }
}
