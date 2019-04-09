using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;


public class Speech : MonoBehaviour {

    public SpatialUnderstandingCustomMesh spatialUnderstandingMesh;

    private Material _swapMaterial;



    void Start()
    {
        _swapMaterial = spatialUnderstandingMesh.MeshMaterial;   
    }


    //触发是否开启Draw已处理完毕的网格
    public void ToggleMesh()
    {
        //若还未扫描完成，不做操作
        if (SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Done)
            return;

        //交换材质
        //即放置物体成功后的Occlusion材质和初始时默认的材质进行交换，以实现“T”健改变材质
        var anotherMaterial = spatialUnderstandingMesh.MeshMaterial;
        spatialUnderstandingMesh.MeshMaterial = _swapMaterial;
        _swapMaterial = anotherMaterial;
    }

}
