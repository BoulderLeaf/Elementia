using UnityEngine;
using System.Collections;

public abstract class InputPuppet : MonoBehaviour
{
    public abstract void PuppetUpdate();
    public virtual void PuppetFocusOn() { }
    public virtual void PuppetFocusOff() { }
}