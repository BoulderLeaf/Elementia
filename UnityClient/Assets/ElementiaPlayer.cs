using System.Collections;
using System.Collections.Generic;
using PandeaGames;
using Terra.MonoViews;
using Terra.Services;
using Terra.ViewModels;
using UnityEngine;

public class ElementiaPlayer : AbstractTerraMonoComponent
{
    private bool _raiseEarthButtonDown;
    private bool _lowerEarthButtonDown;
    private bool _makeFlatButtonDown;
    private bool _addWaterButtonDown;
    private void Update()
    {
        if (Input.GetKey(KeyCode.P) && !Input.GetKey(KeyCode.O))
        {
            RaiseEarth();
        }
        
        if (Input.GetKey(KeyCode.O) && !Input.GetKey(KeyCode.P))
        {
            LowerEarth();
        }
    }

    private void MakeFlatEarth()
    {
        
    }

    private void RaiseEarth()
    {
        TerraChunksViewModel vm = Game.Instance.GetViewModel<TerraChunksViewModel>(0);
        TerraVector vector = new TerraVector((int)transform.position.x, (int)transform.position.z);
        TerraPoint point = vm.CurrentChunk.GetFromWorld(vector);
        point.Height += 1;
        vm.CurrentChunk.SetFromWorld(vector, point);
    }

    private void LowerEarth()
    {
        TerraChunksViewModel vm = Game.Instance.GetViewModel<TerraChunksViewModel>(0);
        TerraVector vector = new TerraVector((int)transform.position.x, (int)transform.position.z);
        TerraPoint point = vm.CurrentChunk.GetFromWorld(vector);
        point.Height -= 1;
        vm.CurrentChunk.SetFromWorld(vector, point);
    }
    
    private void AddWater()
    {
        
    }
}
