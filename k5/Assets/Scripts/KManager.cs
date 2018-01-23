

namespace KGUI
{
    public  class KManager
    {
        public static Stage stageInstance { get; private set; }
        public static GRoot grootInstance { get; private set; }
        public static Timers timersInstance { get; private set; }
        //记录数
        public static int countTimesNew { get { return _countTimesNew; }}
        private static int _countTimesNew = 0;
        public static void NewInstance()
        {
            _countTimesNew += 1;
            timersInstance = new Timers();
            stageInstance = new Stage();//严格顺序必须比GRoot先实例化
            grootInstance = new GRoot();
        }

        public static void DisposeInstance()
        {
            grootInstance.Dispose();
            grootInstance = null;
            stageInstance.Dispose();
            stageInstance = null;
            timersInstance = null;
            _countTimesNew -= 1;
        }


    }
}