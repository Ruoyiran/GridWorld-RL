
public abstract class Command
{
    protected const string QUIT = "QUIT";
    protected const string RESET = "RESET";
    protected const string STEP = "STEP";
    protected string _name = "Command";
    protected Observer _observer;
    public string Name
    {
        get
        {
            return _name;
        }
    }

    public Command(Observer observer)
    {
        _observer = observer;
    }

    public static Command GetCommand(string cmd, Observer observer)
    {
        switch (cmd)    
        {
            case QUIT:
                return new CommandQuit(observer);
            case RESET:
                return new CommandReset(observer);
            case STEP:
                return new CommandStep(observer);
            default:
                break;
        }
        return null;
    }

    public abstract void Action();
}

public class CommandQuit : Command
{
    public CommandQuit(Observer observer) : base(observer)
    {
        _name = QUIT;
    }

    public override void Action()
    {
        if(_observer != null)
            _observer.Quit();
    }
}

public class CommandReset : Command
{
    public CommandReset(Observer observer) : base(observer)
    {
        _name = RESET;
    }

    public override void Action()
    {
        if (_observer != null)
            _observer.Reset();
    }
}

public class CommandStep : Command
{
    public CommandStep(Observer observer) : base(observer)
    {
        _name = STEP;
    }

    public override void Action()
    {
        if (_observer != null)
            _observer.Step();
    }
}
