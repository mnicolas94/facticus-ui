using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Facticus.UI.WindowInterfaces;
using UnityEngine;
using Void = Facticus.UI.WindowInterfaces.Void;

namespace Facticus.UI
{
    /// <summary>
    /// A state of the user interface. It is used to open and close windows in a given state.
    /// </summary>
    public interface IUiState
    {
        public List<GameObject> GetWindowsPrefabs();
    }

    public static class UiStateExtensions
    {
        public static bool IsOpen(this IUiState state) => AreAllInStatus(state, WindowStatus.Open);

        public static bool IsClosed(this IUiState state) => AreAllInStatus(state, WindowStatus.Closed);
        
        public static bool IsOpening(this IUiState state) => IsAnyInStatus(state, WindowStatus.Opening);

        public static bool IsClosing(this IUiState state) => IsAnyInStatus(state, WindowStatus.Closing);

        public static bool IsTransitioning(this IUiState state) => IsOpening(state) || IsClosing(state);

        private static bool AreAllInStatus(this IUiState state, WindowStatus status)
        {
            var manager = WindowsManager.GetOrCreate();
            foreach (var windowPrefab in state.GetWindowsPrefabs())
            {
                var isInStatus = manager.GetStatus(windowPrefab) == status;
                if (!isInStatus)
                {
                    return false;
                }
            }
                
            return true;
        }
        
        private static bool IsAnyInStatus(this IUiState state, WindowStatus status)
        {
            var manager = WindowsManager.GetOrCreate();
            foreach (var windowPrefab in state.GetWindowsPrefabs())
            {
                var isInStatus = manager.GetStatus(windowPrefab) == status;
                if (isInStatus)
                {
                    return true;
                }
            }
                
            return false;
        }
        
        public static void Open(this IUiState state, UiStateOpenInfo info)
        {
            var windowsManager = WindowsManager.GetOrCreate();
            
            if (info.CloseOther)
            {
                if (info.KeepHistory)
                {
                    windowsManager.OpenNewHistoryList();
                }
                else
                {
                    windowsManager.CloseOthers(state.GetWindowsPrefabs());
                }
            }
            
            windowsManager.OpenAll(state.GetWindowsPrefabs());
        }

        public static void Open<T>(this IUiState state, T initializationData, UiStateOpenInfo info)
        {
            var windowsManager = WindowsManager.GetOrCreate();
            state.Open(info);
            foreach (var windowPrefab in state.GetWindowsPrefabs())
            {
                if (windowsManager.TryGetWindowInstance(windowPrefab, out var windowInstance))
                {
                    if (windowInstance.TryGetWindowInterface<IWindowInitializable<T>>(out var windowInitializable))
                    {
                        windowInitializable.Initialize(initializationData);
                    }
                }
            }
        }
        
        /// <summary>
        /// Open a UiState that has a <see cref="IWindowPopup"/> component and will close automatically after
        /// <see cref="IWindowPopup.WaitPopup"/> ends. Useful for popups.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ct"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async UniTask OpenAsPopup(this IUiState state, UiStateOpenInfo info, CancellationToken ct)
        {
            // open the windows
            state.Open(info);

            // wait for popup
            await state.WaitResult<Void>(ct);

            // close it
            Close(state, info);
        }
        
        /// <summary>
        /// Same as <see cref="OpenAsPopup"/> but the window can be initialized with an argument as well.
        /// </summary>
        /// <param name="initializationData"></param>
        /// <param name="info"></param>
        /// <param name="ct"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async UniTask OpenAsPopup<T>(this IUiState state, T initializationData,
            UiStateOpenInfo info, CancellationToken ct)
        {
            // open the windows
            state.Open(initializationData, info);

            // wait for result
            await state.WaitResult<Void>(ct);
            
            // close it
            Close(state, info);
        }
        
        /// <summary>
        /// Open a UiState that has a <see cref="IWindowWithResult{T}"/> window and will close automatically after
        /// the result is available. Useful for popups that return a result.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ct"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async UniTask<T> OpenAsPopup<T>(this IUiState state, UiStateOpenInfo info, CancellationToken ct)
        {
            // open the windows
            state.Open(info);

            // wait for result
            var result = await state.WaitResult<T>(ct);

            // close it
            Close(state, info);
            
            // return result
            return result;
        }

        /// <summary>
        /// Same as <see cref="OpenAsPopup{T}"/> but the window can be initialized as well.
        /// </summary>
        /// <param name="initializationData"></param>
        /// <param name="info"></param>
        /// <param name="ct"></param>
        /// <typeparam name="TI"></typeparam>
        /// <typeparam name="TO"></typeparam>
        /// <returns></returns>
        public static async UniTask<TO> OpenAsPopup<TI, TO>(this IUiState state, TI initializationData, UiStateOpenInfo info, CancellationToken ct)
        {
            // open the windows
            state.Open(initializationData, info);

            // wait for result
            var result = await state.WaitResult<TO>(ct);
            
            // close it
            Close(state, info);
            
            // return result
            return result;
        }
        
        /// <summary>
        /// This is a useful method to access WindowsManager.CloseCurrentHistoryList() method, but it is not
        /// tied to a particular opened state since it will close all states open in the current history list.
        /// </summary>
        public static void CloseGoBackHistory(this IUiState state)
        {
            WindowsManager.GetOrCreate().CloseCurrentHistoryList();
        }

        public static void Close(this IUiState state)
        {
            WindowsManager.GetOrCreate().CloseAll(state.GetWindowsPrefabs());
        }
        
        /// <summary>
        /// Close according to the info to open it
        /// </summary>
        /// <param name="state"></param>
        /// <param name="info"></param>
        private static void Close(IUiState state, UiStateOpenInfo info)
        {
            if (info.CloseOther && info.KeepHistory)
            {
                state.CloseGoBackHistory();
            }
            else
            {
                state.Close();
            }
        }

        public static async UniTask WaitUntilClose(this IUiState state, CancellationToken ct)
        {
            while (!state.IsClosed() && !ct.IsCancellationRequested)
            {
                await UniTask.Yield();
            }
        }
        
        private static async UniTask<T> WaitResult<T>(this IUiState state, CancellationToken ct)
        {
            var firstPrefab = state.GetWindowsPrefabs()[0];  // only supports getting a result from the first window
            if (WindowsManager.GetOrCreate().TryGetWindowInstance(firstPrefab, out var windowInstance))
            {
                if (windowInstance.TryGetWindowInterface<IWindowWithResult<T>>(out var windowWithResult))
                {
                    var result = await windowWithResult.WaitForResult(ct);
                    
                    return result;
                }
            }
            
            return default;
        }
    }
}