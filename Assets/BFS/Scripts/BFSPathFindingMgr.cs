using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Node
{
    public float Cost_to_here;
    public float Cost_to_goal;
    public float Total_cost
    {
        get => Cost_to_goal + Cost_to_here;
    }
    public Vector3Int Pos;
    public Node From;

    public Node(float CTH,float CTG,Vector3Int Pos,Node From)
    { 
        Cost_to_here = CTH;
        Cost_to_goal = CTG;
        this.Pos = Pos;
        this.From = From;
    }

    public bool ArriveNode(Node Target)
    {
        return Pos == Target.Pos;
    }
}

public class BFSPathFindingMgr : MonoBehaviour
{
    public Tile road;
    public Tile ground;
    public Tile obstacle;
    public Tile search;
    public Tile opened;

    public Tilemap Ground;
    public Tilemap Obstacle;

    List<Node> OpenList = new();
    List<Node> CloseList = new();

    public GameObject StartPoint;
    public GameObject TargetPoint;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetPath(StartPoint.transform.position,TargetPoint.transform.position));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator GetPath(Vector3 Start,Vector3 Target)
    {
        List<Node> AroundList = new();
        List<Node> SearchList = new();

        Vector3Int StartPos_Int = Ground.WorldToCell(Start);
        Vector3Int TargetPos_Int = Ground.WorldToCell(Target);

        Node StartNode = new(0,0,StartPos_Int,null);
        Node TargetNode = new(0,0,TargetPos_Int,null);

        OpenList.Add(StartNode);
        SearchList.Add(StartNode);

        while(OpenList.Find(x => x.Pos == TargetPos_Int) == null)
        {   
            GetNodeToSearch(AroundList,SearchList,TargetPos_Int);
            SearchList.Clear();

            foreach (var Node in OpenList)
            {
                Draw(opened,Node.Pos);
            }

            yield return new WaitForSeconds(0.1f);

            foreach (var Node in AroundList)
            {
                Draw(search,Node.Pos);
            }
            yield return new WaitForSeconds(0.1f);

            foreach (var Node in AroundList)
            {
                SearchList.Add(Node);
            }

            AroundList.Clear();
        }

        OpenList.Reverse();
        Node LastNode = OpenList.Find(x => x.Pos == TargetPos_Int);
        CloseList.Add(LastNode);
        for(int i = 0;i < OpenList.Count - 1;i++ )
        {
            if (OpenList[i] == LastNode.From)
            {
                CloseList.Add(OpenList[i]);
                LastNode = OpenList[i];
            }
        }
        CloseList.Reverse();

        foreach (var Node in CloseList)
        {
            Draw(road,Node.Pos);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.1f);
    }

    void GetNodeToSearch(List<Node>SearchList,List<Node> Middle,Vector3Int TargetPos)
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
                if(Ground.GetTile(Vec) && !Obstacle.GetTile(Vec))
                { 
                    if(OpenList.Find(x => x.Pos == Vec) == null)
                    {
                        float CTH = Node.Cost_to_here + 1;
                        float CTT = MathF.Abs((Vec - TargetPos).x) + MathF.Abs((Vec - TargetPos).y);
                        Node newNode = new(CTH,CTT,Vec,Node);
                        OpenList.Add(newNode);
                        SearchList.Add(newNode);
                    }
                }
            }

            foreach (var Vec in Around_2)
            {
                if (Ground.GetTile(Vec) && !Obstacle.GetTile(Vec))
                {
                    if (OpenList.Find(x => x.Pos == Vec) == null)
                    {
                        float CTH = Node.Cost_to_here + 1.4f;
                        float CTT = MathF.Abs((Vec - TargetPos).x) + MathF.Abs((Vec - TargetPos).y);
                        Node newNode = new(CTH, CTT, Vec, Node);
                        OpenList.Add(newNode);
                        SearchList.Add(newNode);
                    }
                }
            }

        }
    }

    void Draw(Tile Tile,Vector3Int TargetPos)
    {
        Ground.SetTile(TargetPos,Tile);
    }
}
