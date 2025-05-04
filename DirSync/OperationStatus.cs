/// <summary>
/// Represents the status of an operation, tracking whether any failure occurred.<br/>
/// Exposes a <see cref="Success"/> property to check final result. <br/>
/// And methods <see cref="AssertTrue(bool)"/> and <see cref="Fail()"/> to report failure.
/// </summary>
public class OperationStatus
{
    /// <summary>
    /// Indicates whether the operation completed successfully (i.e., no failures were recorded).
    /// </summary>
    public bool Success => !_hasFailed;
    private bool _hasFailed = false;

    /// <summary>
    /// Marks the operation as failed if the provided condition is false.
    /// </summary>
    public void AssertTrue(bool result)
    {
        if (!result) _hasFailed = true;
    }

    public void Fail()
    {
        _hasFailed = true;
    }
}