﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using Alex.Engine;


namespace Alex.API.World
{
    public class TickManager
	{
		private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(TickManager));

		private IWorld World { get; }
		private ConcurrentDictionary<Action, long> _scheduledTicks { get; }
		private long _tick = 0;
	    public TickManager(IWorld world)
	    {
		    World = world;
		    _scheduledTicks = new ConcurrentDictionary<Action, long>();
		}

		private TimeSpan _lastTickTime = TimeSpan.Zero;
		public void Update(GameTime gameTime)
		{
			if ((gameTime.TotalGameTime - _lastTickTime).TotalMilliseconds >= 50)
			{
				_lastTickTime = gameTime.TotalGameTime;
				var ticks = _scheduledTicks.Where(x => x.Value <= _tick).ToArray();

				foreach (var tick in ticks)
				{
					_scheduledTicks.TryRemove(tick.Key, out long _);
				}

				//Executed scheduled ticks
				foreach (var tick in ticks)
				{
					try
					{
						tick.Key.Invoke();
					}
					catch (Exception ex)
					{
						Log.Error($"An exception occureced while executing a scheduled tick!", ex);
					}
				}

				_tick++;
			}
		}

		public void ScheduleTick(Action action, long ticksFromNow)
		{
			if (!_scheduledTicks.TryAdd(action, _tick + ticksFromNow))
			{
				Log.Warn($"Could not schedule tick!");
			}
		}
    }
}
