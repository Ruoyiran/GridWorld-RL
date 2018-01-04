using GridWorld.Network;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class GridAgent : Agent
{
    private int imageWidth = 84;
    private int imageHeight = 84;
    public Camera renderCamera;
    private Communicator _communicator;
    private string _ipAddress = "127.0.0.1";
    private int _port = 8008;
    private Dictionary<string, Command> _commands;
    private void Start() {
        _commands = new Dictionary<string, Command>();
        _communicator = new Communicator();
        //_communicator.ConnectToServer(_ipAddress, _port);
    }
	
	private void LateUpdate()
    {
        if (_communicator == null ||
            !_communicator.IsConnected)
            return;
        Command cmd = GetCommand();
        if(cmd != null)
            cmd.Action();
    }

    private Command GetCommand()
    {
        string data = _communicator.ReceiveFromServer();
        if(_commands.ContainsKey(data))
            return _commands[data];
        Command cmd = Command.GetCommand(data, this);
        if (cmd != null)
            _commands.Add(data, cmd);
        return cmd;
    }

    public override void Reset()
    {
        Logger.Print("Reset");
        _communicator.SendToServer("Reset");
    }

    public override void Step()
    {
        Logger.Print("Step");
        _communicator.SendToServer("Received");
        string jsonData = _communicator.ReceiveFromServer();
        AgentMessage message = JsonConvert.DeserializeObject<AgentMessage>(jsonData);
        Step((AgentAction)message.Action);

        //Texture2D tex = ImageTool.RenderToTex(renderCamera, imageWidth, imageHeight);
        //byte[] imageBytes = tex.EncodeToPNG();
        //byte[] bytes = ImageTool.AppendLength(imageBytes);
        //Object.DestroyImmediate(tex);
        //Resources.UnloadUnusedAssets();
        //_communicator.SendToServer(bytes);
    }

    public override void Quit()
    {
        Logger.Print("Quit");
        _communicator.Disconnect();
        Application.Quit();
    }

    private void OnApplicationQuit()
    {
        if(_communicator != null)
            _communicator.Disconnect();
    }

    private void Step(AgentAction action)
    {
        switch (action)
        {
            case AgentAction.Up:
                break;
            case AgentAction.Down:
                break;
            case AgentAction.Left:
                break;
            case AgentAction.Right:
                break;
            default:
                break;
        }
    }
}
