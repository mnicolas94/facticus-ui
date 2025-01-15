namespace UI
{
    /// <summary>
    /// A window that can be initialized. Used with <see cref="UiStatePrefabGroup.Open{T}"/> and <see cref="UiStatePrefabGroup.OpenWaitResult{T1, T2}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWindowInitializable<T> : IWindowInterface
    {
        void Initialize(T data);
    }
}