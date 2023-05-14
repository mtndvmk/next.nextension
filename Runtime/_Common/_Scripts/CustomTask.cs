using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class CustomTask<T>
{
    private CancellationTokenSource m_CancellationToken;

    public delegate Task<T> TaskWillBeRun(CancellationToken token);

    private TaskWillBeRun m_CurrentTask;

    public CustomTask(TaskWillBeRun task)
    {
        m_CurrentTask = task;
    }

    public void start()
    {
        if (m_CancellationToken != null)
        {
            Debug.Log("Task has started");
        }
        else
        {
            Debug.Log("Run task in object: " + m_CurrentTask.Target);
            m_CancellationToken = new CancellationTokenSource();
            Task.Run(() => taskWorker(m_CancellationToken.Token), m_CancellationToken.Token);
        }
    }

    /// <summary>
    /// Invoke cancel token of task
    /// </summary>
    public void stop()
    {
        m_CancellationToken.Cancel();
    }

    private Task<T> taskWorker(CancellationToken token)
    {
        Task<T> result = default(Task<T>);
        try
        {
            result = m_CurrentTask(token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Task is be cancelled");
        }
        catch (Exception e)
        {
            Debug.Log("Task is be error: " + e);
        }
        return result;
    }

    ~CustomTask()
    {
        m_CancellationToken?.Cancel();
    }
}

public class CustomTask
{
    private CancellationTokenSource m_CancellationToken;
    private Action<CancellationToken> m_CurrentTask;

    public CancellationToken CancellationToken { get { return m_CancellationToken.Token; } }

    public CustomTask(Action<CancellationToken> task)
    {
        m_CurrentTask = task;
    }

    public bool start()
    {
        if (m_CancellationToken != null)
        {
            Debug.Log("Task has started");
            return false;
        }
        else
        {
            m_CancellationToken = new CancellationTokenSource();
            return Task.Run(() => taskWorker(m_CancellationToken.Token), m_CancellationToken.Token) != null;
        }
    }

    /// <summary>
    /// Invoke cancel token of task
    /// </summary>
    public void stop()
    {
        m_CancellationToken.Cancel();
    }

    private void taskWorker(CancellationToken token)
    {
        try
        {
            m_CurrentTask(token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Task is be cancelled");
        }
        catch (Exception e)
        {
            Debug.Log("Task is be error: " + e);
        }
    }

    ~CustomTask()
    {
        m_CancellationToken?.Cancel();
    }
}
