
public abstract class Command{
    protected const string EXIT = "EXIT";
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
            case EXIT:
                return new CommandExit(agent);
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

public class CommandExit : Command
{
    public CommandExit(Agent agent) : base(agent)
    {
        _name = EXIT;
    }

    public override void Action()
    {
        _agent.Exit();
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
