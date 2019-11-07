using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoundingVolumeHierarchies;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            Vector3[] list = new Vector3[] { new Vector3(1,1,1), new Vector3(2,2,2),
            new Vector3(-2,-2,-2), new Vector3(-1,-1,-1),
            new Vector3(0,2,-2), new Vector3(1,7,-1),
            new Vector3(2,1,4), new Vector3(3,2,4),
            new Vector3(7,2,-2), new Vector3(11,4,0),
            };

            List<BoundingVolumeNode> listB = new List<BoundingVolumeNode>();
            var tree = new BoundingVolumeHierarchies.BoundingVolumeHierarchies();
            for (int i = 0; i < list.Length; i += 2)
            {
                var min = list[i];
                var max = list[i + 1];
                var vol = new BoundingVolumeNode(new BoundingVolume(min, max));
                listB.Add(vol);
                tree.Insert(vol);
                tree.Print();
                Console.WriteLine("\n");
            }

            Console.WriteLine("----- Remove -----\n");
            Random random = new Random();
            for(int i = 0; i < 4; i++)
            {
                int rnd = random.Next() % (listB.Count);
                var rm = listB[rnd];
                listB.RemoveAt(rnd);
                tree.Remove(rm);
                tree.Print();
                Console.WriteLine("\n");
            }

            BoundingVolumeNode[] rst = tree.Intersect(new BoundingVolume(new Vector3(0, 0, 0), new Vector3(4, 1, 2)));
            foreach(var r in rst)
            {
                Console.WriteLine(r.ToString());
            }
        }
    }
}
