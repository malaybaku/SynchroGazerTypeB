namespace Baku.SynchroGazer
{
    public class RightWinkState : WinkStateBase
    {
        public override BlinkActionTypes EnterActionType => BlinkActionTypes.RightEyeClosed;
        public override BlinkActionTypes BlinkActionType => BlinkActionTypes.RightEyeClosedBlink;

        public override long GetBlinkDuration(SynchroGazerStatus source)
        {
            //とりあえず決め打ち
            return 2500;
        }

        public override int GetCommandBlinkCount(SynchroGazerStatus source)
        {
            //とりあえず決め打ち
            return 3;
        }

        public override bool GetTargetEyeOpened(SynchroGazerStatus source)
        {
            return source.LeftEyeOpened;
        }

        public override bool ShouldGoToDefaultState(SynchroGazerStatus source)
        {
            return source.RightEyeOpened;
        }
    }
}
