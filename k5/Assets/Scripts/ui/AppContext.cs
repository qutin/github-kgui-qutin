

namespace KGUI
{
    public class AppContext
    {
        public Stage stage { get; protected set; }
        public GRoot groot { get; protected set; }
        public Timers timers { get; protected set; }
        public AppContext()
        {
            this.stage = new Stage();
            this.groot = new GRoot();
            this.groot.Init(this,"none");
            this.timers = new Timers();
        }
    }
}
