using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Terra.SerializedData.Entities
{
    public class TerraEntity
    {
        public delegate void LabelDelegate(TerraEntity entity, string label);

        public event LabelDelegate OnLabelRemoved;
        public event LabelDelegate OnLabelAdded;
        
        public int InstanceId { get; set; } = GUID.Generate().GetHashCode();
        public HashSet<string> Labels { get; set; } = new HashSet<string>();
        public string Type { get; set; } = "";

        public override int GetHashCode()
        {
            return InstanceId;
        }

        public void AddLabel(string label)
        {
            Labels.Add(label);
            OnLabelAdded?.Invoke(this, label);
        }

        public void RemoveLabel(string label)
        {
            Labels.Remove(label);
            OnLabelRemoved?.Invoke(this, label);
        }

        public bool HasLabel(string label)
        {
            return Labels.Contains(label);
        }
    }
}