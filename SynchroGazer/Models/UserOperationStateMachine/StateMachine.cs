namespace Baku.SynchroGazer
{
    public class StateMachine<T>
    {
        public StateMachine(IState<T> start)
        {
            _state = start;
        }

        private IState<T> _state;

        public void Update(T source)
        {
            var next = _state.Update(source);

            if (next != _state)
            {
                _state.Quit(source);
                next.Enter(source);
                _state = next;
            }
        }
    }

    public interface IState<T>
    {
        void Enter(T source);
        IState<T> Update(T source);
        void Quit(T source);
    }
}
