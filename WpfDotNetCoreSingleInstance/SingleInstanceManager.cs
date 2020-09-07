using System.Threading;

namespace WpfDotNetCoreSingleInstance
{
    public class SingleInstanceManager
    {
        public Mutex Mutex { get; }

        public bool IsOnlyInstance { get; }

        public SingleInstanceManager(string identifier)
        {
            var isOnlyInstance = false;

            Mutex = new Mutex(true, identifier, out isOnlyInstance);
            
            IsOnlyInstance = isOnlyInstance;

        }
    }
}