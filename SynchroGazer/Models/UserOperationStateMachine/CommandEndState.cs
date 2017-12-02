namespace Baku.SynchroGazer
{
    public class CommandEndState : IState<SynchroGazerStatus>
    {
        public void Enter(SynchroGazerStatus source)
        {
        }

        public void Quit(SynchroGazerStatus source)
        {
        }

        public IState<SynchroGazerStatus> Update(SynchroGazerStatus source)
        {
            if (source.IsUserPresent &&
                source.LeftEyeOpened &&
                source.RightEyeOpened)
            {
                return new DefaultState();
            }

            return this;
        }
    }
}
