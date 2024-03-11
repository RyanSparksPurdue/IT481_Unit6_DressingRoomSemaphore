using System;
using System.Collections.Generic;
using System.Threading;






    class DressingRooms
    {
        private Semaphore semaphore;

        public DressingRooms(int numRooms = 3)
        {
            semaphore = new Semaphore(numRooms, numRooms);
        }

        public void RequestRoom()
        {
            semaphore.WaitOne();
        }

        public void ReleaseRoom()
        {
            semaphore.Release();
        }
    }

    class Customer
    {
        public int NumItems { get; }

        private int customerId;
        private DressingRooms dressingRooms;
        private int totalTryOnTime = 0;

        public Customer(int id, DressingRooms rooms, int maxItems = 6, int numberOfItems = 0)
        {
            customerId = id;
            NumItems = numberOfItems == 0 ? new Random().Next(1, maxItems + 1) : Math.Clamp(numberOfItems, 1, 20);
            dressingRooms = rooms;
        }

        public void TryOnClothes()
        {
            Console.WriteLine($"\nCustomer {customerId} has selected {NumItems} items and is approaching the dressing room.");
            dressingRooms.RequestRoom();
            Console.WriteLine($"\nCustomer {customerId} enters the dressing room.");
            for (int i = 1; i <= NumItems; i++)
            {
                int tryOnTimeSeconds = new Random().Next(1, 4);
                totalTryOnTime += tryOnTimeSeconds;
                Thread.Sleep(tryOnTimeSeconds * 1000);
                Console.WriteLine($"\nCustomer {customerId} tries on item {i} for {tryOnTimeSeconds} minutes.");
            }
            dressingRooms.ReleaseRoom();
            Console.WriteLine($"\nCustomer {customerId} leaves the dressing room.");
        }

        public int GetTotalTryOnTime()
        {
            return totalTryOnTime;
        }
    }

    class Scenario
    {
        private List<Customer> customers = new List<Customer>();
        private DateTime startTime;
        private DateTime endTime;
        private int totalTryOnTime = 0;
        private bool loadTesting;
        public Scenario(int numCustomers, bool loadTesting = false)
        {
            this.loadTesting = loadTesting;
            DressingRooms dressingRooms = new DressingRooms();
            for (int i = 1; i <= numCustomers; i++)
            {
                int numberOfItems = loadTesting ? new Random().Next(1, 21) : 0;
                customers.Add(new Customer(i, dressingRooms, numberOfItems: numberOfItems));
            }
        }

        public void RunScenario()
        {
            startTime = DateTime.Now;
            Console.WriteLine($"\n*******************************Starting Scenario with {customers.Count} customers********************************\n");

            if (loadTesting)
            {
                Console.WriteLine("**Load testing enabled. Default store item limit is ignored.**\n");
            }

            List<Thread> threads = new List<Thread>();

            foreach (Customer customer in customers)
            {
                Thread thread = new Thread(customer.TryOnClothes);
                threads.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            endTime = DateTime.Now;

            foreach (Customer customer in customers)
            {
                totalTryOnTime += customer.GetTotalTryOnTime();
            }
            Console.WriteLine($"\n\n**************************************End Scenario**************************************");
            PrintResults();
        }

        private void PrintResults()
        {
            double totalTryOnTimeMinutes = totalTryOnTime / 60.0;
            Console.WriteLine($"\n\nScenario/Simulation took {totalTryOnTimeMinutes:N2} real-world minutes.");
            int totalItems = 0;
            foreach (Customer customer in customers)
            {
                totalItems += customer.NumItems;
            }
            Console.WriteLine($"\nTotal customers: {customers.Count}");
            Console.WriteLine($"\nTotal items: {totalItems}");
            Console.WriteLine($"\nAverage number of items per customer: {(double)totalItems / customers.Count}");
            double averageUsageTimeMinutes = totalTryOnTimeMinutes / customers.Count;
            Console.WriteLine($"\nAverage simulated usage time of the room per customer: {averageUsageTimeMinutes * 60:N2} minutes");
            Console.WriteLine($"\nTotal simulated try-on time for all customers: {totalTryOnTimeMinutes * 60:N2} minutes\n\n");


        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Scenario scenario1 = new Scenario(numCustomers: 10, loadTesting: false);
            scenario1.RunScenario();

            Scenario scenario2 = new Scenario(numCustomers: 20, loadTesting: false);
            scenario2.RunScenario();

            Scenario scenario3 = new Scenario(numCustomers: 20, loadTesting: true);
            scenario3.RunScenario();

            Console.ReadLine();
        }
    }


