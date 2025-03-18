namespace RDPMS.Core.Persistence.Model;

public enum JobState
{
    /// <summary>
    /// Job is created, but not executed
    /// </summary>
    Created = 0,
    
    /// <summary>
    /// Job is assigned to a worker, and execution is prepared
    /// </summary>
    Pending = 1,
    
    /// <summary>
    /// Job is being executed right now
    /// </summary>
    Running = 2,
    
    /// <summary>
    /// The job has terminated with an error
    /// </summary>
    Failed = 3,
    
    /// <summary>
    /// The job has terminated successfully
    /// </summary>
    Finished = 4,
    
    /// <summary>
    /// The process was terminated remotely
    /// </summary>
    Terminated = 5,
    
    /// <summary>
    /// The runner did not send an update for a certain period of time
    /// </summary>
    RunnerNotResponding = 6
}