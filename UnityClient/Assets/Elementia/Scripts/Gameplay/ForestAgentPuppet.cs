using UnityEngine;
using System.Collections;
using PandeaGames;

public class ForestAgentPuppet : InputPuppet
{
    [SerializeField]
    private ServiceManager _serviceManager;

    private WorldDataAccessService _worldDataAccessService;
    private Rigidbody _rigidBody;
    private int _radius = 3;
    private int _waterRadius = 1;

    public void Start()
    {
        _worldDataAccessService = Game.Instance.GetService<WorldDataAccessService>();
        _rigidBody = GetComponent<Rigidbody>();
    }

    public override void PuppetUpdate()
    {

    }

    public void Move(float x, float y)
    {
        _rigidBody.AddForce(new Vector3(x * 0.1f, 0, y * 0.1f), ForceMode.Impulse);
    }

    public void AddWater()
    {
        //_worldDataAccessService.GetToken(new TokenRequest((int)transform.position.x - _waterRadius, (int)transform.position.x + _waterRadius, (int)transform.position.z + _waterRadius, (int)transform.position.z - _waterRadius), AddWaterTokenRecievedComplete, () => { });
    }

    public void MakeFlatEarth()
    {
       // _worldDataAccessService.GetToken(new TokenRequest((int)transform.position.x - _radius, (int)transform.position.x + _radius, (int)transform.position.z + _radius, (int)transform.position.z - _radius), LowerEarthTokenRecievedComplete, () => { });
    }

    public void LowerEarth()
    {
       // _worldDataAccessService.GetToken(new TokenRequest((int)transform.position.x - _radius, (int)transform.position.x + _radius, (int)transform.position.z + _radius, (int)transform.position.z - _radius), LowerEarthTokenRecievedComplete, () => { });
    }

    public void RaiseEarth()
    {
       // _worldDataAccessService.GetToken(new TokenRequest((int)transform.position.x - _radius, (int)transform.position.x + _radius, (int)transform.position.z + _radius, (int)transform.position.z - _radius), RaiseEarthTokenRecievedComplete, () => { });
    }

    private void MakeFlatTokenRecievedComplete(WorldDataToken token)
    {
        ushort height = token.GetUshort(token.Request.left, token.Request.top, UshortDataID.HeightLayerData);
        for (int x = 0; x < token.Request.width; x++)
        {
            for (int y = 0; y < token.Request.height; y++)
            {
                token.SetUshort(x, y, height, UshortDataID.HeightLayerData);
            }
        }

       // _worldDataAccessService.SaveToken(token, (WorldDataToken returnToken) => { });
    }

    private void AddWaterTokenRecievedComplete(WorldDataToken token)
    {
        for (int x = 0; x < token.Request.width; x++)
        {
            for (int y = 0; y < token.Request.height; y++)
            {
                token.SetByte(x, y, 200, ByteDataLyerID.WaterLayerData);
            }
        }

      //  _worldDataAccessService.SaveToken(token, (WorldDataToken returnToken) => { });
    }

    private void RaiseEarthTokenRecievedComplete(WorldDataToken token)
    {
        for (int x = 0; x < token.Request.width; x++)
        {
            for (int y = 0; y < token.Request.height; y++)
            {
                token.SetUshort(x, y, (ushort)(token.GetUshort(x, y, UshortDataID.HeightLayerData) + 3), UshortDataID.HeightLayerData);
            }
        }

        //_worldDataAccessService.SaveToken(token, (WorldDataToken returnToken) => { });
    }

    private void LowerEarthTokenRecievedComplete(WorldDataToken token)
    {
        for (int x = 0; x < token.Request.width; x++)
        {
            for (int y = 0; y < token.Request.height; y++)
            {
                token.SetUshort(x, y, (ushort)(token.GetUshort(x, y, UshortDataID.HeightLayerData) - 3), UshortDataID.HeightLayerData);
            }
        }

       // _worldDataAccessService.SaveToken(token, (WorldDataToken returnToken) => { });
    }
}
