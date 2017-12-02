using System;

namespace Baku.SynchroGazer
{
    public class MouseController
    {
        public MouseController(
            SettingFile setting,
            SynchroGazerVolatileSetting volatileSetting, 
            SynchroGazerStatus status
            )
        {
            _setting = setting;
            _volatileSetting = volatileSetting;
            _status = status;

            _status.BlinkActionHappened += OnBlinkAction;
        }

        private readonly SettingFile _setting;
        private readonly SynchroGazerVolatileSetting _volatileSetting;
        private readonly SynchroGazerStatus _status;


        /// <summary>
        /// マウス操作を実際に行った際に発生します。
        /// </summary>
        public event EventHandler<MouseActionEventArgs> MouseActionStart;

        private async void OnBlinkAction(object sender, BlinkActionEventArgs e)
        {
            MouseActionTypes mouseAction = GetMouseActionTypeFrom(
                e.ActionType,
                _volatileSetting.ActionType,
                _setting.Setting
                );

            int interval = (int)_setting.Setting.MouseDoubleClickIntervalMillisec;

            //処理の前に撃つ: タスク開始時点で実行するという事自体は確定するので。
            if (mouseAction != MouseActionTypes.None)
            {
                MouseActionStart?.Invoke(this, new MouseActionEventArgs(e.X, e.Y, mouseAction));
            }

            await ClickCommandExecutor.DoMouseActionAsync(
                e.X, e.Y, mouseAction, interval
                );
        }

        private static MouseActionTypes GetMouseActionTypeFrom(
            BlinkActionTypes blinkAction,
            GazeActionTypes gazeAction,
            SynchroGazerSetting setting
            )
        {
            //ガード: Wink以外禁止してるとき
            if (setting.AllowOnlyWink && blinkAction == BlinkActionTypes.Blink)
            {
                return MouseActionTypes.None;
            }

            //中央クリックは問答無用で中央クリックにマッピング(これでいいのかという話もあるが)
            if (gazeAction == GazeActionTypes.CenterClick)
            {
                return MouseActionTypes.CenterClick;
            }

            //シングル or ダブルクリック -> 反転設定をチェックしつつ。

            if (gazeAction == GazeActionTypes.SingleClick)
            {
                switch (blinkAction)
                {
                    case BlinkActionTypes.Blink:
                        return MouseActionTypes.LeftSingleClick;
                    case BlinkActionTypes.LeftEyeClosedBlink:
                        return (setting.IsReverseMode) ?
                            MouseActionTypes.RightSingleClick :
                            MouseActionTypes.LeftSingleClick;
                    case BlinkActionTypes.RightEyeClosedBlink:
                        return (setting.IsReverseMode) ?
                            MouseActionTypes.LeftSingleClick :
                            MouseActionTypes.RightSingleClick;
                }
            }

            if (gazeAction == GazeActionTypes.DoubleClick)
            {
                switch (blinkAction)
                {
                    case BlinkActionTypes.Blink:
                        return MouseActionTypes.LeftDoubleClick;
                    case BlinkActionTypes.LeftEyeClosedBlink:
                        return (setting.IsReverseMode) ?
                            MouseActionTypes.RightDoubleClick :
                            MouseActionTypes.LeftDoubleClick;
                    case BlinkActionTypes.RightEyeClosedBlink:
                        return (setting.IsReverseMode) ?
                            MouseActionTypes.LeftDoubleClick :
                            MouseActionTypes.RightDoubleClick;
                }
            }

            //ここには来ないはず
            return MouseActionTypes.None;
        }

    }

    public class MouseActionEventArgs : EventArgs
    {
        public MouseActionEventArgs(double x, double y, MouseActionTypes actionType)
        {
            X = x;
            Y = y;
            ActionType = actionType;
        }

        /// <summary>
        /// スクリーン座標(Win32APIにそのまま渡す値)のX
        /// </summary>
        public double X { get; }
        /// <summary>
        /// スクリーン座標(Win32APIにそのまま渡す値)のY
        /// </summary>
        public double Y { get; }

        public MouseActionTypes ActionType { get; }
    }
}
