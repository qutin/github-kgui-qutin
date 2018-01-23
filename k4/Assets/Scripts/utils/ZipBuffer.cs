using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using UnityEngine;

namespace KGUI
{
    public class ZipBuffer
    {
        private ByteBuffer iStream;
		private List<ZipEntry> iEntries;   //Vector.<ZipEntry>;
		private int iPkg;
		//private int iValid;
		private IDictionary iNameIndex;
		private int iCompressSize;
		private int iSourceSize;
        private Boolean iUseUncompressMethod;

        public ZipBuffer(byte[] ba, int pkg = 0, Boolean useUncompressMethod = false)
        {
            iStream = new ByteBuffer(ba);
            iStream.endian = ByteBuffer.Endian.LITTLE_ENDIAN;
            iPkg = pkg;
            //iValid = -1;
            iUseUncompressMethod = useUncompressMethod;
        }

        public List<ZipEntry> Entries
        {
            get
            {
                if( iEntries == null ) 
                {
                    try {
                        GetEntries();
                    }
                    catch(System.Exception)
                    {
                        Debug.LogError("getEntries 报错");    //trace(err);
                        iEntries = new List<ZipEntry>();
                    }
                }           
                return iEntries;
            }
            
			
           
        }
		
        public int CompressSize
        {
            get
            {
                return iCompressSize;
            }            
        }
		
        public int SourceSize 
        {
            get
            {
                return iSourceSize;
            }            
        }
		
        public ZipEntry GetEntryByName(string n)
        {
            if(iNameIndex == null)
                return null;
			
            if ( !iNameIndex.Contains(n) )
                return null;   
            int i = (int)iNameIndex[n];
			
            return iEntries[i];
        }
		
        public void CreateNameIndex()
        {
            if(iNameIndex != null)
                return;
			
            iNameIndex = new Dictionary<string,int>();
            int cnt = Entries.Count;
            for(int i=0;i<cnt;i++) {
                ZipEntry entry = iEntries[i];
                iNameIndex[entry.name] = i;
            }
        }

        public void GetEntries()   //private
        {
            iEntries = new List<ZipEntry>();
            iSourceSize = 0;
            iCompressSize = 0;
			
            iStream.position = iStream.length - 22;
            byte[] tmpBuf = new byte[22];
           
            //iStream.readBytes(buf, 0, 22);
            iStream.ReadBytes(ref tmpBuf, 0, 22);
            ByteBuffer buf = new ByteBuffer(tmpBuf);
            buf.endian = ByteBuffer.Endian.LITTLE_ENDIAN;
            buf.position = 10;
            int entryCount = buf.ReadUshort();
            buf.position = 16;
            iStream.position = (int)buf.ReadUint();
            //buf.length = 0;

			
            for(int i = 0; i < entryCount; i++) 
            {
                tmpBuf = new byte[46];
                iStream.ReadBytes( ref tmpBuf, 0, 46);                
                buf = new ByteBuffer(tmpBuf);
                buf.endian = ByteBuffer.Endian.LITTLE_ENDIAN;
                buf.position = 28;
                uint len = buf.ReadUshort();
                string name = iStream.ReadString((int)len);
                int len2 = buf.ReadUshort() + buf.ReadUshort();
                iStream.position += len2;
                if(  UtilsStr.EndsWith(name, "/") || UtilsStr.EndsWith(name, "\\"))
                    continue;
				
                //name = name.Replace() .replace(/\\/g, "/");
                name = name.Replace("\\", "/");
                ZipEntry e = new ZipEntry();
                e.name = name;
                buf.position = 10;
                e.compress = buf.ReadUshort();
                buf.position = 16;
                e.crc = buf.ReadUint();
                e.size = buf.ReadUint();
                e.sourceSize = buf.ReadUint();
                iCompressSize += (int)e.size;
                iSourceSize += (int)e.sourceSize;
                buf.position = 42;
                e.offset = buf.ReadUint() + 30 + len;
                e.pkg = iPkg;
				
                iEntries.Add(e);    // .push(e);
            }
        }
		
        public byte[] GetEntryData(ZipEntry entry) 
        {
            byte[] ba = null;
            if (entry == null || entry.size == 0)
                return ba;

            ba = new byte[entry.size];
            iStream.position = (int)entry.offset;
            iStream.ReadBytes(ref ba, 0, (int)entry.size);
            if( entry.compress != 0) 
            {
                //ba = ZipHelper.Inflate(ba);
                ba = DeCompress(ba);
            }
            return ba;
        }
        public static byte[] DeCompress(byte[] bytesToDecompress)
        {
            var writeData = new byte[4096];
            var outStream = new MemoryStream();

            InflaterInputStream s2 = new InflaterInputStream(new MemoryStream(bytesToDecompress), new Inflater(true));
            while (true)
            {
                int size = s2.Read(writeData, 0, writeData.Length);
                if (size > 0)
                    outStream.Write(writeData, 0, size);
                else
                    break;
            }
            outStream.Close();
            s2.Close();
            byte[] outArr = outStream.ToArray();
            return outArr;
        }
    }
}
