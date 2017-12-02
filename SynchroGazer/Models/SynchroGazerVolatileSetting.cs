namespace Baku.SynchroGazer
{
    public class SynchroGazerVolatileSetting : NotifiableBase
    {
        public GazeActionTypes ActionType
        {
            get => (GazeActionTypes)GazeActionNumber;
            set => GazeActionNumber = (int)value;
        }

        private int _gazeActionNumber = 0;
        public int GazeActionNumber
        {
            get => _gazeActionNumber;
            set 
            {
                //-1が飛んでくることがある(選択解除のような主旨)が、その場合は弾く
                if (value < (int)GazeActionTypes.SingleClick ||
                    value > (int)GazeActionTypes.CenterClick)
                {
                    //無視したうえで自分の値に直すよう要求
                    NotifyPropertyChanged();
                }
                else
                {
                    SetValue(ref _gazeActionNumber, value);
                }
            }

        }
    }

    public enum GazeActionTypes
    {
        SingleClick = 0,
        DoubleClick = 1,
        CenterClick = 2,
    }

}
