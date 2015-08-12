using ToSic.Eav.BLL;

namespace ToSic.Eav.Persistence
{
    public class DbExtensionCommandsBase
    {
        public EavContext Context { get; internal set; }
        public EavDataController DataController { get; internal set; }


        //public DbExtensionCommandsBase(EavDataController dataController)
        //{
        //    DataController = dataController;
        //    Context = dataController.Context;
        //}

        public DbExtensionCommandsBase(EavContext cntx)
        {
            Context = cntx;
        }

    }
}
