using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace HandyControlDemo
{
    public static class DispatcherBuilder
    {
        public static Dispatcher Build()
        {
            Dispatcher dispatcher = null;
            var manualResetEvent = new ManualResetEvent(false);
            var thread = new Thread(() =>
            {
                dispatcher = Dispatcher.CurrentDispatcher;
                var synchronizationContext = new DispatcherSynchronizationContext(dispatcher);
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);

                manualResetEvent.Set();

                try
                {
                    Dispatcher.Run();
                }
                catch
                {
                    // ignore
                }
            }, maxStackSize: 1);
            thread.Priority = ThreadPriority.Normal;
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            manualResetEvent.WaitOne();
            manualResetEvent.Dispose();
            return dispatcher;
        }
    }
}
