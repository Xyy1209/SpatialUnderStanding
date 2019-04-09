using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using System;

public class ObjectCollectionManager : Singleton<ObjectCollectionManager>
{
    //存储欲放置方建筑物列表
    public List<GameObject> SquareBuildingPrefabs;
    public Vector3 SquareBuildingSize = new Vector3(.5f, .5f, .5f);

    //存储欲放置宽建筑物的列表
    public List<GameObject> WideBuildingPrefabs;
    public Vector3 WideBuildingSize = new Vector3(1.0f, .5f, .5f);

    //欲存放高建筑物列表
    public List<GameObject> TallBuildingPrefabs;
    public Vector3 TallBuildingSize = new Vector3(.25f,.05f,.05f);

    //欲存放树
    public List<GameObject> TreePrefabs;
    public Vector3 TreeSize = new Vector3(.25f,.5f,.25f);


    public float ScaleFactor;

    //存放已实例化的物体
    public List<GameObject> ActiveHolograms = new List<GameObject>();



    public void CreateSquareBuilding(int number,Vector3 positionCenter,Quaternion rotation)
    {
        CreateBuilding(SquareBuildingPrefabs[number], positionCenter, rotation, SquareBuildingSize);
    }
   


    public void CreateWideBuilding(int number, Vector3 positionCenter, Quaternion rotation)
    {
        CreateBuilding(WideBuildingPrefabs[number], positionCenter, rotation, WideBuildingSize);
    }


    public void CreateTallBuilding(int number,Vector3 positionCenter,Quaternion rotation)
    {
        CreateBuilding(TallBuildingPrefabs[number], positionCenter, rotation, TallBuildingSize);
    }


    //创建建筑物
    private void CreateBuilding(GameObject buildingToCreate, Vector3 positionCenter, Quaternion rotation, Vector3 desiredSize)
    {
        var position = positionCenter - new Vector3(0, desiredSize.y * .5f, 0);

        GameObject newObject = Instantiate(buildingToCreate, position, rotation) as GameObject;

        if (newObject != null)
        {
            newObject.transform.parent = gameObject.transform;

            newObject.transform.localScale = RescaleToSameScaleFactor(buildingToCreate);
            ActiveHolograms.Add(newObject);
        }

    }


    //创建树
    public void CreateTree(int number,Vector3 positionCenter,Quaternion rotation)
    {
        var position = positionCenter - new Vector3(0, TreeSize.y * .5f, 0);

        GameObject newObject = Instantiate(TreePrefabs[number], position, rotation);

        if(newObject !=null)
        {
            newObject.transform.parent = gameObject.transform;

            newObject.transform.localScale = RescaleToSameScaleFactor(TreePrefabs[number]);
            ActiveHolograms.Add(newObject);
        }
    }





    //给物体及其子组件增加Mesh Collider网格碰撞器
    private void AddMeshColliderToAllChildren(GameObject obj)
    {
        for(int i=0;i<obj.transform.childCount;i++)
        {
            obj.transform.GetChild(i).gameObject.AddComponent<MeshCollider>();
        }
    }




    /*
    //单例类的public方法可以在其他脚本中直接通过 ——类名.Instance.方法名 ——调用。
    public void CreateGround(GameObject groundToCreate, Vector3 positionCenter, Quaternion rotation, Vector3 size)
    {
        var position = positionCenter - new Vector3(0, size.y * .5f, 0);

        GameObject newObject = Instantiate(groundToCreate, position, rotation) as GameObject;

        if (newObject != null)
        {
            newObject.transform.parent = gameObject.transform;

            newObject.transform.localScale = StretchToFit(groundToCreate,size);
            ActiveHolograms.Add(newObject);
        }

    }
    */



    //给所有欲放置的物体相同的缩放因子
    private Vector3 RescaleToSameScaleFactor(GameObject objectToScale)
    {
        if (ScaleFactor == 0.0f)
        {
            CalculateScaleFactor();
        }

        return objectToScale.transform.localScale * ScaleFactor;
    }




    //伸展适应？？？不懂!!!
    private Vector3 StretchToFit(GameObject obj, Vector3 desiredSize)
    {
        var curBounds=GetBoundsForAllChildren(obj).size;

        return new Vector3(desiredSize.x / curBounds.x / 2, desiredSize.y, desiredSize.z / curBounds.z / 2);
    }




    private void CalculateScaleFactor()
    {
        float maxScale = float.MaxValue;

        var ratio = CalculateScaleFactorHelper(WideBuildingPrefabs, WideBuildingSize);

        if(ratio<maxScale)
        {
            maxScale = ratio;
        }

        ScaleFactor = maxScale;

    }



    private float CalculateScaleFactorHelper(List<GameObject> objects, Vector3 desiredSize)
    {
        float maxScale = float.MaxValue;
        

        foreach (var obj in objects)
        {
            float ratio;

            var curBounds = GetBoundsForAllChildren(obj).size;
            var differnece = curBounds - desiredSize;

            if(differnece.x>differnece.y && differnece.x>differnece.z)
            { ratio = desiredSize.x / curBounds.x; }
            else if(differnece.y >differnece.x && differnece.y>differnece .z)
            { ratio = desiredSize.y / curBounds.y;}
            else
            { ratio = desiredSize.z / curBounds.z; }


            if (ratio < maxScale)
                maxScale = ratio;
        }

        return maxScale;

        
    }



    private Bounds GetBoundsForAllChildren(GameObject findMyBounds)
    {
        Bounds result = new Bounds(Vector3.zero, Vector3.zero);

        foreach (var curRenderer in findMyBounds.GetComponentsInChildren<Renderer>())
        {
            if (result.extents == Vector3.zero)
            {
                result = curRenderer.bounds;
            }

            else
            {
                result.Encapsulate(curRenderer.bounds);
            }
        }

        return result;
    }


	
}
