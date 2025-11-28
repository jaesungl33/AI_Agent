public interface IScreen
{
    public int SortingOrder { get; }
    void Show(int additionalSortingOrder = 0, ScreenParam param = null);
    void Hide();
    void Initialize();
}

public abstract class ScreenParam
{
    
}