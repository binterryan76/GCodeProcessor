namespace CachedProperty;

public class CachedProperty<T>
{
    /// <summary>
    /// Set this to true to cause the Value getter to call the update function.
    /// </summary>
    public bool NeedsUpdate { get; set; }
    private Func<T> updateFunction;

    private T value;
    public T Value
    {
        get 
        { 
            if(NeedsUpdate)
                value = updateFunction();

            return value; 
        }
        set
        {
            this.value = value;
            NeedsUpdate = false;
        }
    }

    public CachedProperty(T initialValue, Func<T> updateFunction, bool needsUpdate = true)
    {
        this.value = initialValue;
        this.updateFunction = updateFunction;
        NeedsUpdate = needsUpdate;
    }
}