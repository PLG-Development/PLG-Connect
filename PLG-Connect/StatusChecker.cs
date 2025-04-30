using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using PLG_Connect_Network;

namespace PLG_Connect;

public class StatusChecker
{
    private CancellationTokenSource _statusCheckCts;



    public void StartStatusChecker()
    {
        _statusCheckCts = new CancellationTokenSource();

        
        Task.Run(async () =>
        {
            while (!_statusCheckCts.Token.IsCancellationRequested)
            {
                foreach (var d in MainWindow._instance.SettingsManager.Settings.Displays)
                {
                    try
                    {
                        //await Dispatcher.UIThread.InvokeAsync(d.Ping());
                        await Dispatcher.UIThread.InvokeAsync(() => _ = d.Ping(true));
                        await Dispatcher.UIThread.InvokeAsync(() => MainWindow._instance.RefreshGUI());
                        // ggf. await d.GetDisplayOfflineOrPingableAsync() verwenden, wenn async
                        //d.Status = d.GetDisplayOfflineOrPingable();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Fehler beim Prüfen von {d.Address}: {ex.Message}");
                        d.Status = DisplayStatus.Offline;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(10), _statusCheckCts.Token);
            }
        }, _statusCheckCts.Token);
    }

    public void StopStatusChecker()
    {
        _statusCheckCts?.Cancel();
    }

}
