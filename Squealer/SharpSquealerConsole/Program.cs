namespace Squealer
{
    public class GitFlags
    {
        private bool _ShowUncommitted = false;
        private bool _ShowDeleted = false;
        private bool _ShowHistory = false;

        public bool ShowUncommitted
        {
            get
            {
                return _ShowUncommitted;
            }
            set
            {
                _ShowUncommitted = value;
            }
        }

        public bool ShowDeleted
        {
            get
            {
                return _ShowDeleted & _ShowUncommitted;
            }
            set
            {
                _ShowDeleted = value;
            }
        }

        public bool ShowHistory
        {
            get
            {
                return _ShowHistory;
            }
            set
            {
                _ShowHistory = value;
            }
        }

        public bool GitEnabled
        {
            get
            {
                return _ShowUncommitted;
            }
        }
    }

    internal class Program
    {
        #region Main Functions
        static void Main()
        {
            My.Logging.WriteLog("Startup.");

            DefineCommands();

        }
        #endregion

        #region Commands
        private static void DefineCommands()
        {
            //Squealer.CommandCatalog.CommandDefinition = cmd;
        }
        #endregion

    }
}
