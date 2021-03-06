﻿using System;
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

		public override bool Equals(object obj)
		{
			if (obj is Car)
			{
				var target = obj as Car;
				return this.Make == target.Make && 
					this.Model == target.Model && 
					this.Colour == target.Colour;
			}
			else
			{
				return false;
			}
		}

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", this.Colour, this.Make, this.Model);
        }
	}
}
