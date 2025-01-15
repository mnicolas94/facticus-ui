namespace UI
{
    /// <summary>
    /// An interface from which every windows interface should extend. It is used to avoid boxing when adding interfaces
    /// to <see cref="WindowInstance"/> with <see cref="WindowInstance.AddWindowInterface"/>.
    /// </summary>
    public interface IWindowInterface
    {
    }
}