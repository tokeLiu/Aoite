﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Aoite.LevelDB
{
    internal class Comparator : LevelDBHandle
    {
        IntPtr NativeName;
        Comparison<NativeArray> Comparison;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void DestructorSignature(IntPtr GCHandleThis);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int CompareSignature(IntPtr GCHandleThis, IntPtr data1, IntPtr size1, IntPtr data2, IntPtr size2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate IntPtr GetNameSignature(IntPtr GCHandleThis);

        public Comparator(string name, IComparer<NativeArray> comparer) : this(name, (a, b) => comparer.Compare(a, b)) { }

        public Comparator(string name, Comparison<NativeArray> comparison)
        {
            GCHandle selfHandle = default(GCHandle);
            try
            {
                var utf = GA.UTF8.GetBytes(name);
                NativeName = Marshal.AllocHGlobal(utf.Length + 1);
                Marshal.Copy(utf, 0, NativeName, utf.Length);

                Comparison = comparison;

                selfHandle = GCHandle.Alloc(this);

                _handle = LevelDBInterop.leveldb_comparator_create(
                    GCHandle.ToIntPtr(selfHandle),
                    Marshal.GetFunctionPointerForDelegate(Destructor),
                    Marshal.GetFunctionPointerForDelegate(Compare),
                    Marshal.GetFunctionPointerForDelegate(GetName));
                if (_handle == IntPtr.Zero)
                    throw new ApplicationException("Failed to initialize LevelDB comparator");
            }
            catch
            {
                if (selfHandle != default(GCHandle))
                    selfHandle.Free();
                if (NativeName != IntPtr.Zero)
                    Marshal.FreeHGlobal(NativeName);
                throw;
            }
        }

        internal override void DestroyUnmanaged()
        {
            if (_handle != IntPtr.Zero)
                LevelDBInterop.leveldb_comparator_destroy(_handle);
            if (NativeName != IntPtr.Zero)
                Marshal.FreeHGlobal(NativeName);
        }

        static DestructorSignature Destructor = (selfHandle) =>
        {
            var gcHandle = GCHandle.FromIntPtr(selfHandle);
            var self = (Comparator)gcHandle.Target;
            self.Dispose();
            gcHandle.Free();
        };

        static CompareSignature Compare = (selfHandle, data1, size1, data2, size2) =>
        {
            var self = (Comparator)GCHandle.FromIntPtr(selfHandle).Target;
            return self.ExecuteComparison(data1, size1, data2, size2);
        };

        static GetNameSignature GetName = (selfHandle) =>
        {
            var self = (Comparator)GCHandle.FromIntPtr(selfHandle).Target;
            return self.NativeName;
        };

        unsafe int ExecuteComparison(IntPtr data1, IntPtr size1, IntPtr data2, IntPtr size2)
        {
            return Comparison(
                new NativeArray() { Data = (byte*)data1, Length = (long)size1 },
                new NativeArray() { Data = (byte*)data2, Length = (long)size2 });
        }

        public unsafe struct NativeArray
        {
            internal byte* Data;
            internal long Length;
            public byte[] ToArray()
            {
                byte[] bytes = new byte[Length];
                for(int i = 0; i < Length; i++)
                {
                    bytes[i] = Data[i];
                }
                return bytes;
            }
        }
    }
}
