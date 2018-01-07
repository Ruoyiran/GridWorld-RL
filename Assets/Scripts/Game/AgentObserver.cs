using GridWorld.Network;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;

namespace GridWorld
{
    public class AgentObserver : Observer
    {
        private string _ipAddress = "127.0.0.1";
        private int _port = 8009;
        private Communicator _communicator;
        private Dictionary<string, Command> _commands;
        private Environment _env;

        public void SetEnvironment(Environment env)
        {
            _env = env;
        }

        public void CreateCommunicator()
        {
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
            if (cmd != null)
                cmd.Action();
        }

        private Command GetCommand()
        {
            string data = _communicator.ReceiveFromServer();
            if (_commands.ContainsKey(data))
                return _commands[data];
            Command cmd = Command.GetCommand(data, this);
            if (cmd != null)
                _commands.Add(data, cmd);
            return cmd;
        }

        public override void Reset()
        {
            byte[] imageBytes = _env.Reset();
            SendDataBytesToServer(imageBytes);
        }

        public override void Step()
        {
            NotifyServerDataReceived();
            string jsonData = _communicator.ReceiveFromServer();
            AgentMessage message = JsonConvert.DeserializeObject<AgentMessage>(jsonData);
            AgentStepMessage stepMsg = _env.Step((Action)message.Action);
            string stepData = JsonConvert.SerializeObject(stepMsg, Formatting.Indented);
            SendDataBytesToServer(Encoding.ASCII.GetBytes(stepData));
            SendDataBytesToServer(_env.GetEnvironmentImageBytes());
        }

        public override void Quit()
        {
            _communicator.Disconnect();
            Application.Quit();
        }

        private void OnApplicationQuit()
        {
            if (_communicator != null)
                _communicator.Disconnect();
        }

        private void NotifyServerDataReceived()
        {
            _communicator.SendToServer("Received");
        }

        private void SendDataBytesToServer(byte[] data)
        {
            byte[] bytes = Algorithm.AppendLength(data);
            _communicator.SendToServer(bytes);
        }
    }
}