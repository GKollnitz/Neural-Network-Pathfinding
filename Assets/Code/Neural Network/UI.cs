using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interface
{

    private Slider speedSlider;
    private Button saveButton;
    private Button loadButton;
    private Button newGenButton;
    public bool save;
    public bool load;
    public string fileName;
    public Text inputField;
    private Toggle mutateToggle;
    private Toggle pathToggle;
    private Toggle collToggle;
    public bool mutate;
    public bool pathfinding;
    public bool colliders;
    public bool newGen = false;
    // Use this for initialization

    public void Initialize()
    {
        mutate = true;
        pathfinding = true;
        colliders = true;

        save = false; load = false;
        GameObject canvasObject = GameObject.FindGameObjectWithTag("canvas");
        Transform child = canvasObject.transform.Find("Slider");
        speedSlider = child.GetComponent<Slider>();

        child = canvasObject.transform.Find("SaveButton");
        saveButton = child.GetComponent<Button>();
        saveButton.onClick.AddListener(SaveOnClick);

        child = canvasObject.transform.Find("LoadButton");
        loadButton = child.GetComponent<Button>();
        loadButton.onClick.AddListener(LoadOnClick);

        child = canvasObject.transform.Find("NewGenButton");
        newGenButton = child.GetComponent<Button>();
        newGenButton.onClick.AddListener(newGeneration);

        child = canvasObject.transform.Find("InputField/Text");
        inputField = child.GetComponent<Text>();

        child = canvasObject.transform.Find("Toggle");
        mutateToggle = child.GetComponent<Toggle>();
        mutateToggle.onValueChanged.AddListener((value)=>{ mutate = value; });

        child = canvasObject.transform.Find("TogglePath");
        pathToggle = child.GetComponent<Toggle>();
        pathToggle.onValueChanged.AddListener((value) => { pathfinding = value; });

        child = canvasObject.transform.Find("ToggleColl");
        collToggle = child.GetComponent<Toggle>();
        collToggle.onValueChanged.AddListener((value) => { colliders = value; });
    }

    public float getSpeed()
    {
        return speedSlider.value;
    }

    void newGeneration()
    {
        newGen = true;
    }

    void SaveOnClick()
    {
        save = true;
        fileName = inputField.text;
    }

    void LoadOnClick()
    {
        load = true;
        fileName = inputField.text;
    }

    public bool getOnClick()
    {
        return false;
    }
    
}
