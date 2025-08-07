using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CalcPartition : MonoBehaviour
{
    // Caches for factorials from 0 to 13
    static long[] factorialCache = InitFactorials();

    static long ptf0 = 0;   // partitions thus far
    static long ptf1 = 0;

    public int[] CalcAnswer(long serial, int moduleId)
    {
        int[] finalSubsets = new int[13];
        int localN = 13; // Local copy to prevent static pollution

        long baseIndex = serial % 27644437;
        if (baseIndex == 27644436)
        {
            return new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        }

        ptf0 = 0; ptf1 = 0; // Reset partition tracking state between runs

        // Generate integer partition based on base index
        List<int> intPar = findIntPar(baseIndex, ref localN);           // LOCATES THE INTEGER PARTITION (APPENDIX A

        //string guh = ""; foreach (int fucker in intPar) { guh += fucker + ", "; } Debug.Log(guh);

        decimal indexA = baseIndex - (ptf1 - CalcIPC(intPar, localN));        // CALCULATES IndexA1

        intPar.Sort();

        List<int> elements = Enumerable.Range(0, 13).ToList(); // Reset elements each call
        List<List<int>> setPart = new List<List<int>>();

        while (intPar.Count > 0)                                            //RUNS UNTIL EVERY SUBSET IS FOUND
        {
            int ele = intPar[0];
            int countEle = intPar.Count(x => x == ele); // Avoid multiple enumerations

            int r = (ele == 1 && countEle > 1) ? countEle : ele;            //TREATS MULTIPLE SUBSETS OF r = 1 AS MERGED


            decimal CPSn = CalcIPC(intPar, localN) / (decimal)(Factorial(localN) / (Factorial(r) * Factorial(localN - r))); 
            decimal snv = indexA / CPSn;
            int snvRound = (int)Math.Floor(snv);                

            

            List<int> subsetN = CountSubset(snvRound, elements, r);     // finds answer to Subset N

            foreach (int e in subsetN)                      
                elements.Remove(e);
            localN = elements.Count;                            // updates N

            if (ele == 1 && countEle > 1)                     // Removes all instances of r = 1 subsets, unmerges them
            {
                intPar.RemoveAll(x => x == ele);
                foreach (int led in subsetN)
                    setPart.Add(new List<int> { led });
            }
            else                                            // saves Subset N
            {
                intPar.Remove(ele);
                setPart.Add(subsetN);
            }

            indexA = Decimal.Round((snv - snvRound) * CalcIPC(intPar, localN));               // calculates IndexA2

            //Debug.Log("snv: " + snv + "     CPSn: " + CPSn + "     indexA: " + indexA);
        }

        for (int i = 0; i < setPart.Count; i++)
        {
            foreach (int elem in setPart[i])
            {
                finalSubsets[elem] = i;
            }
        }

        return finalSubsets;
    }

    public static List<int> CountSubset(int index, List<int> elements, int r)
    {
        int n1 = elements.Count;
        List<int> subN = new List<int>();
        int start = 0;

        for (int i = 0; i < r; i++)
        {
            for (int j1 = start; j1 < n1; j1++)
            {
                int count = Binomial(n1 - j1 - 1, r - i - 1);
                if (index < count)
                {
                    subN.Add(elements[j1]);
                    start = j1 + 1;
                    break;
                }
                index -= count;
            }
        }

        return subN;
    }

    public static List<int> findIntPar(long baseIndex, ref int localN)
    {
        List<int> p = new List<int> { localN };
        int k = 0;

        while (true)
        {
            long currentIPC = CalcIPC(p, localN);
            ptf1 += currentIPC;

            if (baseIndex <= ptf1 && baseIndex > ptf0)
                break;

            ptf0 += currentIPC;
            int rem_val = 0;

            while (k >= 0 && p[k] == 1)
            {
                rem_val += p[k];
                p.RemoveAt(k--);
            }

            if (k < 0)
                break;

            p[k]--;
            rem_val++;

            while (rem_val > p[k])
            {
                p.Add(p[k]);
                rem_val -= p[k];
                k++;
            }

            p.Add(rem_val);
            k++;
        }

        return p;
    }

    public static long CalcIPC(List<int> currentIntPar, int localN)
    {
        long ipcDen = 1;
        HashSet<int> seen = new HashSet<int>();

        foreach (int ele in currentIntPar)
        {
            ipcDen *= Factorial(ele);
            if (seen.Add(ele))
            {
                ipcDen *= Factorial(currentIntPar.Count(x => x == ele));
            }
        }

        return Factorial(localN) / ipcDen;
    }

    public static long Factorial(int subject)
    {
        return factorialCache[subject];
    }

    public static long[] InitFactorials()
    {
        long[] cache = new long[14];
        cache[0] = 1;
        for (int i = 1; i <= 13; i++)
        {
            cache[i] = cache[i - 1] * i;
        }
        return cache;
    }

    public static int Binomial(int a, int b)
    {
        if (b < 0 || b > a) return 0;
        if (b == 0 || b == a) return 1;

        int res = 1;
        for (int i = 1; i <= b; i++)
        {
            res *= a--;
            res /= i;
        }
        return res;
    }
}
