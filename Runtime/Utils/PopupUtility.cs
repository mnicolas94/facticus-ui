using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Facticus.UI.Utils
{
    public static class PopupUtility
    {
        private static List<Func<UniTask>> _popupsTaskQueue = new ();

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeQueue()
        {
            _popupsTaskQueue = new();
        }
        
        public static void QueuePopup(Func<UniTask> openPopupFunction)
        {
            _popupsTaskQueue.Add(openPopupFunction);
            if (_popupsTaskQueue.Count == 1)
            {
                StartQueueLoop().Forget();
            }
        }

        private static async UniTask StartQueueLoop()
        {
            while (_popupsTaskQueue.Count > 0 && !Application.exitCancellationToken.IsCancellationRequested)
            {
                try
                {
                    var taskFunc = _popupsTaskQueue[0];
                    var task = taskFunc();
                    await task;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                _popupsTaskQueue.RemoveAt(0);
            }
        }
    }
}