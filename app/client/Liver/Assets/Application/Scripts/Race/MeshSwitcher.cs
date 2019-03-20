using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSwitcher : MonoBehaviour
{

    public MeshFilter target;
    public Mesh[] meshes;
    public int Current { get; private set; }

    public void Set(int index)
    {
        Current = index;
        target.mesh = meshes[index];
    }
}
