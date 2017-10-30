using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlagiarismAlgorithmService
{
    class BoundaryDetection
    {
        public static List<List<IndexedBoundary>> DetectInitialSet(List<int[]> M, int thetaG)
        {
            int[] M1 = new int[M.Count];
            int[] M2 = new int[M.Count];
            for (int i = 0; i < M.Count; i++)
            {
                M1[i] = M[i][0];
                M2[i] = M[i][1];
            }



            List<IndexedBoundary> initialBoundaries = new List<IndexedBoundary>();
            IndexedBoundary boundary = new IndexedBoundary() { lower = M1[0], lowerIndex = 0 };
            for (int i = 1; i < M1.Length; i++)
            {
                if (Math.Abs(M1[i] - M1[i - 1]) > thetaG)
                {
                    boundary.upper = M1[i - 1];
                    boundary.upperIndex = i - 1;
                    initialBoundaries.Add(boundary);

                    boundary = new IndexedBoundary() { lower = M1[i], lowerIndex = i };
                }
            }
            boundary.upper = M1[M1.Length - 1];
            boundary.upperIndex = M1.Length - 1;
            initialBoundaries.Add(boundary);


            List<IndexedBoundary> finalBoundaries = new List<IndexedBoundary>();
            foreach (IndexedBoundary initialBoundary in initialBoundaries)
            {
                boundary = new IndexedBoundary() { lower = M2[initialBoundary.lowerIndex], lowerIndex = initialBoundary.lowerIndex };
                for (int i = initialBoundary.lowerIndex + 1; i < initialBoundary.upperIndex; i++)
                {
                    if (Math.Abs(M2[i] - M2[i - 1]) > thetaG)
                    {
                        boundary.upper = M2[i - 1];
                        boundary.upperIndex = i - 1;
                        finalBoundaries.Add(boundary);

                        boundary = new IndexedBoundary() { lower = M2[i], lowerIndex = i };
                    }
                }
                boundary.upper = M2[initialBoundary.upperIndex];
                boundary.upperIndex = initialBoundary.upperIndex;
                finalBoundaries.Add(boundary);
            }

            List<List<IndexedBoundary>> boundaries = new List<List<IndexedBoundary>>();
            foreach (IndexedBoundary indexedboundary in finalBoundaries)
            {

                IndexedBoundary susp = new IndexedBoundary() { lower = M1[indexedboundary.lowerIndex], upper = M1[indexedboundary.upperIndex] };
                IndexedBoundary original = new IndexedBoundary() { lower = M2[indexedboundary.lowerIndex], upper = M2[indexedboundary.upperIndex] };
                List<IndexedBoundary> mList = new List<IndexedBoundary> { susp, original };
                boundaries.Add(mList);

            }

            return boundaries;
        }

    }
}
