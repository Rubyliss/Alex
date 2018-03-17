using System.Collections.Generic;

namespace Alex.Engine.Content
{
	internal abstract class ContentLoader : DisposableBase
	{
		public virtual object PlaceholderValue => null;

		public virtual IEnumerable<string> GetPossibleFilePaths(string filePath)
		{
			yield return filePath;
		}

		public abstract object Load(string entry, ContentManager contentManager, Game game, ContentManager.LoadOptions loadOptions);
	}
}
