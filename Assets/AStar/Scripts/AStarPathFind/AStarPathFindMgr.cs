using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Tilemaps;

public class AStarPoint
{
    public float Cost_to_come;
    public float Cost_to_target;
    public Vector3Int Pos;
    public AStarPoint FormerPoint;

    public AStarPoint(float ctc,float ctt,Vector3Int pos,AStarPoint former)
    { 
        Cost_to_come = ctc;
        Cost_to_target = ctt;
        Pos = pos;
        FormerPoint = former;
    }
}

public class AStarPathFindMgr : MonoBehaviour
{
    public Tile road;
    public Tile ground;
    public Tile obstacle;
    public Tile search;
    public Tile opened;

    public Tilemap Ground;
    public Tilemap Obstacle;

   

    public List<GameObject> StartPoint;
    public GameObject TargetPoint;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var StartPoint in StartPoint)
        { StartCoroutine(GetPath(StartPoint.transform.position, TargetPoint.transform.position)); }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator GetPath(Vector3 Start, Vector3 Target)
    {
        float StartTime = Time.time;
        List<Node> OpenList = new();
        List<Node> CloseList = new();
        List<Node> AroundList = new();
        List<Node> SearchList = new();

        Vector3Int StartPos_Int = Ground.WorldToCell(Start);
        Vector3Int TargetPos_Int = Ground.WorldToCell(Target);

        Node StartNode = new(0, 0, StartPos_Int, null);
        Node TargetNode = new(0, 0, TargetPos_Int, null);

        OpenList.Add(StartNode);
        SearchList.Add(StartNode);

        while (CloseList.Find(x => x.Pos == TargetPos_Int) == null)
        {
            Debug.Log("Path");
            GetNodeToSearch(AroundList, SearchList, TargetPos_Int,OpenList,CloseList);
            foreach(var Node in SearchList)
            {
                if(CloseList.Find(x => x.Pos == Node.Pos) == null) CloseList.Add(Node);
                OpenList.Remove(Node);
            }
            SearchList.Clear();
            #region “图形绘制”
            foreach (var Node in CloseList)
            {
                Draw(obstacle, Node.Pos);
            }

            yield return new WaitForSeconds(0.1f);

            foreach (var Node in AroundList)
            {
                Draw(search, Node.Pos);
            }
            yield return new WaitForSeconds(0.1f);
            #endregion

            foreach (var Node in AroundList)
            {
                if (OpenList.Find(x => x.Pos == Node.Pos) == null) OpenList.Add(Node);
            }

            foreach (var Node in OpenList)
            {
                Draw(opened, Node.Pos);
            }

            Node CloestNode = OpenList[OpenList.Count-1];
            float CloestDistance = CloestNode.Total_cost;
            foreach (var Node in OpenList)
            {
                if(Node.Total_cost < CloestDistance && Node.Pos != StartPos_Int)
                {
                    CloestDistance = Node.Total_cost;
                    CloestNode = Node;
                }
            }

            foreach(var Node in OpenList)
            {
                if (Node.Total_cost == CloestDistance)
                {
                    SearchList.Add(Node);
                }
            }

            AroundList.Clear();
        }

        #region 获取路径
        CloseList.Reverse();
        Debug.Log(CloseList.Count);
        Node LastNode = CloseList.Find(x => x.Pos == TargetPos_Int);
        List<Node> Copy = new(CloseList);
        foreach (var Node in Copy)
        {
            if(LastNode.From == null) break;
            if (Node != LastNode.From)
            {
                CloseList.Remove(Node);
            }
            else
            { 
                LastNode = Node;
            }
        }
        CloseList.Reverse();
        CloseList.Add(TargetNode);
        #endregion

        Debug.Log("寻路用时:" + (Time.time - StartTime));

        foreach (var Node in CloseList)
        {
            Draw(road, Node.Pos);

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.1f);
    }

    void GetNodeToSearch(List<Node> SearchList, List<Node> Middle, Vector3Int TargetPos,List<Node> OpenList,List<Node> CloseList)
    {
        foreach (var Node in Middle)
        {
            Vector3Int[] Around_1 = new Vector3Int[4]
            {
                new Vector3Int(Node.Pos.x + 1, Node.Pos.y, 0),
                new Vector3Int(Node.Pos.x - 1, Node.Pos.y, 0),
                new Vector3Int(Node.Pos.x, Node.Pos.y + 1, 0),
                new Vector3Int(Node.Pos.x, Node.Pos.y - 1, 0),
            };

            Vector3Int[] Around_2 = new Vector3Int[4]
            {
                new Vector3Int(Node.Pos.x + 1, Node.Pos.y + 1, 0),
                new Vector3Int(Node.Pos.x - 1, Node.Pos.y - 1, 0),
                new Vector3Int(Node.Pos.x + 1, Node.Pos.y - 1, 0),
                new Vector3Int(Node.Pos.x - 1, Node.Pos.y + 1, 0),
            };

            foreach (var Vec in Around_1)
            {
                if (Ground.GetTile(Vec) && !Obstacle.GetTile(Vec))
                {
                    if (OpenList.Find(x => x.Pos == Vec) == null && CloseList.Find(x => x.Pos == Vec) == null)
                    {
                        float CTH = Node.Cost_to_here + 1;
                        float CTT = MathF.Abs((Vec - TargetPos).x) + MathF.Abs((Vec - TargetPos).y);
                        Node newNode = new(CTH, CTT, Vec, Node);
                        SearchList.Add(newNode);
                    }
                    else if (OpenList.Find(x => x.Pos == Vec) != null && CloseList.Find(x => x.Pos == Vec) == null)
                    {
                        SearchList.Add(OpenList.Find(x => x.Pos == Vec));
                    }
                }
            }

            foreach (var Vec in Around_2)
            {
                if (Ground.GetTile(Vec) && !Obstacle.GetTile(Vec))
                {
                    if (OpenList.Find(x => x.Pos == Vec) == null && CloseList.Find(x => x.Pos == Vec) == null)
                    {
                        float CTH = Node.Cost_to_here + 1.4f;
                        float CTT = MathF.Abs((Vec - TargetPos).x) + MathF.Abs((Vec - TargetPos).y);
                        Node newNode = new(CTH, CTT, Vec, Node);
                        SearchList.Add(newNode);
                    }
                    else if (OpenList.Find(x => x.Pos == Vec) != null && CloseList.Find(x => x.Pos == Vec) == null)
                    {
                        SearchList.Add(OpenList.Find(x => x.Pos == Vec));
                    }
                }
            }

        }
    }

    void Draw(Tile Tile, Vector3Int TargetPos)
    {
        Ground.SetTile(TargetPos, Tile);
    }
}
