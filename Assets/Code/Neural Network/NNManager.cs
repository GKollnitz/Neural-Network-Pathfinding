using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

public class NNManager : MonoBehaviour
{
    private int hiddenLayerSize = 4;
    private int[] layers;  //Inputs: Relative Position of Target on X-axis, Relative Position of Target on Y-axis. "Cell Above, Cell Below, Cell Left, Cell Right".
                            //Outputs: Up, Down, Left, Right
    private List<NeuralNetwork> networks;
    public int nrColliders = 50;
	private int nrUnits = 20;
    private float speed = 1;
	private List<Unit> unitList = null;
    private List<GameObject> colliders = null;
	public GameObject unitPrefab;
    public GameObject collisionPrefab;
    private int generationNumber;
	private float timer;
    public float totalTimer;
    private Interface NNInterface;
    public GameObject[] goal;
    private float xSpawn = 93f;
    private float ySpawn = 93f;
    private float xSpawnMin = 13f;
    private float ySpawnMin = -2.5f;
    //public GameObject goal2;
    public GameObject start;
    public bool reachedEnd = false;
    public int reachedEndCount = 0;
    public bool mutateUnits = true;
    public bool pathfindingToggle = true;
    public bool collidersToggle = true;
    private bool allColliding = true;
    private bool pathfindingToggled = true;


    private bool atGoal = false;

	// Use this for initialization
	void Start () 
	{
        Application.runInBackground = true;
        Physics2D.gravity = Vector2.zero;
        layers = new int[] { 6, hiddenLayerSize, hiddenLayerSize, 4 };
        NNInterface = new Interface();
        NNInterface.Initialize();
        totalTimer = 250;

		networks = new List<NeuralNetwork>();
		unitList = new List<Unit>();
        colliders = new List<GameObject>();
		generationNumber = 1;
		timer = totalTimer;
		for(int i = 0; i<nrUnits; i++)
		{
			NeuralNetwork net = new NeuralNetwork(layers);
            net.Mutate();
            networks.Add(net);
            Vector3 startP = start.transform.position;
            Unit unitP = ((GameObject)Instantiate(unitPrefab, startP, unitPrefab.transform.rotation)).GetComponent<Unit>();
            unitP.Init(networks[i]);
            unitP.goal = goal[0];
            unitList.Add(unitP);
		}
        for (int i = 0; i < nrColliders; i++)
        {
            Vector3 pos = new Vector3(UnityEngine.Random.Range(0, xSpawn) * 0.26f + xSpawnMin, (UnityEngine.Random.Range(0, ySpawn) * -0.26f + ySpawnMin), 0);
            GameObject unitP = ((GameObject)Instantiate(collisionPrefab, pos, collisionPrefab.transform.rotation));
            colliders.Add(unitP);
        }
        CreateNewGeneration();
	}
	
	// Update is called once per frame
	void Update () 
	{
        mutateUnits = NNInterface.mutate;
        collidersToggle = NNInterface.colliders;
        pathfindingToggle = NNInterface.pathfinding;

        
        timer -= (Time.deltaTime*speed);
        float newSpeed = NNInterface.getSpeed();
        allColliding = true;

        if (speed != newSpeed)
        {
            speed = newSpeed;
        }

        if (Input.GetButtonDown("Fire2"))
        {
            /*
            for (int i = 0; i < nrColliders; i++)
            {
                Destroy(colliders[0]);
                colliders.RemoveAt(0);
            }

            for (int i = 0; i < nrColliders; i++)
            {
                Vector3 pos = new Vector3(UnityEngine.Random.Range(0,xSpawn)*0.26f+xSpawnMin, -(UnityEngine.Random.Range(0,ySpawn)*0.26f+ySpawnMin), 0);
                GameObject unitP = ((GameObject)Instantiate(collisionPrefab, pos, collisionPrefab.transform.rotation));
                colliders.Add(unitP);*/
            
        }

        for(int i = 0; i<unitList.Count(); i++)
        {
            if(unitList[i].colliding)
            {
                unitList[i].net.AddKnowledge(-Time.deltaTime*speed);
            }
            
            if(unitList[i].transform.position.x<-0.5)
            {
                unitList[i].colliding = true;
                unitList[i].lastWord = "out";
            }
            else if (unitList[i].transform.position.x > 50.5)
            {
                unitList[i].colliding = true;
                unitList[i].lastWord = "out";
            }
            else if (unitList[i].transform.position.y > 0.5)
            {
                unitList[i].colliding = true;
                unitList[i].lastWord = "out";
            }
            else if (unitList[i].transform.position.y < -27.5)
            {
                unitList[i].colliding = true;
                unitList[i].lastWord = "out";
            }
            if (!unitList[i].colliding)
            {
                allColliding = false;
            }
            if (unitList[i].atGoal && pathfindingToggled)
            {
                unitList[i].timeTaken = 0;
                if (unitList[i].goal == goal[0])
                {
                    unitList[i].goal = goal[1];
                    Color newColor = Color.yellow;
                    newColor.a = 0.3f;
                    unitList[i].GetComponent<SpriteRenderer>().color = newColor;
                    unitList[i].net.AddKnowledge(100-unitList[i].timeTaken);
                }
                else if (unitList[i].goal == goal[1])
                {
                    unitList[i].goal = goal[2];
                    Color newColor = Color.green;
                    newColor.a = 0.3f;
                    unitList[i].GetComponent<SpriteRenderer>().color = newColor;
                    unitList[i].net.AddKnowledge(300 - unitList[i].timeTaken);
                }
                else if (unitList[i].goal == goal[2])
                {
                    unitList[i].goal = goal[3];
                    Color newColor = Color.cyan;
                    newColor.a = 0.3f;
                    unitList[i].GetComponent<SpriteRenderer>().color = newColor;
                    unitList[i].net.AddKnowledge(100 - unitList[i].timeTaken);
                }
                else if (unitList[i].goal == goal[3])
                {
                    Color newColor = Color.magenta;
                    newColor.a = 0.3f;
                    unitList[i].GetComponent<SpriteRenderer>().color = newColor;
                    unitList[i].net.AddKnowledge(600 - unitList[i].timeTaken);
                    unitList[i].goal = start;
                }
                else if (unitList[i].goal == start)
                {
                    reachedEnd = true;
                    unitList[i].net.AddKnowledge(1000 - unitList[i].timeTaken);
                    unitList[i].goal = goal[0];
                }
                unitList[i].atGoal = false;
            }
            else if (unitList[i].doneMoving)
            {
                if (!unitList[i].colliding)
                {
                    CalcAndMove(unitList[i]);
                }
                unitList[i].doneMoving = false;
            }
        }

        

        //Every time the timer expires or the "Jump" Key is pressed, a new generation is born and replaces their senior.

        if (Input.GetButtonDown("Jump") || timer < 0 || atGoal || allColliding || NNInterface.newGen)
		{
            atGoal = false;
            NNInterface.newGen = false;
            CreateNewGeneration();
            for (int i = 0; i < unitList.Count(); i++)
            {
                CalcAndMove(unitList[i]);
                unitList[i].doneMoving = false;
            }
            generationNumber++;
			GameObject canvasObject = GameObject.FindGameObjectWithTag("canvas");
			Transform child = canvasObject.transform.Find("Text");
	 		Text generationText = child.GetComponent<Text>();
	 		generationText.text = "Generation: " + generationNumber.ToString();
	 		
	 	}

        if (NNInterface.save)
        {
            Save(NNInterface.fileName);
            NNInterface.save = false;
        }
        else if (NNInterface.load)
        {
            Load(NNInterface.fileName);
            NNInterface.load = false;
            timer = totalTimer;
            Color te = Color.white;
            te.a = 0.3f;

            for (int i = 0; i < nrUnits; i++)
            {
                unitList[i].net.SetKnowledge(0);
                unitList[i].transform.position = start.transform.position;
                unitList[i].target = start.transform.position;
                unitList[i].goal = goal[0];
                unitList[i].GetComponent<SpriteRenderer>().color = te;
            }
        }
    }  

    void CalcAndMove(Unit unit)
    {
        //Räkna ut knowledge
        float relX = 0;
        float relY = 0;
        if (pathfindingToggled)
        {
            relX = unit.goal.transform.position.x - unit.transform.position.x;
            relY = unit.goal.transform.position.y - unit.transform.position.y;
        }
        Vector2 normRel = new Vector2(relX, relY);
        normRel.Normalize();
        float left = -1, right = -1, up = -1, down = -1;
        float range = 0.3f;
        unit.lastWord = "none";

        //Left check
        RaycastHit2D hit = Physics2D.Raycast(unit.transform.position - new Vector3(0,0.18f,0),Vector2.left, range);
        if (hit.collider != null && hit.transform.gameObject.tag != "unit")
        {
            left = 1;
            unit.lastWord = "left";
            unit.closeCollisions++;
        }
        else
        {
            hit = Physics2D.Raycast(unit.transform.position - new Vector3(0, -0.18f, 0), Vector2.left, range);
            if (hit.collider != null && hit.transform.gameObject.tag != "unit")
            {
                left = 1;
                unit.lastWord = "left";
                unit.closeCollisions++;
            }
            else
            {
                hit = Physics2D.Raycast(unit.transform.position - new Vector3(0.15f, 0, 0), Vector2.left, range);
                if (hit.collider != null && hit.transform.gameObject.tag != "unit")
                {
                    left = 1;
                    unit.lastWord = "left";
                    unit.closeCollisions++;
                }
            }
        }

        //Right check
        hit = Physics2D.Raycast(unit.transform.position + new Vector3(0, -0.18f, 0), Vector2.right, range);
        if (hit.collider != null && hit.transform.gameObject.tag != "unit")
        {
            right = 1;
            unit.lastWord = "right";
            unit.closeCollisions++;

        }
        else
        {
            hit = Physics2D.Raycast(unit.transform.position + new Vector3(0, 0.18f, 0), Vector2.right, range);
            if (hit.collider != null && hit.transform.gameObject.tag != "unit")
            {
                right = 1;
                unit.lastWord = "right";
                unit.closeCollisions++;

            }
            else
            {
                hit = Physics2D.Raycast(unit.transform.position + new Vector3(0.15f, 0, 0), Vector2.right, range);
                if (hit.collider != null && hit.transform.gameObject.tag != "unit")
                {
                    right = 1;
                    unit.lastWord = "right";
                    unit.closeCollisions++;

                }
            }
        }

        //Up check
        hit = Physics2D.Raycast(unit.transform.position + new Vector3(0.18f, 0, 0), Vector2.up, range);
        if (hit.collider != null && hit.transform.gameObject.tag != "unit")
        {
            up = 1;
            unit.lastWord = "up";
            unit.closeCollisions++;

        }
        else
        {
            hit = Physics2D.Raycast(unit.transform.position + new Vector3(-0.18f, 0, 0), Vector2.up, range);
            if (hit.collider != null && hit.transform.gameObject.tag != "unit")
            {
                up = 1;
                unit.lastWord = "up";
                unit.closeCollisions++;

            }
            else
            {
                hit = Physics2D.Raycast(unit.transform.position + new Vector3(0, 0.15f, 0), Vector2.up, range);
                if (hit.collider != null && hit.transform.gameObject.tag != "unit")
                {
                    up = 1;
                    unit.lastWord = "up";
                    unit.closeCollisions++;

                }
            }
        }
        //Down check
        hit = Physics2D.Raycast(unit.transform.position - new Vector3(-0.18f, 0, 0), Vector2.down, range);
        if (hit.collider != null && hit.transform.gameObject.tag != "unit")
        {
            down = 1;
            unit.lastWord = "down";
            unit.closeCollisions++;

        }
        else
        {
            hit = Physics2D.Raycast(unit.transform.position - new Vector3(0.18f, 0, 0), Vector2.down, range);
            if (hit.collider != null && hit.transform.gameObject.tag != "unit")
            {
                down = 1;
                unit.lastWord = "down";
                unit.closeCollisions++;

            }
            else
            {
                hit = Physics2D.Raycast(unit.transform.position - new Vector3(0, 0.15f, 0), Vector2.down, range);
                if (hit.collider != null && hit.transform.gameObject.tag != "unit")
                {
                    down = 1;
                    unit.lastWord = "down";
                    unit.closeCollisions++;

                }
            }
        }
        //Debug.Log(left + ", " + right + ", " + up + ", " + down);
        
        float[] inputs = new float[] { normRel.x, normRel.y, left, right, up, down};
        float[] outputs = unit.net.FeedForward(inputs);

        if (outputs[0] > outputs[1] && outputs[0] > outputs[2] && outputs[0] > outputs[3] && !unit.wentLeft)
        {
            unit.wentRight = true;
            unit.wentLeft = false;
            unit.wentUp = false;
            unit.wentDown = false;
            unit.lastTarget = unit.target;
            unit.target = unit.transform.position + new Vector3(0.26f, 0, 0);
            unit.lastPos = unit.transform.position;
        }
        else if (outputs[1] > outputs[2] && outputs[1] > outputs[3] && !unit.wentRight)
        {
            unit.wentLeft = true;
            unit.wentRight = false;
            unit.wentUp = false;
            unit.wentDown = false;
            unit.lastTarget = unit.target;
            unit.target = unit.transform.position + new Vector3(-0.26f, 0, 0);
            unit.lastPos = unit.transform.position;

        }
        else if (outputs[2] > outputs[3] && !unit.wentDown)
        {
            unit.wentUp = true;
            unit.wentLeft = false;
            unit.wentRight = false;
            unit.wentDown = false;
            unit.lastTarget = unit.target;
            unit.target = unit.transform.position + new Vector3(0, 0.26f, 0);
            unit.lastPos = unit.transform.position;
        }
        else if(!unit.wentUp)
        {
            unit.wentDown = true;
            unit.wentLeft = false;
            unit.wentUp = false;
            unit.wentRight = false;
            unit.lastTarget = unit.target;
            unit.target = unit.transform.position + new Vector3(0, -0.26f, 0);
            unit.lastPos = unit.transform.position;
        }
        else
        {
            if (outputs[0] > outputs[1] && outputs[0] > outputs[2] && !unit.wentLeft)
            {
                unit.wentRight = true;
                unit.wentLeft = false;
                unit.wentUp = false;
                unit.wentDown = false;
                unit.lastTarget = unit.target;
                unit.target = unit.transform.position + new Vector3(0.26f, 0, 0);
                unit.lastPos = unit.transform.position;
            }
            else if (outputs[1] > outputs[2] && !unit.wentRight)
            {
                unit.wentLeft = true;
                unit.wentRight = false;
                unit.wentUp = false;
                unit.wentDown = false;
                unit.lastTarget = unit.target;
                unit.target = unit.transform.position + new Vector3(-0.26f, 0, 0);
                unit.lastPos = unit.transform.position;

            }
            else if (!unit.wentDown)
            {
                unit.wentUp = true;
                unit.wentLeft = false;
                unit.wentRight = false;
                unit.wentDown = false;
                unit.lastTarget = unit.target;
                unit.target = unit.transform.position + new Vector3(0, 0.26f, 0);
                unit.lastPos = unit.transform.position;
            }
            else
            {
                Debug.Log("MEN ÅHHHHHHHH");
            }
        }
        
    }

	void CreateNewGeneration ()
	{
        if (reachedEnd)
        {
            reachedEndCount++;
            reachedEnd = false;
        }
        else
            reachedEndCount = 0;

        if (reachedEndCount>6)
        {
            string timedate = "_"+System.DateTime.Now.Hour+"-"+System.DateTime.Now.Minute/*+"_"+System.DateTime.Now.Year + ":" + System.DateTime.Now.Month + ":" + System.DateTime.Now.Day*/;
            Save("auto"+generationNumber+timedate);
            reachedEndCount = 0;
        }

        timer = totalTimer;
        try
        {
            for (int i = 0; i < nrColliders; i++)
            {
                Destroy(colliders[0]);
                colliders.RemoveAt(0);
            }
        }
        catch { }

        //CollidersCheck
        if (collidersToggle)
        {
            for (int i = 0; i < nrColliders; i++)
            {
                Vector3 pos = new Vector3(UnityEngine.Random.Range(0, xSpawn) * 0.26f + xSpawnMin, (UnityEngine.Random.Range(0, ySpawn) * -0.26f + ySpawnMin), 0);
                GameObject unitP = ((GameObject)Instantiate(collisionPrefab, pos, collisionPrefab.transform.rotation));
                colliders.Add(unitP);
            }
        }

        pathfindingToggled = pathfindingToggle;

        //PathfindingCheck
        if (!pathfindingToggle)
        {
            for (int i = 0; i < 4; i++)
            {
                Color tmp = goal[i].GetComponent<SpriteRenderer>().color;
                tmp.a = 0f;
                goal[i].GetComponent<SpriteRenderer>().color = tmp;
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                Color tmp = goal[i].GetComponent<SpriteRenderer>().color;
                tmp.a = 1f;
                goal[i].GetComponent<SpriteRenderer>().color = tmp;
            }
        }

        for (int i = 0; i < nrUnits; i++)
        {
            //unitList[i].net.AddKnowledge(unitList[i].closeCollisions*unitList[i].net.GetKnowledge()*0.1f);
            unitList[i].net.AddKnowledge(100 - Vector3.Distance(unitList[i].transform.position, unitList[i].goal.transform.position));
            unitList[i].closeCollisions = 0;
            unitList[i].transform.rotation = new Quaternion();
            unitList[i].goal = goal[0];
            unitList[i].colliding = false;
            unitList[i].wentLeft = false;
            unitList[i].wentRight = false;
            unitList[i].wentUp = false;
            unitList[i].wentDown = false;
            unitList[i].name = "Unit (Clone)";
            Color te = Color.white;
            te.a = 0.3f;
            unitList[i].GetComponent<SpriteRenderer>().color = te;
        }
            //Sortera dem efter effektivitet
            networks.Sort();
        
        //Rensa bort den sämre hälften
        networks.RemoveRange(0, nrUnits / 2);
        List<NeuralNetwork> newNetworks = new List<NeuralNetwork>();
        //Ta två parents och generera fyra barn, och ersätt dem.
        for(int i = 0; i<networks.Count; i=i+2)
        {
            float[] parent1Layer1 = networks[i].GetWeights(1);
            float[] parent1Layer2 = networks[i].GetWeights(2);
            float[] parent1Layer3 = networks[i].GetWeights(3);

            float[] parent2Layer1 = networks[i+1].GetWeights(1);
            float[] parent2Layer2 = networks[i+1].GetWeights(2);
            float[] parent2Layer3 = networks[i+1].GetWeights(3);

            int arraySize = layers[0] * layers[1] + layers[1] * layers[2] + layers[2] * layers[3];

            for (int j = 0; j < 4; j=j+2)
            {
                int rand1 = UnityEngine.Random.Range(0, arraySize);

                float[] child1_1 = new float[rand1];
                float[] child1_2 = new float[arraySize - rand1];
                float[] child2_1 = new float[rand1];
                float[] child2_2 = new float[arraySize - rand1];


                float[] extendedArray1 = new float[arraySize];
                float[] extendedArray2 = new float[arraySize];

                parent1Layer1.CopyTo(extendedArray1, 0);
                parent1Layer2.CopyTo(extendedArray1, parent1Layer1.Length);
                parent1Layer3.CopyTo(extendedArray1, parent1Layer1.Length + parent1Layer2.Length);

                parent2Layer1.CopyTo(extendedArray2, 0);
                parent2Layer2.CopyTo(extendedArray2, parent2Layer1.Length);
                parent2Layer3.CopyTo(extendedArray2, parent2Layer1.Length + parent2Layer2.Length);

                
                child1_1 = extendedArray1.Take(rand1).ToArray();
                child1_2 = extendedArray1.Skip(rand1).ToArray();

                child2_1 = extendedArray2.Take(rand1).ToArray();
                child2_2 = extendedArray2.Skip(rand1).ToArray();

                float[] newLayer1 = new float[arraySize];
                float[] newLayer2 = new float[arraySize];

                child1_1.CopyTo(newLayer1, 0);
                child2_1.CopyTo(newLayer2, 0);

                child2_2.CopyTo(newLayer1, rand1);
                child1_2.CopyTo(newLayer2, rand1);

                NeuralNetwork net1 = new NeuralNetwork(layers);
                net1.SetWeights(newLayer1);

                NeuralNetwork net2 = new NeuralNetwork(layers);
                net2.SetWeights(newLayer2);

                if (mutateUnits)
                {
                    net1.Mutate();
                    net2.Mutate();
                }
                unitList[i*2 + j].net = net1;
                unitList[i * 2 + j + 1].net = net2;

                newNetworks.Add(unitList[i * 2 + j].net);
                newNetworks.Add(unitList[i * 2 + j + 1].net);

            }

        }

        networks.RemoveRange(0, networks.Count());

        networks = newNetworks;
        
        //Reset Knowledgemätare
        for (int i = 0; i<nrUnits; i++)
        {
            unitList[i].timeTaken = 0;
            unitList[i].net.SetKnowledge(0);
            unitList[i].transform.position = start.transform.position;
        }
    }

    void Save(string fileName)
    {
        int arraySize = layers[0] * layers[1] + layers[1] * layers[2] + layers[2] * layers[3];

        string[] output = new string[arraySize*nrUnits+1];
        List<string> lines = new List<string>();
        lines.Add(generationNumber.ToString());
        for (int i = 0; i<nrUnits; i++)
        {
            float[] weights1 = networks[i].GetWeights(1);
            float[] weights2 = networks[i].GetWeights(2);
            float[] weights3 = networks[i].GetWeights(3);

            float[] totalArray = new float[arraySize];

            weights1.CopyTo(totalArray, 0);
            weights2.CopyTo(totalArray, weights1.Length);
            weights3.CopyTo(totalArray, weights1.Length+weights2.Length);

            for(int j = 0; j < arraySize; j++)
            {
                lines.Add(totalArray[j].ToString());
            }
        }

        output = lines.ToArray();
        try
        {
            System.IO.File.WriteAllLines((Application.dataPath)+"/Saves/" + fileName + ".txt", output);
        }
        catch
        {
            System.IO.Directory.CreateDirectory((Application.dataPath) + "/Saves/");
            System.IO.File.WriteAllLines((Application.dataPath) + "/Saves/" + fileName + ".txt", output);
        }
    }

    void Load(string fileName)
    {
        int arraySize = layers[0] * layers[1] + layers[1] * layers[2] + layers[2] * layers[3];
        try {
            string[] input = System.IO.File.ReadAllLines((Application.dataPath) + "/Saves/" + fileName + ".txt");

            generationNumber = Int32.Parse(input[0]) + 1;
            GameObject canvasObject = GameObject.FindGameObjectWithTag("canvas");
            Transform child = canvasObject.transform.Find("Text");
            Text generationText = child.GetComponent<Text>();
            generationText.text = "Generation: " + generationNumber.ToString();

            input = input.Skip(1).ToArray();
            for (int i = 0; i < nrUnits; i++)
            {
                float[] totalArray = new float[arraySize];

                float[] weights1 = new float[layers[0] * layers[1]];
                float[] weights2 = new float[layers[1] * layers[2]];
                float[] weights3 = new float[layers[2] * layers[3]];

                for (int j = 0; j < layers[0] * layers[1]; j++)
                {
                    weights1[j] = Single.Parse(input[i * arraySize + j]);
                }
                for (int j = 0; j < layers[1] * layers[2]; j++)
                {
                    weights2[j] = Single.Parse(input[i * arraySize + layers[0] * layers[1] + j]);
                }
                for (int j = 0; j < layers[2] * layers[3]; j++)
                {
                    weights3[j] = Single.Parse(input[i * arraySize + layers[0] * layers[1] + layers[1] * layers[2] + j]);
                }

                weights1.CopyTo(totalArray, 0);
                weights2.CopyTo(totalArray, weights1.Length);
                weights3.CopyTo(totalArray, weights1.Length + weights2.Length);

                unitList[i].net.SetWeights(totalArray);
            }
        }
        catch { }
    }
    


}
