using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InnerDb.Tests.TestHelpers
{
	class Car
	{
		public string Make { get; set; }
		public string Model { get; set; }
		public string Colour { get; set; }

		public Car(string make, string model, string colour)
		{
			this.Make = make;
			this.Model = model;
			this.Colour = colour;
		}
	}
}
