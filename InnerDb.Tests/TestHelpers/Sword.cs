using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace InnerDb.Tests.TestHelpers
{
	class Sword
    {
        public string Name { get; set; }
        public uint Cost { get; set; }

		public Sword() { }

		public Sword(string name, uint cost)
		{
			this.Name = name;
			this.Cost = cost;
		}

        public override bool Equals(object obj)
        {
            if (obj is Sword)
            {
                var target = obj as Sword;
                return this.Name == target.Name && this.Cost == target.Cost;
            }
            else
            {
                return false;
            }
        }

		public override string ToString()
		{
			return string.Format("{0}: cost={1}", this.Name, this.Cost);
		}
    }
}
