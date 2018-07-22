using UnityEngine;
using System.Collections;

public class PlayerForestController : InputMaster
{
    [SerializeField]
    private ForestAgentPuppet _puppet;

    private bool _raiseEarthButtonDown;
    private bool _lowerEarthButtonDown;
    private bool _makeFlatButtonDown;
    private bool _addWaterButtonDown;

    // Update is called once per frame
    void Update()
    {
        if(_puppet != null)
        {
            float yAxis = Input.GetAxis("Vertical");
            float xAxis = Input.GetAxis("Horizontal");
            _puppet.Move(xAxis, yAxis);
        }

        if(!_makeFlatButtonDown && Input.GetKeyDown(KeyCode.O) && Input.GetKeyUp(KeyCode.P))
        {
            _puppet.MakeFlatEarth();
            _makeFlatButtonDown = true;
            _lowerEarthButtonDown = false;
            _raiseEarthButtonDown = false;
        }

        if (_lowerEarthButtonDown && Input.GetKeyUp(KeyCode.O) && Input.GetKeyUp(KeyCode.P))
        {
            _lowerEarthButtonDown = false;
            _makeFlatButtonDown = false;
        }
        if (!_lowerEarthButtonDown && Input.GetKeyDown(KeyCode.O) && !_makeFlatButtonDown)
        {
            _lowerEarthButtonDown = true;
            _puppet.LowerEarth();
        }

        if (_raiseEarthButtonDown && Input.GetKeyUp(KeyCode.P) && Input.GetKeyUp(KeyCode.O))
        {
            _raiseEarthButtonDown = false;
            _makeFlatButtonDown = false;
        }
        if (!_raiseEarthButtonDown && Input.GetKeyDown(KeyCode.P) && !_makeFlatButtonDown)
        {
            _raiseEarthButtonDown = true;
            _puppet.RaiseEarth();
        }

        if (_addWaterButtonDown && Input.GetKeyUp(KeyCode.I))
        {
            _addWaterButtonDown = false;
        }
        if (!_addWaterButtonDown && Input.GetKeyDown(KeyCode.I))
        {
            _addWaterButtonDown = true;
            _puppet.AddWater();
        }
    }

    public override void FocusOn()
    {
        base.FocusOn();
        _puppet.PuppetFocusOn();
    }

    public override void FocusOff()
    {
        base.FocusOff();
        _puppet.PuppetFocusOff();
    }
}
