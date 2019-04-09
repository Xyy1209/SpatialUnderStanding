using System;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity.InputModule;
//using UnityEngine.Serialization;


public class SpatialUnderstandingState : Singleton<SpatialUnderstandingState>,IInputClickHandler,ISourceStateHandler
{
    public float MinAreaForStats = 3.0f;
    public float MinAreaForComplete = 30.0f;
    public float MinHorizAreaForComplete = 15.0f;
    public float MinWallAreaForComplete = 10.0f;

    private uint trackedHandsCount = 0;

    public TextMesh DebugDisplay;
    public TextMesh DebugSubDisplay;

    private bool _triggered;
    public bool HideText = false;

    private bool ready = false;



    private string _spaceQueryDescription;

    //场景队列描述字符串
    //没有出现，因为set方法中的_spaceQueryDescription是private变量，无法从Unity编辑器中输入
    public string SpaceQueryDescription
    {
        get
        {
            return _spaceQueryDescription;
        }

        set
        {
            //value表示输入的数据，可以认为是一个准关键字
            _spaceQueryDescription = value;

        }
    }


    //主要过程：1.允许SpatialUnderstanding；2.开启扫描状态；3.获取场景数据指针，DLL对其进行队列化；4.指定DLL的场景数据（SpatialUnderstanding->DLL）
    public bool DoesScanMeetMinBarForCompletion
    {
        get
        {
            //扫描没有达到完成条件的情况：1.不处于扫描状态或不允许场景理解；2.队列化场景数据失败；
            //扫描达到完成条件的情况：不处于以上两种状态，且达到最小面积阈值
            if ((SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Scanning) || (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
            { return false; }

            IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
            if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
            { return false; }
            SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

            if ((stats.TotalSurfaceArea > MinAreaForComplete) && (stats.HorizSurfaceArea > MinHorizAreaForComplete) && (stats.WallSurfaceArea > MinWallAreaForComplete))
            { return true; }

            return false;

        }
    }


    //get方法想要获取它，所以会有返回值
    public string PrimaryText
    {
        get
        {
            if (HideText)
                return string.Empty;

            //显示空间和物体队列的结果（有优先级）
            if (!string.IsNullOrEmpty(SpaceQueryDescription))
            {
                return SpaceQueryDescription;
            }

            //依据扫描过程中的不同大状态，显示不同文字
            if(SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
            {
                switch(SpatialUnderstanding.Instance.ScanState)
                {
                    //若处于扫描中,获取该状态
                    case SpatialUnderstanding.ScanStates.Scanning:

                        //C#调用C++写的DLL时用到IntPtr,它封装了一个指针
                        IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                        if(SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr)==0)
                        {
                            return "Playspace Stats Query Failed!";
                        }

                        return "Walk Around And Scan in Your Playspace";


                    case SpatialUnderstanding.ScanStates.Finishing:
                        return "Finalizing Scan (please wait)!";


                    case SpatialUnderstanding.ScanStates.Done:
                        return "Scan Complete!";


                    default:
                        return "ScanState = " + SpatialUnderstanding.Instance.ScanState;
                }
            }

            return string.Empty;
        }
    }


    public Color PrimaryColor
    {
        get
        {
            ready = DoesScanMeetMinBarForCompletion;

            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning)
            {
                if (trackedHandsCount > 0)
                { return ready ? Color.green : Color.red; }

                return ready ? Color.yellow : Color.white;

            }

            //Color:rgba(alpha定义透明度):0是transparent,1是opaque
            float alpha = 1.0f;

            return (!string.IsNullOrEmpty(SpaceQueryDescription)) ? 
                (PrimaryText.Contains("processing") ? new Color(1.0f, 0.0f, 0.0f, 1.0f) :new Color(1.0f, 0.0f, 0.1f, alpha)) : 
                 new Color(1.0f, 1.0f, 1.0f, alpha);

        }
    }


    public string DetailsText
    {
        get
        {
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.None)
                return "";

            //如果是正处于Scanning状态的某些特定时刻，再提示更加详细的信息
            if((SpatialUnderstanding.Instance.ScanState==SpatialUnderstanding.ScanStates.Scanning)&&(SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
            {
                IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();

                //队列化空间场景失败时，QueryPlayspaceStats返回0
                if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr)==0)
                {
                    return "Playspace stats query failed!";
                }

                //若队列化场景成功(返回值不为0)，则将场景数据传给DLL脚本进行下一步处理
                SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

                if(stats.TotalSurfaceArea>MinAreaForStats)
                {
                    SpatialMappingManager.Instance.DrawVisualMeshes = false;
                    string subDisplayText = string.Format("totalArea={0:0.0},horiz={1:0.0},wall={2:0.0}", stats.TotalSurfaceArea, stats.HorizSurfaceArea, stats.WallSurfaceArea);
                    subDisplayText += string.Format("\nnumFloorCells={0},numCeilingCells{1},numPlatfomCells={2}", stats.NumFloor, stats.NumCeiling, stats.NumPlatform);
                    subDisplayText += string.Format("\npaintMode={0},seenCells={1},notSeen={2}", stats.CellCount_IsPaintMode, stats.CellCount_IsSeenQualtiy_Seen, stats.CellCount_IsSeenQualtiy_None);

                    return subDisplayText;
                }
                return "";
            }

            return "";
        }
    }


    private void Update_DebugDisplay()
    {
        if(DebugDisplay==null)
        {
            return;
        }

        DebugDisplay.text = PrimaryText;
        DebugDisplay.color = PrimaryColor;
        DebugSubDisplay.text = DetailsText;
    }

    

    private void Start()
    {
        InputManager.Instance.PushFallbackInputHandler(gameObject);
    }



    //创建ObjectPlacer的实例，以调用其类中的CreateScene方法
    public ObjectPlacer Placer;

    // Update检测是否ScanState处于Done,若处于则设置_triggered为true
    private void Update ()
    {
        Update_DebugDisplay();

        if (!_triggered && SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
        {
            _triggered = true;
            Placer.CreateScene();

        }
	}


    //请求FinishScan的条件：1.以满足结束的最小面积限制；2.处于扫描状态且扫描重复数据；3.做air tap
    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (ready && (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) && SpatialUnderstanding.Instance.ScanStatsReportStillWorking)
            SpatialUnderstanding.Instance.RequestFinishScan();
    }


    //ISourceStateHandler接口可检测输入源状态的改变：追踪到或丢失
    public void OnSourceDetected(SourceStateEventData eventData)
    {
        trackedHandsCount++;
    }


    public void OnSourceLost(SourceStateEventData eventData)
    {
        trackedHandsCount--;
    }
  

}

