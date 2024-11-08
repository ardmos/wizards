public abstract class AIState
{
    protected WizardRukeAIServer ai;

    public AIState(WizardRukeAIServer ai)
    {
        this.ai = ai;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}




