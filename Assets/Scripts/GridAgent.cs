using GridWorld.Network;
using System.Collections.Generic;

public class GridAgent : Agent
{
    private Communicator _communicator;
    private string _ipAddress = "127.0.0.1";
    private int _port = 8008;
    private Dictionary<string, Command> _commands;
    private void Start() {
        _commands = new Dictionary<string, Command>();
        _communicator = new Communicator();
        _communicator.ConnectToServer(_ipAddress, _port);
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
        _communicator.SendToServer("Step");
    }

    public override void Exit()
    {
        Logger.Print("Exit");
        _communicator.Disconnect();
    }

    private void OnApplicationQuit()
    {
        if(_communicator != null)
            _communicator.Disconnect();
    }
}
