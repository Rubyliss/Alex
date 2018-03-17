using System.Linq;
using System.Numerics;
using Veldrid;

namespace Alex.Engine
{
    public static class Extensions
    {
	    public static Vector2 ToVector2(this Point point)
	    {
			return new Vector2(point.X, point.Y);
	    }

	    public static Point ToPoint(this Vector2 vector)
	    {
			return new Point((int) vector.X, (int) vector.Y);
	    }

	    public static bool IsKeyDown(this InputSnapshot snapshot, Key key)
	    {
		    return snapshot.KeyEvents.Any(x => x.Key == key && x.Down);
	    }
	}
}
