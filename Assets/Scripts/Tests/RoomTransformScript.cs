using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Material))]
[RequireComponent(typeof(Dropdown))]
[RequireComponent(typeof(Text))]
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Slider))]
public class RoomTransformScript : MonoBehaviour {
	public enum DayState
	{
		DAWN=0,
		MIDDAY=1,
		EVENING=2,
		DUSK=3,
		MIDNIGHT=4
	}

	public enum TransitionType
	{
		REAL_TIME,
		GAME_TIME,
        EVENT_TIME
	}

    static private RoomTransformScript _S;

    //material array to house skyboxes
    public Material[] skyboxes = new Material[5];
    public Dropdown dropdown;
    public Text transitionText;
    public Text dayStateText;
    public Text dayModifierText;
    public Text dayLengthText;
    public Button eventTimeButton;
    public Slider gameTimeSlider;
    public Slider realTimeSlider;

    //consider edits to these and possibly set public to set initial states
    static private DayState dayState;
	static private TransitionType transitionType;

	public string changeKey;//hold value of button to cause transition -- temp

    public float skyRotationSpeed = 1.0f;//value managing the speed background rotates at

    //used for determining game timing transitions
    public float gameTimeDuration = 0.01f;
    private float currGameTime;

    //used for handling system time transitions
    public int timeHourModifier = 0;

    //used for letting system know button has been pressed
    private bool buttonPressed;

    /*
	color schema change for lighting
	-dawn - pink
	-morning - yellow
	-midday - white/skyblue
	-afternoon - skyblue
	-dusk - orange
	-evening - purple
	-midnight - black
	*/

    // Use this for initialization
    void Start () {
        //set listeners for each of the main UI components
        dropdown.onValueChanged.AddListener(delegate {
            SetTransitionType(dropdown.value);
        });

        eventTimeButton.onClick.AddListener(delegate {
            ButtonPressed();
        });

        gameTimeSlider.onValueChanged.AddListener(delegate {
            SetGameTimeDay(gameTimeSlider.value);
        });

        realTimeSlider.onValueChanged.AddListener(delegate {
            SetRealTimeModifier((int)(realTimeSlider.value));
        });

        //set base transition data
        transitionType = TransitionType.REAL_TIME;
        transitionText.text = "Real Time";

        //call reset method to set intial values
        Reset();
    }

    // Update is called once per frame
    void Update () {
        //check on transition type
        if(transitionType == TransitionType.REAL_TIME){
            SystemTime();
        } else if(transitionType == TransitionType.GAME_TIME){
            GameTime();
        } else if(transitionType == TransitionType.EVENT_TIME){
            EventTime();
        } 

        //rotate skybox
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * skyRotationSpeed);
    }

    //method used to reset key values
    private void Reset(){
        dayState = DayState.DAWN;
        dayStateText.text = "Current State: " + dayState;
        RenderSettings.skybox = skyboxes[(int)dayState];
        currGameTime = 0.0f; 
        gameTimeDuration = 0.01f;
        buttonPressed = false;
        dayModifierText.text = "Real Time Modifier: " + timeHourModifier;
        dayLengthText.text = "Game Time Span (Hours): " + gameTimeDuration;
    }

    //reset skybox on quit
    private void OnApplicationQuit()
    {
        RenderSettings.skybox = skyboxes[0];
    }

    //set the transition type based on value
    public void SetTransitionType(int type){
        if (type == 0){
            transitionType = TransitionType.REAL_TIME;
            transitionText.text = "Real Time";
            Reset();
        } else if (type == 1){
            transitionType = TransitionType.GAME_TIME;
            transitionText.text = "Game Time";
            Reset();
        } else if (type == 2){
            transitionType = TransitionType.EVENT_TIME;
            transitionText.text = "Event Time";
            Reset();
        } else {
            Debug.Log("Bad Transition Type Request");
        }
    }

    //method to set button as pressed for event time
    public void ButtonPressed(){
        buttonPressed = true;
    }

    //method to set the modifier to be used by real time
    public void SetRealTimeModifier(int modValue)
    {
        timeHourModifier = modValue;
        dayModifierText.text = "Real Time Modifier: " + timeHourModifier;
    }

    //method to set the amount of time is a day in game
    public void SetGameTimeDay(float durValue)
    {
        gameTimeDuration = (float)Math.Round(durValue, 3);
        dayLengthText.text = "Game Time Span (Hours): " + gameTimeDuration;
    }

    //method to manage time by system time
    private void SystemTime()
    {
        //get time with modifier
        //set day state based on time with modified
        DateTime dateTime = DateTime.Now;
        string formattedTime = dateTime.ToString("HH");

        int hoursToInt = Int32.Parse(formattedTime);

        int modPlusHours = timeHourModifier + hoursToInt - 4;

        if(modPlusHours < 0){
            modPlusHours = 24 + modPlusHours;
        }

        int gameTime = modPlusHours % 24;

        if(dayState != (DayState)(gameTime / 5)){
            dayState = (DayState)(gameTime / 5);

            Debug.Log("Current State: " + dayState);
            dayStateText.text = "Current State: " + dayState;

            RenderSettings.skybox = skyboxes[(int)dayState];
        }
    }

    //method to manage time by game time
    private void GameTime()
    {
        //check if game time is over interval
        //if over interval then call day transition
        float incrementTime = (gameTimeDuration * 60 * 60) / 5;

        currGameTime += Time.deltaTime;

        if(currGameTime >= incrementTime){
            currGameTime = 0;
            DayTransition();
        }
    }

    //method to manage time by event triggers
    private void EventTime()
    {
        //check if button has been pressed
        //if button pressed then call day transition
        if (Input.GetKeyUp(changeKey))
        {
            buttonPressed = true;
        }

        if(buttonPressed){
            buttonPressed = false;
            DayTransition();
        }
    }

    //function calls to manage the transition of day phases based on respective criteria
    private void DayTransition () 
	{
        //increment time of day
        if ((int)dayState >= 4)
            dayState = (DayState)0;
        else
            dayState++;

        Debug.Log("Current State: " + dayState);
        dayStateText.text = "Current State: " + dayState;

        //set skybox
        RenderSettings.skybox = skyboxes[(int)dayState];
    }

    //singleton for script
    static private RoomTransformScript S
    {
        get
        {
            if (_S == null)
            {
                Debug.LogError("Unable to get script before it has been set");
                return null;
            }

            return _S;
        }

        set
        {
            if (_S != null)
            {
                Debug.LogError("Script has already been set");
            }

            _S = value;
        }
    }
}