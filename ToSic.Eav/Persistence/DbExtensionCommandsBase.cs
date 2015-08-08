namespace ToSic.Eav.Persistence
{
    public class DbExtensionCommandsBase
    {
        public EavContext Context { get; internal set; }

        public DbExtensionCommandsBase(EavContext cntx)
        {
            Context = cntx;
        }

    }
}
