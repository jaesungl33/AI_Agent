
using UnityEngine.Events;

public interface IInitializableManager
{
    public UnityAction<bool> OnInitialized { get; set; }
    public void Initialize();
}