using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace producerConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            SyncBuffer _printerBuffer = new SyncBuffer(5);
            Random _random = new Random();
            int nThread = 2;
            Thread[] clients = new Thread[nThread];
            Thread[] printers = new Thread[nThread];
            for(int i=0; i<nThread; i++)
            {
                clients[i] = new Thread(new Client(_printerBuffer, _random).print);
                clients[i].Name = new StringBuilder("Client").Append(i.ToString()).ToString();
                clients[i].Start();

            }
            for(int i=0; i<nThread; i++)
            {
                printers[i] = new Thread(new Printer(_printerBuffer).print);
                printers[i].Name = new StringBuilder("Printer").Append(i.ToString()).ToString();
                printers[i].Start();
            }
            for(int i=0; i<nThread; i++)
            {
                clients[i].Join();
                printers[i].Join();
            }
            Console.WriteLine("Done printing all Documents");
            //Thread.Sleep(600);

           // Console.ReadLine();
        }
    }

    class SyncBuffer
    {
        private Queue<string> _printerBuffer;
        private readonly int _maxSize = -1;
        private Object _lockObj = new object();

        public SyncBuffer(int maxSize)
        {
            this._maxSize = maxSize;
            _printerBuffer = new Queue<string>(maxSize);
        }

        public void Print(string doc)
        {
            bool lockTaken = false;
            Monitor.Enter(_lockObj,ref lockTaken);
            //{
            try
            {
                if (_printerBuffer.Count == _maxSize)
                {
                    Monitor.Wait(_lockObj);
                }
                if (_printerBuffer.Count != _maxSize)
                {
                    _printerBuffer.Enqueue(doc);
                    Console.WriteLine("Thread:{0}, added doc:{1}", Thread.CurrentThread.Name, doc);
                }
                else
                {
                    Console.WriteLine("Thread:{0}, couldn't add doc:{1}", Thread.CurrentThread.Name, doc);
                }
                Monitor.Pulse(_lockObj);
            } finally
            {
                if(lockTaken)
                {
                    Monitor.Exit(_lockObj);
                }
            }
            //}

        }

        public void Print()
        {
            //lock (_lockObj) {
            bool lockTaken = false;
            Monitor.Enter(_lockObj, ref lockTaken);
            try
            {
                if (_printerBuffer.Count == 0)
                {
                    Monitor.Wait(_lockObj);
                }
                if (_printerBuffer.Count != 0)
                {
                    Console.WriteLine("Thread:{0}, removed doc:{1}", Thread.CurrentThread.Name, _printerBuffer.Dequeue());
                }
                Monitor.Pulse(_lockObj);

            }
            finally
            {
                if(lockTaken == true)
                {
                    Monitor.Exit(_lockObj);
                }
            }
            //}

        }
    }

    class Client
    {
        private SyncBuffer _printer;
        private Random _random;
        public Client(SyncBuffer buffer, Random random)
        {
            this._printer = buffer;
            this._random = random;
        }

        public void print()
        {
            int _count = 1;
            for(int i=0; i<10; i++)
            {

                Thread.Sleep(5);
                StringBuilder doc = new StringBuilder("DocID:");
                doc.Append(Thread.CurrentThread.Name);
                doc.Append(_count.ToString());
                _printer.Print(doc.ToString());
                _count++;
            }
        }
    }

    class Printer
    {
        private SyncBuffer _printer;
        public Printer(SyncBuffer printer)
        {
            this._printer = printer;
        }
        public void print()
        {
            for(int i=0; i<10; i++) {
                Thread.Sleep(50);
                _printer.Print();
            }
        }

    }
}
