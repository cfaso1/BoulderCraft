using UnityEngine;

public class SpawnHoldCommand : ICommand
{
    readonly GameObject prefab;
    readonly Vector3 position;
    readonly Quaternion rotation;
    readonly Vector3 scale;
    readonly Vector3 wallNormal;
    readonly Transform parent;
    readonly string instanceSaveId;
    GameObject spawned;

    public SpawnHoldCommand(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, Vector3 wallNormal, Transform parent)
    {
        this.prefab = prefab;
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        this.wallNormal = wallNormal;
        this.parent = parent;
        instanceSaveId = System.Guid.NewGuid().ToString();
    }

    public void Execute()
    {
        if (spawned == null)
        {
            spawned = Object.Instantiate(prefab, position, rotation);
            spawned.transform.localScale = scale;
            spawned.transform.SetParent(parent, true);

            var b = spawned.GetComponent<HoldBehavior>();
            if (b != null)
            {
                b.isLocked = false;
                b.lastWallNormal = wallNormal;
                b.instanceSaveId = instanceSaveId;
            }
        }
        else
        {
            spawned.SetActive(true);
        }
    }

    public void Undo()
    {
        if (PlacementManager.Instance.SelectedHold == spawned)
            PlacementManager.Instance.Deselect();
        spawned.SetActive(false);
    }
}
