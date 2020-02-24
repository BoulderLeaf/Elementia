using UnityEngine;
using ViewControllers;

public class ElementiaGameStart : MonoBehaviour
{
    private void Start()
    {
        ElementiaViewController vc =new ElementiaViewController();
        vc.ShowView();
    }
}
