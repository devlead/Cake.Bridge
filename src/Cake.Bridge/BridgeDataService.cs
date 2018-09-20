using Cake.Core;

internal class BridgeDataService : ICakeDataService
{
    private System.Collections.Concurrent.ConcurrentDictionary<System.Type, object> Repository { get; } = new System.Collections.Concurrent.ConcurrentDictionary<System.Type, object>();

    public void Add<TData>(TData value) where TData : class
    {
        Repository.AddOrUpdate(
            typeof(TData),
            key=>value,
            (key, oldValue)=>value
            );
    }

    public TData Get<TData>() where TData : class
    {
        return Repository[typeof(TData)] as TData;
    }
}