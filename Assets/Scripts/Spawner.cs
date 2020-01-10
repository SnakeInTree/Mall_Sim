using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Kees VandenBerg - 260725510 - COMP 521 A4

//this is the class responsible for spawning all players 
//when a player is spawned, we keep a record of their circle collider and make it a member of a publci list
//we then use this list to test against! We also add table circel collider here, and change the Player script to work off this alone 


public class Spawner : MonoBehaviour
{

    public GameObject Player;
    public GameObject Advertiser;
    public GameObject ShopParent;

    //dist from which the advertiser can see the flyered shopper
    public float observation_dist;

    //dist from which the advertiser can deliever a successful pitch 
    public float pitch_dist;

    //rate at which we test for ad drops
    public float ad_rate;

    //probability we drop an ad
    public float ad_probability;

    public int Shopper_Count = 3;
    public int Advertiser_Count = 2;

    private int spawned_shoppers = 0;
    private int spawned_ads = 0;

    //list of active actors
    public List<GameObject> PlayerList;

    private int numGen;

    //list of shops
    public List<int> shopList;

    //list of chairs
    public List<GameObject> chairlist;

    //list of flyers
    public List<GameObject> flyerList;
    //list of flyered shoppers 
    public List<GameObject> flyeredShoppers;

    void Start()
    {   

        //set defualts for ad process
        observation_dist = 3f;
        pitch_dist = 3f;
        ad_rate = 4f;
        ad_probability = 0.5f;

        //init gamelists 
        chairlist = new List <GameObject>();
        flyerList = new List <GameObject>();
        flyeredShoppers = new List <GameObject>();
        ShopParent = GameObject.Find("ShopParent");

        //list of shop indexes
        shopList = new List<int>() {0,1 ,2 , 3, 4, 5, 6, 7, 8, 9, 10};
        numGen = 0;
        PlayerList = new List<GameObject>();
        //create each Shopper object
        for (int i=0; i<Shopper_Count; i++) {
            //random y start pos 
            float pos = Random.Range(-2.5f, 2.5f);
            GameObject shopper = Instantiate(Player, new Vector3(-8.5f, pos, -2), Quaternion.identity);
            PlayerList.Add(shopper);
            shopper.transform.SetParent(this.transform);
            shopper.name = "shopper_" + numGen;
            numGen++;
            spawned_shoppers++;
        }
        for (int j=0; j<Advertiser_Count; j++) {
            SpawnAdvertiser();
        }
    }

    void Update() {
       
        //make sure the number of active advertisers and shoppers matches that set by the unity editor!
        if (spawned_ads < Advertiser_Count) {
            SpawnAdvertiser();
        }
        if (spawned_shoppers < Shopper_Count) {
            SpawnShopper();
        }
    }

    //vector dist method
    private float distance(Vector3 a, Vector3 b) {
        return Mathf.Sqrt((a.x - b.x) * (a.x - b.x)  + (a.y - b.y) * (a.y - b.y));
    }

    public int getShop() {

        int rand = Random.Range(0, 10);
        shopList.Remove(rand);
        return rand;

    }

    //when a shopper successfully goes to a shop, return it to the list of selectable shops. 
    public void returnShop(int toRet) {
        shopList.Add(toRet);

    } 
    
    //called by the Shopper 
    public GameObject collect_flyer(GameObject shopper) {
        
        //see if the Player is within range of a Flyer! If so, return the Gameobject so that the shopper can pickit up
        for (int i=0; i< flyerList.Count; i++) {
            if (flyerList[i] != null && distance(shopper.transform.position, flyerList[i].transform.position) < 0.4f) {
                return flyerList[i];
            }
        }
        return null;
    }

    public Vector3 separation(GameObject curr) {

        int count = 0;
        Vector3 force = Vector3.zero;
        float SEPARATION_RADIUS = 1f;

        //go through the list of active actors (shoppers, advertisers) and see if they are within the current actor's separation radius 
        for (int k=0; k< PlayerList.Count; k++) {
            if (PlayerList[k] != curr && distance(PlayerList[k].transform.position, curr.transform.position) <= SEPARATION_RADIUS) {

                //find a general force vector for all actors who are within the sep. rad. 
                force.x += PlayerList[k].transform.position.x - curr.transform.position.x;
                force.y += PlayerList[k].transform.position.y - curr.transform.position.y;
                count++;
            }
        }
        
        //make this into a generalized, aveage force vector
        if (count != 0) {
            force.x /= count;
            force.y /= count;
            force *= -1;
        }

        Vector3 ret = Vector3.Normalize(force);
        return ret;

    }

    public void remove (GameObject shopper_done) {

        //remove a shopper when they reach the right side of the screen after accomplishing their goals. 
        PlayerList.Remove(shopper_done);
        GameObject.Destroy(shopper_done);        
        spawned_shoppers--;
    }

    public void removeAd (GameObject ad_done) {
        //remove advertiser when they pitch 3 times successfully
        PlayerList.Remove(ad_done);
        GameObject.Destroy(ad_done);      
        spawned_ads--;  
    }

    public void SpawnAdvertiser() {

        //pick one of the stores, and spawn with an offset?
        int pos = Random.Range(0, 10);
        
        Vector3 placement = ShopParent.transform.GetChild(pos).transform.position;

        if (pos < 5) {
            placement.y -= 1.5f;
        }
        else { 
            placement.y += 1.5f;
        }

        placement.z = -2;

        GameObject Ad_man = Instantiate(Advertiser, placement, Quaternion.identity);
        //add to list of separation chars 
        PlayerList.Add(Ad_man);
        Ad_man.transform.SetParent(this.transform);

        spawned_ads++;

    }


    public void AddFlyer(GameObject curr) {
        //add flyer on drop from advertiser 
        flyerList.Add(curr);
    }

    public void DropFlyer(GameObject curr) {
        //destroy flyer on drop
        flyerList.Remove(curr);
        GameObject.Destroy(curr);
    }

    //add a flyered shopper to the list 
    public void AddFlyeredShopper(GameObject curr) {
        flyeredShoppers.Add(curr);
    }

    public void RemoveFlyeredShopper(GameObject curr) {
        //remove a flyered shopper from the list 
        flyeredShoppers.Remove(curr);
    }

    //spawn shopper method    
    public void SpawnShopper() {

        //pick random x coord 

        float pos = Random.Range(-2.5f, 2.5f);
        GameObject shopper = Instantiate(Player, new Vector3(-9, pos, -2), Quaternion.identity);
        //add the shopper to list of separation objects
        PlayerList.Add(shopper);
        shopper.transform.SetParent(this.transform);
        shopper.name = "shopper_" + numGen;
        numGen++;
        spawned_shoppers++;

    }



}
