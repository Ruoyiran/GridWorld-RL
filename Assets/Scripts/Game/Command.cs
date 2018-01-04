
public abstract class Command{
    protected const string QUIT = "QUIT";
    protected const string RESET = "RESET";
    protected const string STEP = "STEP";
    protected string _name = "Command";
    protected Agent _agent;
    public string Name
    {
        get
        {
            return _name;
        }
    }

    public Command(Agent agent)
    {
        _agent = agent;
    }

    public static Command GetCommand(string cmd, Agent agent)
    {
        switch (cmd)    
        {
            case QUIT:
                return new CommandQuit(agent);
            case RESET:
                return new CommandReset(agent);
            case STEP:
                return new CommandStep(agent);
            default:
                break;
        }
        return null;
    }

    public abstract void Action();
}

public class CommandQuit : Command
{
    public CommandQuit(Agent agent) : base(agent)
    {
        _name = QUIT;
    }

    public override void Action()
    {
        _agent.Quit();
    }
}

public class CommandReset : Command
{
    public CommandReset(Agent agent) : base(agent)
    {
        _name = RESET;
    }

    public override void Action()
    {
        if (_agent != null)
            _agent.Reset();
    }
}

public class CommandStep : Command
{
    public CommandStep(Agent agent) : base(agent)
    {
        _name = STEP;
    }

    public override void Action()
    {
        if (_agent != null)
            _agent.Step();
    }
}
