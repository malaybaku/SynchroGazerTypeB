using System.Collections.Generic;
using System.Windows;

namespace Baku.SynchroGazer
{
    /// <summary>
    /// 左右のウィンク状態の共通部分を持ってきた基底クラス。差分コーディングが目的で作成。
    /// </summary>
    public abstract class WinkStateBase : IState<SynchroGazerStatus>
    {

        private readonly Queue<BlinkToggleData> _blinks = new Queue<BlinkToggleData>();
        private bool _eyeOpened;

        //NOTE: これらの数値はSetting系のデータから引っ張ってくる可能性が高いので抽象化した方が無難。
        public abstract long GetBlinkDuration(SynchroGazerStatus source);
        public abstract bool GetTargetEyeOpened(SynchroGazerStatus source);
        public abstract int GetCommandBlinkCount(SynchroGazerStatus source);

        public abstract bool ShouldGoToDefaultState(SynchroGazerStatus source);

        /// <summary>この状態に入った時のアクション(片目閉じとか)の種類</summary>
        public abstract BlinkActionTypes EnterActionType { get; }
        /// <summary>マウスアクションに相当するほうのアクション種類</summary>
        public abstract BlinkActionTypes BlinkActionType { get; }

        public void Enter(SynchroGazerStatus source)
        {
            source.RaiseBlinkAction(EnterActionType);
            System.Windows.Forms.Cursor.Position 
                = new System.Drawing.Point((int)source.DisplayX, (int)source.DisplayY);
            source.FixDisplayPosition = true;
            _eyeOpened = GetTargetEyeOpened(source);
        }

        public void Quit(SynchroGazerStatus source)
        {
        }

        public IState<SynchroGazerStatus> Update(SynchroGazerStatus source)
        {
            if (!source.IsUserPresent)
            {
                source.RaiseBlinkAction(BlinkActionTypes.None);   
                return new NotPresentState();
            }

            if (ShouldGoToDefaultState(source))
            {
                source.RaiseBlinkAction(BlinkActionTypes.None);
                return new DefaultState();
            }

            bool blinkCheckNeeded = UpdateBlinkCount(source);
            if (blinkCheckNeeded &&
                StateMachineUtils.GetWinkCount(_blinks) >= GetCommandBlinkCount(source))
            {
                //NOTE: BlinkActionより先にFixを解除することが絶対に必要。
                //理由: マウスが視点アイコンと干渉するのを防ぐ為。
                source.FixDisplayPosition = false;
                source.RaiseBlinkAction(BlinkActionType);
                return new CommandEndState();
            }

            return this;
        }

        private bool UpdateBlinkCount(SynchroGazerStatus source)
        {
            if (_eyeOpened == GetTargetEyeOpened(source))
            {
                return false;
            }

            _eyeOpened = GetTargetEyeOpened(source);

            _blinks.Enqueue(new BlinkToggleData(
                source.DisplayX,
                source.DisplayY,
                source.TimeStamp,
                _eyeOpened
                ));

            StateMachineUtils.RefreshBlinkToggleQueue(
                _blinks, source.TimeStamp, GetBlinkDuration(source)
                );
            return true;
        }
    }
}
