using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Kees VandenBerg - 260725510 - COMP 521 A4

public class Setup : MonoBehaviour
{
    
    public GameObject TablePlant;
    public GameObject Shopper;
    public GameObject Planter;
    public GameObject PlanterMaster;

    //Purpose of this is to spawn the chair and tables in random locations

    //x = [-9, 9] - only --1 to 5
    //y = [-7, 7] - only go -3 - 3
    void Start()
    {
        PlanterMaster = GameObject.Find("PlanterMaster");
        
        //get a (slightly) random number of tables 
        float tabNum = Random.Range(0f, 1f);
        float plantNum = Random.Range(0f, 1f);

        bool table_c = (tabNum > 0.5f) ? true : false;
        bool plant_c = (plantNum > 0.5f) ? true : false;

        //if table_c, spawn 4 tables 
        if (table_c) {

        //Spawning tables - pick random number btwn the 2 x values, pick random number between 2 y values, pick random location and spawn  

            //get random y values 
            float y1 = (float) Random.Range(-2.5f, 2.5f);
            float y2 = (float) Random.Range(-2.5f, 2.5f);
            float y3 = (float) Random.Range(-2.5f, 2.5f);
            float y4 = (float) Random.Range(-2.5f, 2.5f);

            //attach these to set x columns 
            Vector3 pos1 = new Vector3(-6.25f, y1,-1);
            Vector3 pos2 = new Vector3(-2.47f, y2,-1);
            Vector3 pos3 = new Vector3(1.5f, y3,-1);
            Vector3 pos4 = new Vector3(4.93f, y4,-1);

            Vector3 p1_spawn = Vector3.zero;
            Vector3 p2_spawn = Vector3.zero;
            Vector3 p3_spawn = Vector3.zero;


            //some minor modification to make surce tables/planters are spaced well
            if (y1 > 1&& y2 > 1)  {
                p1_spawn = new Vector3(-4.54f, Random.Range(-2.5f,0.5f),-1);
            }

            else if (y1 < -1 && y2 < -1) {
                p1_spawn = new Vector3(-4.54f, Random.Range(0.5f,2.5f),-1);
            }
            else if (y1 > 1) {
                p1_spawn = new Vector3(-4.54f, Random.Range(-1.5f,0.5f),-1);
            }
            else {
                p1_spawn = new Vector3(-4.54f, Random.Range(0.5f,1.5f),-1);
            }

            ///////

            if (y3 > 1&& y4 > 1)  {
                p2_spawn = new Vector3(-0.49f, Random.Range(-2.5f,0.5f),-1);
            }

            else if (y3 < -1 && y4 < -1) {
                p2_spawn = new Vector3(-0.49f, Random.Range(0.5f,2.5f),-1);
            }
            else if (y3 > 1) {
                p2_spawn = new Vector3(-0.49f, Random.Range(-1.5f,0.5f),-1);
            }
            else {
                p2_spawn = new Vector3(-0.49f, Random.Range(0.5f,1.5f),-1);
            }

            ///////

            if (y3 > 1&& y4 > 1)  {
                p3_spawn = new Vector3(7.07f, Random.Range(-2.5f,0.5f),-1);
            }

            else if (y3 < -1 && y4 < -1) {
                p3_spawn = new Vector3(7.07f, Random.Range(0.5f,2.5f),-1);
            }
            else if (y3 > 1) {
                p3_spawn = new Vector3(7.07f, Random.Range(-1.5f,0.5f),-1);
            }
            else {
                p3_spawn = new Vector3(7.07f, Random.Range(0.5f,1.5f),-1);
            }


            //spawn tables / chairs 
            GameObject t1 = Instantiate(TablePlant, new Vector3(-6.25f, y1,-1), Quaternion.identity);
            t1.name = "t1";
            GameObject t2 = Instantiate(TablePlant, new Vector3(-2.47f, y2,-1), Quaternion.identity);
            t2.name = "t2";

            GameObject t3 = Instantiate(TablePlant, new Vector3(1.5f, y3,-1), Quaternion.identity);
            t3.name = "t3";

            GameObject t4 = Instantiate(TablePlant, new Vector3(4.93f, y4,-1), Quaternion.identity);
            t4.name = "t4";

            float p_count = Random.Range(0f, 1f);

            //spawn tables        
            GameObject p1 = Instantiate(Planter, p1_spawn, Quaternion.Euler(0, 0, Random.Range(0, 360)));
            p1.transform.SetParent(PlanterMaster.transform);
            GameObject p2 = Instantiate(Planter,p2_spawn, Quaternion.Euler(0, 0, Random.Range(0, 360)));
            p2.transform.SetParent(PlanterMaster.transform);
            GameObject p3 = Instantiate(Planter, p3_spawn, Quaternion.Euler(0, 0, Random.Range(0, 360)));
            p3.transform.SetParent(PlanterMaster.transform);

        }
        else {
            
            //same as above, but spawning only 3 tables
            List<float> pos = new List<float> {-6.67f,-2.28f,2.05f , 6.57f };
            float y1 = (float) Random.Range(-2.5f, 2.5f);
            float y2 = (float) Random.Range(-2.5f, 2.5f);
            float y3 = (float) Random.Range(-2.5f, 2.5f);

            int p_count = (int) Random.Range(2f, 4f);
            
            for (int i=0; i<p_count; i++) {

                int index = (int) Random.Range(0, pos.Count);
                float toS = pos[index];
                pos.RemoveAt(index);

                float y = (float) Random.Range(-4f,4f);

                GameObject p = Instantiate(Planter, new Vector3(toS, y,-1), Quaternion.Euler(0, 0, Random.Range(0, 360)));
                p.transform.SetParent(PlanterMaster.transform);
            }

            GameObject t1 = Instantiate(TablePlant, new Vector3(-4.5f, y1,-1), Quaternion.identity);
            t1.name = "t1";

            GameObject t2 = Instantiate(TablePlant, new Vector3(0f, y2,-1), Quaternion.identity);            
            t2.name = "t2";            

            GameObject t3 = Instantiate(TablePlant, new Vector3(4.46f, y3,-1), Quaternion.identity);
            t3.name = "t3";

        }

    }
}
