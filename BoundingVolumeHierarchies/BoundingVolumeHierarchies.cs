using System;
using System.Collections;
using System.Collections.Generic;

namespace BoundingVolumeHierarchies
{
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x = 0, float y = 0, float z = 0)
        {
            this.x = x; this.y = y; this.z = z;
        }

        public float Magnitude
        {
            get { return (float)Math.Sqrt(x * x + y * y + z * z); }
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return a + (-b);
        }

        public static Vector3 operator -(Vector3 a)
        {
            return new Vector3(-a.x, -a.y, -a.z);
        }

        public static Vector3 Min(Vector3 a, Vector3 b)
        {
            return new Vector3(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
        }

        public static Vector3 Max(Vector3 a, Vector3 b)
        {
            return new Vector3(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
        }

        public override string ToString()
        {
            return "( " + x + ", " + y + ", " + z + " )";
        }
    }

    public struct BoundingVolume
    {
        public Vector3 Min;
        public Vector3 Max;
        public float SurfaceArea;
        public float Volume;

        public BoundingVolume(Vector3 min, Vector3 max)
        {
            this.Min = min;
            this.Max = max;
            Vector3 diff = Max - Min;
            Volume = diff.x * diff.y * diff.z;
            SurfaceArea = 2f * (diff.x * diff.y + diff.y * diff.z + diff.z * diff.x);
        }

        public void Set(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
            Vector3 diff = Max - Min;
            Volume = diff.x * diff.y * diff.z;
            SurfaceArea = 2f * (diff.x * diff.y + diff.y * diff.z + diff.z * diff.x);
        }

        public static BoundingVolume operator +(BoundingVolume a, BoundingVolume b)
        {
            var mmin = Vector3.Min(a.Min, b.Min);
            var mmax = Vector3.Max(a.Max, b.Max);
            var rst = new BoundingVolume(mmin, mmax);
            return rst;
        }

        public bool Intersect(BoundingVolume another)
        {
            return !(this.Min.x > another.Max.x ||
                this.Max.x < another.Min.x ||
                this.Min.y > another.Max.y ||
                this.Max.y < another.Min.y ||
                this.Min.z > another.Max.z ||
                this.Max.z < another.Min.z);
        }

        public override string ToString()
        {
            return "Min: " + Min.ToString() + "\nMax: " + Max.ToString();
        }
    }

    public class Node<T>
    {
        public T Value;
        public Node<T> Parent;
        public Node<T> LeftChild;
        public Node<T> RightChild;

        public Node(T v)
        {
            Value = v;
        }

        public Node(Node<T> n)
        {
            Value = n.Value;
            Parent = n.Parent;
            LeftChild = n.LeftChild;
            RightChild = n.RightChild;
        }

        public void SetHierarchies(Node<T> P, Node<T> L, Node<T> R)
        {
            Parent = P;
            LeftChild = L;
            RightChild = R;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class BoundingVolumeNode : Node<BoundingVolume>
    {
        public BoundingVolumeNode(BoundingVolume v) : base(v)
        {
        }

        public bool Intersect(BoundingVolumeNode another)
        {
            return this.Value.Intersect(another.Value);
        }
    }

    public class BoundingVolumeHierarchies
    {
        public BoundingVolumeNode root;
        private HashSet<BoundingVolumeNode> dictionary;

        public BoundingVolumeHierarchies()
        {
            dictionary = new HashSet<BoundingVolumeNode>(32);
        }

        public void Insert(BoundingVolumeNode node)
        {
            if (dictionary.Contains(node))
                return;

            dictionary.Add(node);

            if (root == null)
                root = node;
            else
            {
                float score = -1;
                BoundingVolumeNode candidate = null;
                InsertHierarchy(root, node, score, 0, ref candidate);

                if (candidate != null)
                {
                    var newNode = new BoundingVolumeNode(node.Value + candidate.Value);
                    if (candidate.Parent == null) //现节点是根节点
                    {
                        root = newNode;
                    }
                    else
                    {
                        if (candidate == candidate.Parent.LeftChild)
                            candidate.Parent.LeftChild = newNode;
                        else
                            candidate.Parent.RightChild = newNode;
                    }
                    newNode.Parent = candidate.Parent;
                    newNode.LeftChild = candidate;
                    newNode.RightChild = node;
                    candidate.Parent = newNode;
                    node.Parent = newNode;

                    var cur = newNode;
                    while (cur.Parent != null)
                    {
                        cur.Parent.Value += cur.Value;
                        cur = (BoundingVolumeNode)cur.Parent;
                    }
                }
            }
        }

        private void InsertHierarchy(BoundingVolumeNode current, BoundingVolumeNode target, float bestcost, float inheritedcost, ref BoundingVolumeNode candidate)
        {
            bool isLeaf = current.LeftChild == null; // 必定是二叉树判断一边就行
            var newVol = current.Value + target.Value;
            float curCost = inheritedcost + newVol.Volume;
            inheritedcost += newVol.Volume - current.Value.Volume;

            if (curCost <= bestcost || bestcost < 0)
            {
                bestcost = curCost;
                candidate = current;

                if (!isLeaf)
                {
                    InsertHierarchy((BoundingVolumeNode)current.LeftChild, target, bestcost, inheritedcost, ref candidate);
                    InsertHierarchy((BoundingVolumeNode)current.RightChild, target, bestcost, inheritedcost, ref candidate);
                }
            }
        }

        public void Remove(BoundingVolumeNode node)
        {
            if (!dictionary.Contains(node)) return;
            dictionary.Remove(node);

            if (node == root)
                root = null;
            else
            {
                bool onLeft = node.Parent.LeftChild == node;
                BoundingVolumeNode brother;
                if (onLeft)
                    brother = (BoundingVolumeNode)node.Parent.RightChild;
                else
                    brother = (BoundingVolumeNode)node.Parent.LeftChild;

                if(node.Parent == root)
                {
                    root = brother;
                    brother.Parent = null;
                }
                else
                {
                    bool onRootLeft = brother.Parent.Parent.LeftChild == brother.Parent;
                    if (onRootLeft)
                        brother.Parent.Parent.LeftChild = brother;
                    else
                        brother.Parent.Parent.RightChild = brother;
                    brother.Parent = brother.Parent.Parent;
                }

                var cur = brother;
                while (cur.Parent != null)
                {
                    cur.Parent.Value = cur.Parent.LeftChild.Value + cur.Parent.RightChild.Value;
                    cur = (BoundingVolumeNode)cur.Parent;
                }
            }
        }

        public BoundingVolumeNode[] Intersect(BoundingVolume volume)
        {
            var list = new List<BoundingVolumeNode>();
            if (root != null)
            {
                var queue = new Queue<BoundingVolumeNode>();
                queue.Enqueue(root);
                while (queue.Count > 0)
                {
                    var node = queue.Dequeue();
                    bool isLeaf = node.LeftChild == null;
                    bool intersect = node.Value.Intersect(volume);
                    if (intersect && isLeaf)
                    {
                        list.Add(node);
                    }
                    else if (intersect)
                    {
                        queue.Enqueue((BoundingVolumeNode)node.LeftChild);
                        queue.Enqueue((BoundingVolumeNode)node.RightChild);
                    }
                }
            }
            return list.ToArray();
        }

        public void Print()
        {
            if (root == null)
            {
                Console.WriteLine("Empty Tree/n"); return;
            }
            Queue<BoundingVolumeNode> q = new Queue<BoundingVolumeNode>();
            q.Enqueue(root);
            while (q.Count > 0)
            {
                var vol = q.Dequeue();
                if (vol == null) continue;
                string msg = "[ ";
                if (vol.Parent != null)
                {
                    msg += vol.Parent.GetHashCode() + " - ";
                }
                msg += vol.GetHashCode() + " ] IsLeaf: " + (vol.LeftChild == null) + "\n" + vol.ToString();
                q.Enqueue((BoundingVolumeNode)vol.LeftChild);
                q.Enqueue((BoundingVolumeNode)vol.RightChild);
                Console.WriteLine(msg);
            }
        }
    }
}