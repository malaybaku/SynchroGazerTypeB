namespace Baku.SynchroGazer
{
    public class LeftWinkState : WinkStateBase
    {
        public override BlinkActionTypes EnterActionType => BlinkActionTypes.LeftEyeClosed;
        public override BlinkActionTypes BlinkActionType => BlinkActionTypes.LeftEyeClosedBlink;

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
            return source.RightEyeOpened;
        }

        public override bool ShouldGoToDefaultState(SynchroGazerStatus source)
        {
            return source.LeftEyeOpened;
        }
    }
}
