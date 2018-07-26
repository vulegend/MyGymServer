using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HookappServer
{
    public static class Extensions
    {
        public static void Append(this byte[] destination,byte[] source,int offset)
        {
            Buffer.BlockCopy(source, 0, destination, offset, source.Length);
        }
    }
}
