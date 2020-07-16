using ProtoBuf;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace LOU
{
    public class MemoryMap
    {
        private String MapName = "";
        public Int32 MapSize { get; set; } = 0;
        private String MutexName = "";
        private MemoryMappedFile m_memMap = null;

        public int LastMemoryOccupation { get; set; } = 0;
        public double LastMemoryOccupationPerc { get => Math.Round((double)LastMemoryOccupation / (double)MapSize * 100D, 2); }

        public MemoryMap(string MapName, Int32 MapSize, string MutexName)
        {
            this.MapName = MapName;
            this.MapSize = MapSize;
            this.MutexName = MutexName;
        }

        public bool OpenExisting()
        {
            using (var mutex = new Mutex(false, this.MutexName))
            {
                var mutexAcquired = false;
                try
                {
                    mutexAcquired = mutex.WaitOne(1000);
                }
                catch (AbandonedMutexException)
                {
                    mutexAcquired = true;
                }
                if (!mutexAcquired)
                {
                    throw new Exception("Mutex " + this.MutexName + " not acquired!");
                }
                try
                {
                    //Try to open an existing memory map
                    try
                    {
                        m_memMap = MemoryMappedFile.OpenExisting(this.MapName);
                        return true;
                    }
                    catch (FileNotFoundException ex)
                    {
                        m_memMap = null;
                        return false;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Cannot open memory map " + this.MapName + ": " + ex.ToString());
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        public bool OpenOrCreate()
        {
            using (var mutex = new Mutex(false, this.MutexName))
            {
                var mutexAcquired = false;
                try
                {
                    mutexAcquired = mutex.WaitOne(1000);
                }
                catch (AbandonedMutexException)
                {
                    mutexAcquired = true;
                }
                if (!mutexAcquired)
                {
                    throw new Exception("Mutex " + this.MutexName + " not acquired!");
                }
                try
                {
                    //Try to open an existing memory map
                    try
                    {
                        m_memMap = MemoryMappedFile.OpenExisting(this.MapName);
                    }
                    catch (FileNotFoundException ex)
                    {
                        m_memMap = null;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Cannot open memory map " + this.MapName + ": " + ex.ToString());
                    }

                    //Create the memory map if needed
                    try
                    {
                        if (m_memMap == null)
                            m_memMap = MemoryMappedFile.CreateNew(this.MapName, this.MapSize);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Cannot create memory map " + this.MapName + ": " + ex.ToString());
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        public void ReadMemoryMap<T>(out T Object)
        {
            if (m_memMap == null)
            {
                Object = default(T);
                this.LastMemoryOccupation = -1;
                return;
            }

            using (var mutex = new Mutex(false, this.MutexName))
            {
                var mutexAcquired = false;
                try
                {
                    mutexAcquired = mutex.WaitOne(1000);
                }
                catch (AbandonedMutexException)
                {
                    mutexAcquired = true;
                }
                if (!mutexAcquired)
                {
                    this.LastMemoryOccupation = -1;
                    throw new Exception("Mutex " + this.MutexName + " not acquired!");
                }
                try
                {
                    //Create the view stream
                    using (var memStream = m_memMap.CreateViewStream())
                    {
                        //Seek to start of memory (i.e. start of object)
                        memStream.Seek(0, SeekOrigin.Begin);

                        //Try to read length first
                        Serializer.TryReadLengthPrefix(memStream, PrefixStyle.Fixed32, out int length);

                        //Seek to start of memory (i.e. start of object)
                        memStream.Seek(0, SeekOrigin.Begin);

                        //Then try to read
                        try
                        {
                            Object = Serializer.DeserializeWithLengthPrefix<T>(memStream, PrefixStyle.Fixed32);
                        }
                        catch (Exception ex)
                        {
                            this.LastMemoryOccupation = -1;
                            throw new Exception("Cannot read from memory map " + this.MapName + " object of type " + typeof(T).ToString() + ": " + ex.ToString());
                        }

                        this.LastMemoryOccupation = 4 + length;
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        public void ReadMemoryMap<T1, T2>(out T1 Object1, out T2 Object2)
        {
            if (m_memMap == null)
            {
                Object1 = default(T1);
                Object2 = default(T2);
                this.LastMemoryOccupation = -1;
                return;
            }

            using (var mutex = new Mutex(false, this.MutexName))
            {
                var mutexAcquired = false;
                try
                {
                    mutexAcquired = mutex.WaitOne(1000);
                }
                catch (AbandonedMutexException)
                {
                    mutexAcquired = true;
                }
                if (!mutexAcquired)
                {
                    this.LastMemoryOccupation = -1;
                    throw new Exception("Mutex " + this.MutexName + " not acquired!");
                }
                try
                {
                    //Create the view stream
                    using (var memStream = m_memMap.CreateViewStream())
                    {
                        //Seek to start of memory (i.e. start of first object)
                        memStream.Seek(0, SeekOrigin.Begin);

                        //Try to read first object's length
                        Serializer.TryReadLengthPrefix(memStream, PrefixStyle.Fixed32, out int length1);

                        //Seek to start of memory (i.e. start of first object)
                        memStream.Seek(0, SeekOrigin.Begin);

                        //Then try to read first object
                        try
                        {
                            Object1 = Serializer.DeserializeWithLengthPrefix<T1>(memStream, PrefixStyle.Fixed32);
                        }
                        catch (Exception ex)
                        {
                            this.LastMemoryOccupation = -1;
                            throw new Exception("Cannot read from memory map " + this.MapName + " object of type " + typeof(T1).ToString() + ": " + ex.ToString());
                        }

                        // Store position of second object's length
                        long Position = memStream.Position;

                        //Try to read second length
                        Serializer.TryReadLengthPrefix(memStream, PrefixStyle.Fixed32, out int length2);

                        //Seek to beginning of second object
                        memStream.Seek(Position, SeekOrigin.Begin);

                        try
                        {
                            Object2 = Serializer.DeserializeWithLengthPrefix<T2>(memStream, PrefixStyle.Fixed32);
                        }
                        catch (Exception ex)
                        {
                            this.LastMemoryOccupation = -1;
                            throw new Exception("Cannot read from memory map " + this.MapName + " object of type " + typeof(T2).ToString() + ": " + ex.ToString());
                        }

                        this.LastMemoryOccupation = 4 + length1 + 4 + length2;
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        public void WriteMemoryMap<T>(T Object)
        {
            if (m_memMap == null)
                return;

            using (var mutex = new Mutex(false, this.MutexName))
            {
                var mutexAcquired = false;
                try
                {
                    mutexAcquired = mutex.WaitOne(1000);
                }
                catch (AbandonedMutexException)
                {
                    mutexAcquired = true;
                }
                if (!mutexAcquired)
                {
                    throw new Exception("Mutex " + this.MutexName + " not acquired!");
                }
                try
                {
                    //Create the view stream
                    using (var memStream = m_memMap.CreateViewStream())
                    {
                        memStream.Seek(0, SeekOrigin.Begin);

                        //And try to write
                        try
                        {
                            //Write the byte array to memory
                            Serializer.SerializeWithLengthPrefix<T>(memStream, Object, PrefixStyle.Fixed32);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Cannot write to memory map " + this.MapName + " object of type " + typeof(T).ToString() + ": " + ex.ToString());
                        }
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        public void WriteMemoryMap<T1, T2>(T1 Object1, T2 Object2)
        {
            if (m_memMap == null)
                return;

            using (var mutex = new Mutex(false, this.MutexName))
            {
                var mutexAcquired = false;
                try
                {
                    mutexAcquired = mutex.WaitOne(1000);
                }
                catch (AbandonedMutexException)
                {
                    mutexAcquired = true;
                }
                if (!mutexAcquired)
                {
                    throw new Exception("Mutex " + this.MutexName + " not acquired!");
                }
                try
                {
                    //Create the view stream
                    using (var memStream = m_memMap.CreateViewStream())
                    {
                        memStream.Seek(0, SeekOrigin.Begin);

                        //And try to write
                        try
                        {
                            //Write the byte array to memory
                            Serializer.SerializeWithLengthPrefix<T1>(memStream, Object1, PrefixStyle.Fixed32);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Cannot write to memory map " + this.MapName + " object of type " + typeof(T1).ToString() + ": " + ex.ToString());
                        }
                        try
                        {
                            //Write the byte array to memory
                            Serializer.SerializeWithLengthPrefix<T2>(memStream, Object2, PrefixStyle.Fixed32);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Cannot write to memory map " + this.MapName + " object of type " + typeof(T2).ToString() + ": " + ex.ToString());
                        }
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }
}
