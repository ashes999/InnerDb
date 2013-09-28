using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InnerDb.Tests.TestHelpers
{
    class Creature
    {
        public string Name { get; set; }
        public Alignment Disposition { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Creature)
            {
                var c = obj as Creature;
                return this.Name == c.Name && this.Disposition == c.Disposition;
            }
            else
            {
                return false;
            }
        }
    }

    public enum Alignment { Good, Evil }
}
