using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tobii.Interaction;
using Tobii.Interaction.Framework;

namespace Baku.SynchroGazer
{
    /// <summary>
    /// 実行時に変化するステータスの一覧です。
    /// </summary>
    public class SynchroGazerStatus : NotifiableBase, IDisposable
    {
        public SynchroGazerStatus()
        {
            Host = new Host();
            Host.Streams.CreateGazePointDataStream().GazePoint(OnGazePointData);
            Host.Streams.CreateEyePositionStream().EyePosition(OnEyePositionData);
            Task.Run(() => CheckUserPresense(_cts.Token));

            _timer.Start();
        }

        //安全な開始ステートとしてユーザ不在から始める(普通はすぐデフォルト状態に切り替わる)
        private readonly StateMachine<SynchroGazerStatus> _stateMachine
             = new StateMachine<SynchroGazerStatus>(new NotPresentState());

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly Stopwatch _timer = new Stopwatch();

        private readonly Queue<GazeData> _gazeData = new Queue<GazeData>();

        public Host Host { get; }

        private int GazeDataDuration => 1000;
        //注目位置をローパスするときに適用するレイテンシ
        private int GazeDataFilterLatency => 200;

        //常に計算して更新されている値
        public double _currentX = 0;
        public double _currentY = 0;

        //DisplayX, YはFixしてなければCurrentと一致、FixするとCurrentとの同期が止まる
        private bool _fixDisplayPosition = false;
        public bool FixDisplayPosition
        {
            get => _fixDisplayPosition;
            set => SetValue(ref _fixDisplayPosition, value);
        } 
        private double _displayX = 0;
        public double DisplayX
        {
            get => _displayX;
            private set => SetValue(ref _displayX, value);
        }
        private double _displayY = 0;
        public double DisplayY
        {
            get => _displayY;
            private set => SetValue(ref _displayY, value);
        }

        //状態遷移の判定に使うのはこのあたり
        private bool _leftEyeOpened = false;
        public bool LeftEyeOpened
        {
            get => _leftEyeOpened;
            private set => SetValue(ref _leftEyeOpened, value);
        }
        private bool _rightEyeOpened = false;
        public bool RightEyeOpened
        {
            get => _rightEyeOpened;
            private set => SetValue(ref _rightEyeOpened, value);
        }
        public bool _isUserPresent = false;
        public bool IsUserPresent 
        {
            get => _isUserPresent;
            private set => SetValue(ref _isUserPresent, value);
        }

        /// <summary>
        /// このクラスが独自にミリ秒単位で管理する時刻。
        /// </summary>
        /// <remarks>
        /// ユーザプレゼンス情報にTobiiのタイムスタンプが付属しないので、自前で測るしかない。
        /// </remarks>
        public long TimeStamp => _timer.ElapsedMilliseconds;

        public void Dispose()
        {
            _timer.Stop();
            _cts.Cancel();
            Host.Dispose();
        }


        //NOTE: 設計的にはここpublicになってるのはおかしいが、ステートマシンの都合考えるとコッチの方がラク
        public void RaiseBlinkAction(BlinkActionTypes actionType)
            => BlinkActionHappened?.Invoke(this, new BlinkActionEventArgs(DisplayX, DisplayY, actionType));

        public event EventHandler<BlinkActionEventArgs> BlinkActionHappened;

        private void OnEyePositionData(EyePositionData data)
        {
            LeftEyeOpened = data.HasLeftEyePosition;
            RightEyeOpened = data.HasRightEyePosition;
            _stateMachine.Update(this);
        }

        private void OnGazePointData(double x, double y, double timeStamp)
        {
            _gazeData.Enqueue(new GazeData(x, y, TimeStamp));
            RefreshGazeDataQueue();
            UpdateFilteredPosition();

            if (!FixDisplayPosition)
            {
                DisplayX = _currentX;
                DisplayY = _currentY;
            }

            _stateMachine.Update(this);
        }

        private void UpdateFilteredPosition()
        {
            if (_gazeData.Any(d => d.Time < TimeStamp - GazeDataFilterLatency))
            {
                //データがじゅうぶん溜まっていて、レイテンシつき平均が求まる場合(ふだんはコレ)
                _currentX = _gazeData
                    .Where(d => d.Time < TimeStamp - GazeDataFilterLatency)
                    .Average(d => d.X);
                _currentY = _gazeData
                    .Where(d => d.Time < TimeStamp - GazeDataFilterLatency)
                    .Average(d => d.Y);
            }
            else if (_gazeData.Count > 0)
            {
                //データが溜まっておらず、レイテンシつけると要素が無くなってしまう(データ取り始めの瞬間)場合
                //レイテンシつけた場合と出力値が連続してほしい -> いちばん古いデータ使うしかない
                _currentX = _gazeData.Peek().X;
                _currentY = _gazeData.Peek().Y;
            }
            else
            {
                //データない -> 考えても仕方ないので0,0(本来ここには来ない)
                _currentX = 0;
                _currentY = 0;
            }
        }

        private async void CheckUserPresense(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(32);
                    var presence = await Host.States.GetUserPresenceAsync();
                    IsUserPresent =
                        presence.IsValid &&
                        (presence.Value == UserPresence.Present);
                    _stateMachine.Update(this);
                }
                catch(Exception ex)
                {
                    System.Windows.
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void RefreshGazeDataQueue()
        {
            while (
                _gazeData.Count > 0 && 
                _gazeData.Peek().Time < _timer.ElapsedMilliseconds - GazeDataDuration
                )
            {
                _gazeData.Dequeue();
            }
        }
    }


    public struct GazeData
    {
        public GazeData(double x, double y, long time) : this()
        {
            X = x;
            Y = y;
            Time = time;
        }

        public double X { get; }
        public double Y { get; }
        public long Time { get; }
    }

    public struct BlinkToggleData
    {
        public BlinkToggleData(double x, double y, long time, bool isOpened) : this()
        {
            X = x;
            Y = y;
            Time = time;
            IsOpened = isOpened;
        }

        public double X { get; }
        public double Y { get; }
        public long Time { get; }

        public bool IsOpened { get; }
    }


    public class BlinkActionEventArgs : EventArgs
    {
        public BlinkActionEventArgs(double x, double y, BlinkActionTypes actionType)
        {
            X = x;
            Y = y;
            ActionType = actionType;
        }

        public double X { get; }
        public double Y { get; }

        public BlinkActionTypes ActionType { get; }

    }

    /// <summary>
    /// 瞬きアクションの種類
    /// </summary>
    public enum BlinkActionTypes
    {
        /// <summary>デフォルト状態</summary>
        /// <remarks>
        /// LeftEyeClosedとかRightEyeClosedからBlinkに派生しなかったとき、キャンセル信号的に使われる。
        /// </remarks>
        None,
        /// <summary>両目で、短時間に3回まばたきする</summary>
        Blink,
        /// <summary>左目のみを閉じた状態</summary>
        LeftEyeClosed,
        /// <summary>右目のみを閉じた状態</summary>
        RightEyeClosed,
        /// <summary>左目を閉じた状態で、右目を短時間に3回瞬きする</summary>
        LeftEyeClosedBlink,
        /// <summary>右目を閉じた状態で、左目を短時間に3回瞬きする</summary>
        RightEyeClosedBlink,
    }

}
