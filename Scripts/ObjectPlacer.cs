using System;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;


public class ObjectPlacer : MonoBehaviour
{
    public bool DrawDebugBoxes = false;
    public bool DrawBuildings = true;
    public bool DrawTrees = true;

    public SpatialUnderstandingCustomMesh SpatialUnderstandingMesh;


    /*
    想要的建筑物在世界中的大小
    public Vector3 WideBuildingSize = new Vector3(1.0f, .5f, .5f);

    public GameObject SaloonBuildingPrefab;
    */


    private  readonly List<BoxDrawer.Box> _lineBoxList = new List<BoxDrawer.Box>();

    private readonly Queue<PlacementResult> _results = new Queue<PlacementResult>();

    private bool _timeToHideMesh;
    private BoxDrawer _boxDrawing;

	
    // Use this for initialization
	void Start ()
    {
		if(DrawDebugBoxes)
        {
            _boxDrawing = new BoxDrawer(gameObject);
        }
	}
	

	// Update is called once per frame
	void Update ()
    {
        //每一帧更新一个放置结果，保持帧率
        ProcessPlacementResults();


        //放置物体完毕后 1.隐藏网格 2.设置遮挡材质Occlusion
        if(_timeToHideMesh)
        {
            SpatialUnderstandingState.Instance.HideText = true;
            HideGridEnableOcclusion();
            _timeToHideMesh = false;
        }

        if(DrawDebugBoxes)
        {
            _boxDrawing.UpdateBoxes(_lineBoxList);
        }
		
	}


    //未放置成功时，网格为默认材质
    //在放置成功后，网格材质设置为Occlusion遮挡材质，即可以网格（真实物体）遮挡虚拟物体
    public Material OccludedMaterial; 
  
    private void HideGridEnableOcclusion()
    {
        SpatialUnderstandingMesh.MeshMaterial = OccludedMaterial;
    }



    public void CreateScene()
    {
        if (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
        { return; }

        //在扫描阶段完成、场景敲定后，初始化物体放置解析器。
        SpatialUnderstandingDllObjectPlacement.Solver_Init();

        SpatialUnderstandingState.Instance.SpaceQueryDescription = "Generating World";

       
        List<PlacementQuery> queries = new List<PlacementQuery>();

        if(DrawBuildings)
        {
            //AddRange()方法，可加入多个元素。可以枚举类型或表类型
            queries.AddRange(AddBuildings());
        }

        if(DrawTrees)
        {
            queries.AddRange(AddTrees());
        }


        GetLocationsFromSolver(queries);

    }



    public List<PlacementQuery> AddBuildings()
    {
        var queries = CreateLocationQueriesForSolver(ObjectCollectionManager.Instance.WideBuildingPrefabs.Count, ObjectCollectionManager.Instance.WideBuildingSize, ObjectType.WideBuilding);
        queries.AddRange(CreateLocationQueriesForSolver(ObjectCollectionManager.Instance.SquareBuildingPrefabs.Count, ObjectCollectionManager.Instance.SquareBuildingSize, ObjectType.SquareBuliding));
        queries.AddRange(CreateLocationQueriesForSolver(ObjectCollectionManager.Instance.TallBuildingPrefabs.Count, ObjectCollectionManager.Instance.TallBuildingSize, ObjectType.TallBuliding));

        return queries;
    }



    private List<PlacementQuery> AddTrees()
    {
        var queries = CreateLocationQueriesForSolver(ObjectCollectionManager.Instance.TreePrefabs.Count, ObjectCollectionManager.Instance.TreeSize, ObjectType.Tree);

        return queries;
    }




    private int _placedWideBuilding;
    private int _placedSquareBuildings;
    private int _placedTallBuildings;
    private int _placedTrees;


    //1. 该方法在结果队列不为空的情况下，仅出队一个放置结果（toPlace）并画出Box
    //2. 调用创建物体方法，由
    private void ProcessPlacementResults()
    {
        if(_results.Count>0)
        {
            var toPlace = _results.Dequeue();
            
            //输出
            if(DrawDebugBoxes)
            {
                DrawBox(toPlace, Color.red);
            }

            //每个三维物体都有自己的局部坐标系，旋转物体时，局部坐标系也会跟着旋转；
            //故描述某个三维对象的局部坐标轴的朝向，就可以表示该对象的旋转程度。
            var rotation = Quaternion.LookRotation(toPlace.Normal, Vector3.up);

            switch(toPlace.ObjType)
            {
                case ObjectType.WideBuilding:
                    ObjectCollectionManager.Instance.CreateWideBuilding(_placedWideBuilding++, toPlace.Position, rotation);
                    break;
                case ObjectType.SquareBuliding:
                    ObjectCollectionManager.Instance.CreateSquareBuilding(_placedSquareBuildings++, toPlace.Position, rotation);
                    break;
                case ObjectType.TallBuliding:
                    ObjectCollectionManager.Instance.CreateTallBuilding(_placedTallBuildings++, toPlace.Position, rotation);
                    break;
                case ObjectType.Tree:
                    ObjectCollectionManager.Instance.CreateTree(_placedTrees++, toPlace.Position, rotation);
                    break;

            }
        }
    }



    /*添加了ObjectCollectionManager脚本后可删掉的Code；

    // 1.给出合适的放置位置和朝向
    // 2.实例化放置物体
    // 3.设置欲放置物体的父组件和合适的缩放
    public void CreateWideBuilding(Vector3 positionCenter, Quaternion rotation)
    {
        //Spatial Understanding给出的位置是满足条件三维区域的准确的中心位置，我们想要放置的物体仅在X，Z平面上
        //保持区域的中心的位置，但向下（地面）偏移一点
        var position = positionCenter - new Vector3(0, WideBuildingSize.y * .5f, 0);

        GameObject newObject = Instantiate(SaloonBuildingPrefab, position, rotation) as GameObject;

        if(newObject!=null)
        {
            //gameobject是指该脚本所挂载的物体，即placement
            newObject.transform.parent = gameObject.transform;

            newObject.transform.localScale = RescaleToDesiredSizeProportional(SaloonBuildingPrefab, WideBuildingSize);
        }
    }


    //给要放置的物体乘上一个缩放参数
    private Vector3 RescaleToDesiredSizeProportional(GameObject objectToScale, Vector3 desiredSize)
    {
        //new list<>后加中括号，表示有实参项？？
        float scaleFactor = CalScaleFactorHelper(new List<GameObject>{ objectToScale }, desiredSize);

        return objectToScale.transform.localScale * scaleFactor;
    }



    private float CalScaleFactorHelper(List<GameObject> objects, Vector3 desiredSize)
    {
        float maxScale = float.MaxValue;

        foreach(var obj in objects)
        {
            var curBounds = GetBoundsForAllChildren(obj).size;
            var difference = curBounds - desiredSize;

            float ratio;

            //if...else if...else ,而非if重复，否则值会依赖于if的先后顺序，会改变
            if (difference.x > difference.y && difference.x > difference.z)
            { ratio = desiredSize.x / curBounds.x; }
            else if (difference.y > difference.x && difference.y > difference.z)
            { ratio = desiredSize.y / curBounds.y; }
            else
            { ratio = difference.z / curBounds.z;  }

            //求最大缩放因子，以满足想要的尺寸，实为“缩”；
            if (ratio < maxScale)
            { maxScale = ratio; }

        }

        return maxScale;
    }



    //检测欲放置物体的所有子组件，创建一个可囊括其所有子组件的包围盒
    private Bounds GetBoundsForAllChildren(GameObject findMyBounds)
    {
        //result是最终要使用的边界框
        Bounds result = new Bounds(Vector3.zero, Vector3.zero);

        foreach(var renderer in findMyBounds.GetComponentsInChildren<Renderer>())
        {
            if(result.extents==Vector3.zero)
            {
                //1. 如果最终边界框现阶段还是0大侠，则用该组件的渲染器边界框代替
                result = renderer.bounds;
            }

            else
            {
                //2. 如果现阶段result不为0，则扩展最终边界框result以包含代表渲染器边界框的两个点
                result.Encapsulate(renderer.bounds);
            }

        }

        return result;
    }

    */



    private void DrawBox(PlacementResult boxLocation, Color color)
    {
        if(boxLocation!=null)
        {
            _lineBoxList.Add(
                new BoxDrawer.Box(
                    boxLocation.Position,
                    Quaternion.LookRotation(boxLocation.Normal, Vector3.up),
                    color,
                    boxLocation.Dimensions * 0.5f)
                    );
        }
    }




    private void GetLocationsFromSolver(List<PlacementQuery> placementQueries)
    {
        
        //平台判断：是否在 1.unity编辑模式下（即点击播放后）2.是否在WSA（windows store apps）模式下
        //开启一个新的独立的线程，调用PlaceObject,将返回的非空结果加到放置结果队列中
        //System.Threading.Tasks中的静态Run方法,使用默认值，无需额外参数，来启动任务。通常以异步方式执行

#if UNITY_WSA && !UNITY_EDITOR
        
        System.Threading.Tasks.Task.Run(() =>
         {
             for (int i = 0; i < placementQueries.Count; ++i)
             {
                 var result = PlaceObject(placementQueries[i].ObjType.ToString() + i,
                                          placementQueries[i].PlacementDefinition,
                                          placementQueries[i].Dimensions,
                                          placementQueries[i].ObjType,
                                          placementQueries[i].PlacementRules,
                                          placementQueries[i].PlacementConstraints);
                 if (result != null)
                 {
                     _results.Enqueue(result);
                 }
             }

             //若放置物体已经进行完毕了，则隐藏网格，只显示物体，并设置遮挡材质
             _timeToHideMesh = true;
          });
#else
        _timeToHideMesh = true;
#endif
    }



    private PlacementResult PlaceObject(string placementName,
        SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition placementDefinition,
        Vector3 boxFullDims,
        ObjectType objType,
        List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> placementRules=null,
        List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> placementConstraints=null)

    {
        //Solver_PlaceObject()返回0代表失败，返回1代表成功
        //故判断是否大于0，成功则获取放置结果
        //PinObject直接返回指定的物体在内存中的位置
        if (SpatialUnderstandingDllObjectPlacement.Solver_PlaceObject(
            placementName,
            SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(placementDefinition),
            (placementRules != null) ? placementRules.Count : 0,
            ((placementRules != null) && (placementRules.Count > 0)) ? SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(placementRules.ToArray()) : IntPtr.Zero,
            (placementConstraints != null) ? placementConstraints.Count : 0,
            (placementConstraints!=null)&&(placementConstraints.Count>0)?SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(placementConstraints.ToArray()):IntPtr.Zero,
            SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticObjectPlacementResultPtr())
            >0)
            {
            SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult placementResult = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticObjectPlacementResult();

            return new PlacementResult(placementResult.Clone() as SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult, boxFullDims, objType);

             }
        return null;

    }


    //返回存放放置定义，规则，限制的表
    private List<PlacementQuery> CreateLocationQueriesForSolver(int desiredLocationCount, Vector3 boxFullDims, ObjectType objType)
    {
        List<PlacementQuery> placementQueries = new List<PlacementQuery>();

        //Bounds的size是extent的2倍
        var halfBoxDims = boxFullDims * .5f;

        var distanceFromOtherObjects = halfBoxDims.x > halfBoxDims.z ? halfBoxDims.x * 3f : halfBoxDims.z * 3f;

        for (int i = 0; i < desiredLocationCount; ++i)
        {
            var placementRules = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>
            {
                //放置Rule：要远离其他物体一定距离
                SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(distanceFromOtherObjects)
             };

         //放置Constraint,即每个形状组件之间的连接关系：无
        var placementConstraints = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint>();

        //放置定义：放置在地面上，即放在XZ面上
        SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition placementDefinition = SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnFloor(halfBoxDims);

        placementQueries.Add(
            new PlacementQuery(placementDefinition,
                               boxFullDims,
                               objType,
                               placementRules,
                               placementConstraints
                               ));
        }

        return placementQueries;

        }




    }




