using System;

namespace Alex.Engine
{
    public class GameTime
    {
	    public TimeSpan TotalGameTime { get; set; }

	    public TimeSpan ElapsedGameTime { get; set; }

	    public bool IsRunningSlowly { get; set; }

	    public GameTime()
	    {
		    TotalGameTime = TimeSpan.Zero;
		    ElapsedGameTime = TimeSpan.Zero;
		    IsRunningSlowly = false;
	    }

	    public GameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime)
	    {
		    TotalGameTime = totalGameTime;
		    ElapsedGameTime = elapsedGameTime;
		    IsRunningSlowly = false;
	    }

	    public GameTime(TimeSpan totalRealTime, TimeSpan elapsedRealTime, bool isRunningSlowly)
	    {
		    TotalGameTime = totalRealTime;
		    ElapsedGameTime = elapsedRealTime;
		    IsRunningSlowly = isRunningSlowly;
	    }
	}
}
