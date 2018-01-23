using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGUI
{
    public class ZipEntry
    {
        public int pkg;
		public string name;
		public uint offset;
		public uint size;
		public uint sourceSize;
		public int compress;
		public uint crc;
		public byte data;
    }
}
