using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Terra.Entities
{
    public class TerraEntity : MonoBehaviour
    {
        public int InstanceId { get; set; } = GUID.Generate().GetHashCode();

        public override int GetHashCode()
        {
            return InstanceId;
        }
    }
}