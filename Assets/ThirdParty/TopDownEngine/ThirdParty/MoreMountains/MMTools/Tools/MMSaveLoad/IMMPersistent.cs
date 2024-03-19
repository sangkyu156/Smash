namespace MoreMountains.Tools
{
    /// <summary>
    /// MMPersistencyManager를 통해 저장하려는 인터페이스 클래스를 구현해야 합니다.
    /// </summary>
    public interface IMMPersistent
    {
	    /// <summary>
	    /// Needs to return a unique Guid used to identify this object 
	    /// </summary>
	    /// <returns></returns>
	    string GetGuid();
	    
	    /// <summary>
	    /// Returns a savable string containing the object's data
	    /// </summary>
	    /// <returns></returns>
	    string OnSave();

	    /// <summary>
	    /// Loads the object's data from the passed string and applies it to its properties
	    /// </summary>
	    /// <param name="data"></param>
	    void OnLoad(string data);

	    /// <summary>
	    /// Whether or not this object should be saved
	    /// </summary>
	    /// <returns></returns>
	    bool ShouldBeSaved();
    }
}
