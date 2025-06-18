namespace POCO.Models
{
    public enum SessionStatus
    {
        //
        // Summary:
        //     The task has been initialized but has not yet been scheduled.
        Created = 0,
        //
        // Summary:
        //     The task has been initialized but has not yet been scheduled.
        Ready = 1,
        // Summary:
        //     The task is running but has not yet completed.
        Running = 2,
        //
        // Summary:
        //     The task stopped by the user
        Stopped = 3,
        //
        // Summary:
        //     The task paused by the user.
        Paused = 4,
        //
        // Summary:
        //     The task completed execution successfully.
        Completed = 5,
        //
        // Summary:
        //     The task completed due to exception.
        Faulted = 6
    }
}
