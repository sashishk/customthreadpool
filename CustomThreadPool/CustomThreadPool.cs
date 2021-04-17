using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CustomThreadPool
{
	/// <summary>
	/// Thread State
	/// </summary>
	public enum ThreadState
	{
		waiting,
		working,
		completed
	}

	/// <summary>
	/// Task status 
	/// </summary>
	public enum TaskStatus
	{
		starting,
		done
	}

	/// <summary>
	/// Custom Thread
	/// </summary>
	public class CustomThread
	{
		public ThreadState state;
		public Thread workhorse;
		public Guid id;
		public CustomTask taskToExecute;
	}

	/// <summary>
	/// Custom Task which has a callback
	/// </summary>
	public class CustomTask
	{
		public Action task;
		public Action callback;
		public TaskStatus taskstatus;
	}

	/// <summary>
	/// Simple Thread Pool
	/// </summary>
	public class SimpleThreadPool
	{
		public int _minThread;
		public int _maxThread;
		public ConcurrentQueue<CustomTask> TaskQueue = new ConcurrentQueue<CustomTask>();
		public ConcurrentQueue<CustomThread> ThreadQueue = new ConcurrentQueue<CustomThread>();

		public SimpleThreadPool()
		{
			RunTask();
		}

		public void SetMaxThreads(int max)
		{
			_maxThread = max;
		}

		private void AddThread(CustomThread t)
		{
			t.workhorse = new Thread(() =>
			{
				while (true)
				{
					bool taskhandling = false;
					bool casehandled = false;
					lock (t)
					{
						if (t.taskToExecute.taskstatus == TaskStatus.starting && t.state == ThreadState.waiting)
						{
							taskhandling = true;
							casehandled = true;

						}
						else
						{
							taskhandling = false;
							casehandled = false;
						}
						if (taskhandling)
						{
							try
							{
								t.state = ThreadState.working;
								t.taskToExecute.task.Invoke();
								taskhandling = false;
							}
							catch (Exception ex)
							{
								t.state = ThreadState.completed;
							}
							t.taskToExecute.taskstatus = TaskStatus.done;
						}

						if (t.taskToExecute.callback != null && t.taskToExecute.taskstatus == TaskStatus.done && casehandled)
						{
							t.state = ThreadState.completed;
							t.taskToExecute.callback();
							casehandled = false;
						}
					}
				}
			});
			t.workhorse.Start();
			ThreadQueue.Enqueue(t);
		}

		/// <summary>
		/// 
		/// </summary>
		public void RunTask()
		{
			Thread sceduler = new Thread(() =>
			{
				while (true)
				{
					CustomTask t;
					TaskQueue.TryPeek(out t);
					if (t == null)
					{
						continue;
					}

					bool taskadded = false;
					int count = TaskQueue.Count;
					for (int i = 0; i < count; i++)
					{
						foreach (CustomThread thread in ThreadQueue)
						{
							lock (thread)
							{
								if (thread.state == ThreadState.completed)
								{
									Console.WriteLine("Free Thread :" + thread.id + "   *** threads :" + ThreadQueue.Count.ToString());
									CustomTask customtask;
									TaskQueue.TryDequeue(out customtask);
									if (customtask == null)
										break;
									thread.state = ThreadState.waiting;
									lock (customtask)
									{
										thread.taskToExecute = customtask;
										taskadded = true;
									}
									break;
								}
							}
						}

						if (!taskadded && ThreadQueue.Count < _maxThread)
						{
							CustomThread thread2 = new CustomThread();
							lock (thread2)
							{
								thread2.id = Guid.NewGuid();
								thread2.state = ThreadState.waiting;
								CustomTask customtask2;
								TaskQueue.TryDequeue(out customtask2);
								if (customtask2 == null)
									break;
								thread2.taskToExecute = customtask2;
								AddThread(thread2);
								taskadded = true;
							}
						}
						else if (!taskadded)
						{
							continue;
						}
					}
				}
			});

			sceduler.Start();
		}

		public void QueueUserTask(Action task, Action callback)
		{
			CustomTask t = new CustomTask();
			t.callback = callback;
			t.task = task;
			t.taskstatus = TaskStatus.starting;
			TaskQueue.Enqueue(t);
		}
	}
}
