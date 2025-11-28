public interface IEventManager
{
    void Subscribe<T>(System.Action<T> listener);
    void Unsubscribe<T>(System.Action<T> listener);
    void Invoke<T>(T eventData);
}
