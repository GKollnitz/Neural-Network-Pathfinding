using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour {

    private float speed = 1f;
    public NeuralNetwork net;
    public Vector3 target;
    public Vector3 lastPos;
    public Vector3 lastTarget;
    public bool doneMoving = false;
    public bool atGoal = false;
    private Slider speedSlider;
    public GameObject goal;
    public bool colliding = false;
    public float timeTaken = 0;
    public int closeCollisions = 0;
    public string lastWord = "none";
    public float knowledge = 0;
    public bool wentLeft = false, wentRight = false, wentUp = false, wentDown = false;

    // Use this for initialization
    void Start () {
		target = transform.position;
        GameObject canvasObject = GameObject.FindGameObjectWithTag("canvas");
        Transform child = canvasObject.transform.Find("Slider");
        speedSlider = child.GetComponent<Slider>();

    }
	
	// Update is called once per frame
	void Update () {
        timeTaken += Time.deltaTime*speed;
        speed = speedSlider.value;
        knowledge = net.GetKnowledge();

        if (colliding)
        {
            if(lastWord == "none")
            {
                colliding = false;
                transform.position = target;
            }
        }
        else if (Vector3.Distance(transform.position, goal.transform.position)<0.2)
        {
            atGoal = true;
        }
        else if (Vector3.Distance(transform.position, target) > (0.1))
            Move();
        else
        {
            doneMoving = true;
        }
	}

	public void Init(NeuralNetwork net)
    {
        this.net = net;
    }

    public void Move()
    {
        
		Vector3 move = Vector3.Normalize(target-transform.position)*Time.deltaTime*speed;
		transform.Translate(move);

    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.collider.gameObject.layer == 8)
        {
            colliding = true;
            Color newColor = Color.red;
            newColor.a = 0.3f;
            gameObject.GetComponent<SpriteRenderer>().color = newColor;
        }


    }
}


