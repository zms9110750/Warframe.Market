using System.Runtime.CompilerServices;

namespace Warframe.Market.Helper;
public class ThrottleGate(TimeSpan interval)
{
	public TimeSpan Interval { get; } = interval;
	public DateTimeOffset NextPassTime { get; private set; } = DateTime.Now;
	Lock Lock { get; } = new Lock();
	public TaskAwaiter GetAwaiter()
	{
		lock (Lock)
		{
			TaskAwaiter awaiter;
			if (NextPassTime < DateTime.Now)
			{
				awaiter = Task.CompletedTask.GetAwaiter();
				NextPassTime = DateTime.Now + Interval;
			}
			else
			{
				awaiter = Task.Delay(NextPassTime - DateTime.Now).GetAwaiter();
				NextPassTime += Interval;
			}
			return awaiter;
		}
	}
}