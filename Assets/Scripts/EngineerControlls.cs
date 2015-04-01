using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EngineerControlls : MonoBehaviour {
    
    //
    private float shieldPower;
    private float enginePower;
    private float turretPower;
    
    //the visual sliders in the GUI
    public Slider shieldSlider;
    public Slider engineSlider;
    public Slider turretSlider;

    //server objects
    SFSHandler sfsScript; //SFS-script
    GameObject serverObject;

	// Use this for initialization
	void Start () {
        //access the server script
        sfsScript = GetComponent<SFSHandler>();
        serverObject = GameObject.Find("Server");
        sfsScript = (SFSHandler)serverObject.GetComponent(typeof(SFSHandler));
	}
	
	// Update is called once per frame
	void Update () {

    
    
    }

    //client funtions which handles input from the GUI
    //***************************************************************************
    public void setShieldValue(float _power) {
        shieldPower = _power;
        sfsScript.requestShieldPowerTransfer(_power); // update server
    }

    public void setEngineValue(float _power) {
        enginePower = _power;
        sfsScript.requestEnginePowerTransfer(_power); // update server
    }

    public void setTurretValue(float _power) {
        turretPower = _power;
        sfsScript.requestTurretPowerTransfer(_power); // update server
    }
     
    //funtions that updates GUI based on server information
    //***************************************************************************
    public void updatePowerInfo(float _shield, float _enginge, float _turret) {
        shieldSlider.value = _shield;
        engineSlider.value = _enginge;
        turretSlider.value = _turret;
    }
}
