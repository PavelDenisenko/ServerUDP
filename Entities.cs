using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ServerUDP
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Tick
    {
        public ulong Number;
        public DateTime TimeStamp;
        public decimal Price;

        public override string ToString()
        {
            return $"Number:{Number} Timestamp:{this.TimeStamp} Price:{this.Price}";
        }
        public  byte[] GetBytes()
        {
            int size = Marshal.SizeOf(this);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(this, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public  byte[] RawSerialize()
        {
            int rawsize = Marshal.SizeOf(this);
            byte[] rawdata = new byte[rawsize];
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            Marshal.StructureToPtr(this, handle.AddrOfPinnedObject(), false);
            handle.Free();
            return rawdata;
        }
        public void RawDeserializeStruct(byte[] bytes, ref Tick structure)
        {
            int rawsize = Marshal.SizeOf(structure);
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            structure = (Tick)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), structure.GetType());
            handle.Free();
            
        }

        public static void GetStruct(byte[] bytes, ref Tick structure)
        {
            int sizeInBytes = Marshal.SizeOf(structure);
            IntPtr ptr = Marshal.AllocHGlobal(sizeInBytes);
            Marshal.Copy(bytes, 0, ptr, sizeInBytes);
            structure = (Tick)Marshal.PtrToStructure(ptr, structure.GetType());
            Marshal.FreeHGlobal(ptr);
        }

    }


}
