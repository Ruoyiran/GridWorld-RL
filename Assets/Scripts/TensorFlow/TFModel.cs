using System;
using System.IO;

#if ENABLE_TENSORFLOW
using TensorFlow;

public class TFModel
{
    private TFGraph _tfGraph;
    private TFSession _session;
    private bool _bModelLoaded = false;

    public TFModel(string modelPath)
    {
        _tfGraph = new TFGraph();
        _bModelLoaded = LoadModelFromPath(modelPath);
    }

    public bool LoadModelFromPath(string modelPath)
    {
        if (_bModelLoaded)
            return true;
        Logger.Print("Load TF model from '{0}'", modelPath);
        byte[] bytes = File.ReadAllBytes(modelPath);
        if (bytes == null || bytes.Length == 0)
        {
            Logger.Error("TFModel.LoadModelFromPath - failed to load tf model from '{0}'", modelPath);
            return false;
        }
        try
        {
            _tfGraph.Import(bytes);
            _session = new TFSession(_tfGraph);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex);
            return false;
        }
        return true;
    }

    public float[,] GetValue(string inputNode, string outputNode, byte[] inputData)
    {
        if (!_bModelLoaded)
            return null;
        var runner = _session.GetRunner();
        TFTensor tensor = TFUtils.CreateTensor(inputData, TFDataType.Float);
        runner.AddInput(_tfGraph[inputNode][0], tensor);
        runner.Fetch(_tfGraph[outputNode][0]);
        TFTensor[] output = runner.Run();
        float[,] value = (float[,])output[0].GetValue();
        return value;
    }
}
#endif