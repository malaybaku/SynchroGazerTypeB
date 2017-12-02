using System.Collections.Generic;

namespace Baku.SynchroGazer
{
    public class DefaultState : IState<SynchroGazerStatus>
    {
        //NOTE: 決め打ちじゃなくす場合、source越しにSetting見に行くなどの対応が必要
        private long WinkThreashold => 300;
        private long BlinkDuration => 1500;
        private int CommandBlinkCount => 3;

        private readonly Queue<BlinkToggleData> _leftBlinks = new Queue<BlinkToggleData>();
        private readonly Queue<BlinkToggleData> _rightBlinks = new Queue<BlinkToggleData>();

        private long _leftWinkCountMillisec = 0;
        private long _rightWinkCountMillisec = 0;

        private long _lastTimestamp;
        private bool _leftEyeOpened;
        private bool _rightEyeOpened;


        public void Enter(SynchroGazerStatus source)
        {
            source.FixDisplayPosition = false;
            _lastTimestamp = source.TimeStamp;
            _leftEyeOpened = source.LeftEyeOpened;
            _rightEyeOpened = source.RightEyeOpened;
        }

        public void Quit(SynchroGazerStatus source)
        {
        }

        public IState<SynchroGazerStatus> Update(SynchroGazerStatus source)
        {
            if (!source.IsUserPresent)
            {
                return new NotPresentState();
            }

            UpdateWinkTime(source);
            bool blinkCountUpdated = UpdateBlinkCount(source);

            _lastTimestamp = source.TimeStamp;

            if (_leftWinkCountMillisec > WinkThreashold)
            {
                return new LeftWinkState();
            }
            else if (_rightWinkCountMillisec > WinkThreashold)
            {
                return new RightWinkState();
            }
            else if (
                blinkCountUpdated && 
                StateMachineUtils.GetWinkCount(_leftBlinks) >= CommandBlinkCount && 
                StateMachineUtils.GetWinkCount(_rightBlinks) >= CommandBlinkCount
                )
            {
                _leftBlinks.Clear();
                _rightBlinks.Clear();

                source.RaiseBlinkAction(BlinkActionTypes.Blink);
            }

            return this;
        }


        private void UpdateWinkTime(SynchroGazerStatus source)
        {
            if (!source.LeftEyeOpened && source.RightEyeOpened)
            {
                _leftWinkCountMillisec += source.TimeStamp - _lastTimestamp;
            }
            else
            {
                _leftWinkCountMillisec = 0;
            }

            if (!source.RightEyeOpened && source.LeftEyeOpened)
            {
                _rightWinkCountMillisec += source.TimeStamp - _lastTimestamp;
            }
            else
            {
                _rightWinkCountMillisec = 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns>Queueに新規にデータが投げ込まれたらtrue</returns>
        private bool UpdateBlinkCount(SynchroGazerStatus source)
        {
            bool result = false;

            if (_leftEyeOpened != source.LeftEyeOpened)
            {
                _leftEyeOpened = source.LeftEyeOpened;

                _leftBlinks.Enqueue(new BlinkToggleData(
                    source.DisplayX,
                    source.DisplayY,
                    source.TimeStamp,
                    source.LeftEyeOpened
                    ));
                result = true;
            }

            if (_rightEyeOpened != source.RightEyeOpened)
            {
                _rightEyeOpened = source.RightEyeOpened;

                _rightBlinks.Enqueue(new BlinkToggleData(
                    source.DisplayX,
                    source.DisplayY,
                    source.TimeStamp,
                    source.RightEyeOpened
                    ));
                result = true;
            }

            if (result)
            {
                StateMachineUtils.RefreshBlinkToggleQueue(_leftBlinks, source.TimeStamp, BlinkDuration);
                StateMachineUtils.RefreshBlinkToggleQueue(_rightBlinks, source.TimeStamp, BlinkDuration);
            }

            return result;
        }
    }
}
