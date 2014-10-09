using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Helpers
{
	public static class ATMediaHelper
	{
		public static int GetImageSequence(string imageExtension)
		{
			imageExtension.ThrowArgNull("imageExtension");

			switch (imageExtension.ToLower())
			{
				case "f":
					return 0;
					break;
				case "b":
					return 1;
					break;
				case "l":
					return 3;
					break;
				case "h":
					return 4;
					break;
				default:
					throw new InvalidOperationException("Image not allowed");
			}

		}
	}
}
