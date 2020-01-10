using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Kees VandenBerg - 260725510 - COMP 521 A4
//Class to randomly place chairs around a table 
public class ArrangeChairs : MonoBehaviour
{

    public GameObject Chair;
    public GameObject Table;

    public Spawner SpawnerObj;

    
    void Start() {

        GameObject top_Spawner = GameObject.Find("Spawner");
        SpawnerObj = top_Spawner.GetComponent<Spawner>();

        //make the table 
        GameObject TableObj = Instantiate(Table, this.transform.position, Quaternion.identity);
        TableObj.transform.SetParent(this.transform);

        Vector3 center = this.transform.position;

        //make a List of positions from the center of this circle
        List<Vector3> posList = RandomCircle(center, 0.85f);

        //instantiate each chair at these pos
        for (int i = 0; i < 4; i++){
            GameObject ChairObj = Instantiate(Chair, posList[i], Quaternion.identity);
            ChairObj.transform.SetParent(this.transform);
            SpawnerObj.chairlist.Add(ChairObj);
         }
    }
 
    public List<Vector3> RandomCircle ( Vector3 center ,   float radius  ){
        
        //get a random range between each sector of the circle
        List <Vector3> posList = new List<Vector3>();
        float ang1 = Random.Range(20, 70);
        float ang2 = Random.Range(110, 160);
        float ang3 = Random.Range(200, 250);
        float ang4 = Random.Range(290, 340);

        Vector3 pos1;
        Vector3 pos2;
        Vector3 pos3;
        Vector3 pos4;

        //find the position of a vector which results from applying the slight random position offset

        pos1.x = center.x + radius * Mathf.Sin(ang1 * Mathf.Deg2Rad);
        pos1.y = center.y + radius * Mathf.Cos(ang1 * Mathf.Deg2Rad);
        pos1.z = center.z;

        pos2.x = center.x + radius * Mathf.Sin(ang2 * Mathf.Deg2Rad);
        pos2.y = center.y + radius * Mathf.Cos(ang2 * Mathf.Deg2Rad);
        pos2.z = center.z;

        pos3.x = center.x + radius * Mathf.Sin(ang3 * Mathf.Deg2Rad);
        pos3.y = center.y + radius * Mathf.Cos(ang3 * Mathf.Deg2Rad);
        pos3.z = center.z;

        pos4.x = center.x + radius * Mathf.Sin(ang4 * Mathf.Deg2Rad);
        pos4.y = center.y + radius * Mathf.Cos(ang4 * Mathf.Deg2Rad);
        pos4.z = center.z;

        posList.Add(pos1);
        posList.Add(pos2);
        posList.Add(pos3);
        posList.Add(pos4);

        return posList;



     }

}
