
using System.Collections.Generic;

public class Algorithm {

    public static List<T> RandomSample<T>(List<T> dataList, int count)
    {
        if (dataList == null || count <= 0)
            return new List<T>();
        if (count >= dataList.Count)
            return new List<T>(dataList);
        List<T> samples = new List<T>();
        List<int> orderedSequence = new List<int>();
        for (int i = 0; i < dataList.Count; i++)
        {
            orderedSequence.Add(i);
        }
        System.Random random = new System.Random();
        while (samples.Count < count)
        {
            int randomIndex = random.Next() % orderedSequence.Count;
            int dataIndex = orderedSequence[randomIndex];
            samples.Add(dataList[dataIndex]);
            orderedSequence.RemoveAt(randomIndex);
        }
        return samples;
    }
}
