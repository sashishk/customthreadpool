using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CustomThreadPool
{
	class Program
	{
		static void Main(string[] args)
		{
			TestCustomThreadPool test = new TestCustomThreadPool();
		}
	}

	public class TestCustomThreadPool
	{
		public TestCustomThreadPool()
		{
			SimpleThreadPool simple = new SimpleThreadPool();
			simple.SetMaxThreads(4);
			for (int i = 0; i < 50; i++)
			{
				Action u = new Action(Print);
				Action a = new Action(Printed);
				simple.QueueUserTask(u, a);
			}
			Console.ReadLine();
		}

		public void Print()
		{
			object o = new object();
			lock (o)
			{
				Thread.Sleep(1000);
				Console.WriteLine("i am printing  Executing Thread : " + Thread.CurrentThread.ManagedThreadId.ToString());
			}
		}

		public void Printed()
		{
			object o = new object();
			lock (o)
			{
				Console.WriteLine("I am Callback to Caller ");
			}
		}
	}

}
