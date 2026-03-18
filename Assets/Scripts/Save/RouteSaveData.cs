using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HoldSaveData
{
    public string holdId;
    public string instanceSaveId;
    public string parentInstanceSaveId;
    public Vector3 position;
    public Quaternion rotation;
    public float rotationAngle;
    public bool isLocked;
    public Vector3 lastWallNormal;
}

[Serializable]
public class RouteSaveData
{
    public List<HoldSaveData> holds;
}
