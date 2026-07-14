using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DsaThreading;

Console.WriteLine("Hello, World!");

await ThreadingDemo();


static async Task ThreadingDemo()
{
    // Lets take a look at how C# managaes Threads (OS Threads not CPU threads)
    // In C# Threads are an object - like everything else. Typically theyre managed
    // by the runtime behind the scenes. For example, when this main runs to print Hello World
    // a thread object is created to handle that work.

    Console.WriteLine($"Main runs on thread #{Environment.CurrentManagedThreadId}");

    // We can create our own threads - using the Thread class. Its constructor takes one argument
    // It takes a delegate (we can define with a lambda OR pass it some prewritten method) to eun
    // inside the thread.
    var workerThread = new Thread(() =>
    {
        Console.WriteLine($"Hello from Thread #{Environment.CurrentManagedThreadId}");
    });

    // Once we have a thread setup - we can manually start it
    Console.WriteLine($"Before Start() call, isAlive = {workerThread.IsAlive}"); // Unstarted

    workerThread.Start(); // Thread is now Running
    Console.WriteLine($"During thread delegate running, isAlive {workerThread.IsAlive}");
    workerThread.Join(); // Our thread was called from the Main functions thread
    // Calling .Join() blocks the outer/caller thread similar to an await

    Console.WriteLine($"After Join() call, isAlive = {workerThread.IsAlive}"); // Stopped

    // Parallelism vs concurrency
    // Interleaving - Below even the runtime the actual OS scheduler (the thing the kernel uses to mmap
    // OS threads to CPU threads) interleaves the threads - switches them on and off CPU threads really fast
    // according to rules that we cant influence from our program - so our threads dont really complete
    // in the same order 100% of the time. This can make our code non-deterministic - which is a problem

    // Concurrency - tasks in progress (interleaved, even on one CPU core)
    // Parallelism - tasks executing at the same time (multiple cpu cores)

    // Threads give us concurrency, true parallelism depends on the hardware (and kernel).

    var threads = new List<Thread>(); // empty list of threads

    // Lets just use a loop to create a few really fast
    for (int i = 1; i <= 5; i++)
    {
        int id = i;
        var th = new Thread(() =>
        {
            Thread.Sleep(Random.Shared.Next(5,40)); // Simulating some work
            Console.WriteLine($"Worker {id} finished on thread #{Environment.CurrentManagedThreadId}");
        });

        threads.Add(th);
        th.Start();
    }
    foreach (Thread thread in threads) thread.Join(); //join call on each thread

    // Thread Safe collections

    // Ordinary collections are not optimized or built with multiple threads in mind - they would corrupt or
    // more likely throw runtime exceptions if two thread deleagtes accessed them concurrently.
    // Thankfully there are thread safe version of common collections and methods
    var counts = new ConcurrentDictionary<int, int>();

    var threadPool = new List<Thread>(); // List for our threads

    for (int i = 1; i <= 8; i++)
    {
        int id = i;

        var th = new Thread(() =>
        {
            for (int k = 0; k < 1000; k++)
                counts.AddOrUpdate(id, 1, (_, prev) => prev + 1);
                // In the line above, AddOrUpdate takes the key, the value, and a third argument
                // a delegate to execute if the key already exists
                // _ = C# discard - indicates the key parameter is intentionally ignored because the
                // delegate wont use it
                // prev - the existing integer value currently stored for thatt key
                // prev + 1 = increment that value giving us a new key to insert
        });

        threadPool.Add(th);
        th.Start();

    }
    foreach (var th in threadPool) th.Join(); // join to block mains thread
    Console.WriteLine($"Recorded {counts.Values.Sum()} increments across {counts.Count} threads");

    // When working with Threads, its common to not manually create the threads ourselves
    // For short work items like what we did above, we can use th ThreadPool
    // The ThreadPool is just a runtime managed set of background threads that we dont have to
    // create or destroy - theyre already there we can just borrow one

    // Lets make a ConcurrentQueue for FIFO work, well just have it store ints
    var done = new ConcurrentQueue<int>();

    for (int i = 0; i < 5; i++)
    {
        int n = i;

        // Instead of creating a thread manually and starting it i can just ask for a thread from
        // the background ThreadPool and pass it some delegate or method to execute
        ThreadPool.QueueUserWorkItem(_ => done.Enqueue(n*n));
    }

    // Because we dont actually have the Threads themselves at our disposal - well 
    // do like a crude await
    while (done.Count < 5) Thread.Sleep(5); // await - but way dumber

    Console.WriteLine($"Threadpool finished. {string.Join(", ", done.OrderBy(x => x))}");

    // Task. Weve already seen Tasks. Creating Threads Starting and Joining them manually works.
    // But its very low level. You manage each thread, you cant return a value a traightforward way,
    // etc. Thankfully we have the Task Parallel library. Its like a modern layer ontop.
    ParallelSum();

    static void ParallelSum()
    {
        // Just a big int array
        int[] data = Enumerable.Range(1, 8000000).ToArray();

        // First - lets do this totally sequentially - one thread without tasks
        var sw = Stopwatch.StartNew(); // using a Stopwatch object to track execution time
        long sequential = SumRange(data, 0, data.Length);
        sw.Stop();
        Console.WriteLine($"Sequential sum = {sequential}. {sw.ElapsedTicks} ticks, 1 thread");

        // Before we parallelize this, lets play with Tasks
        Task<long> half1 = Task.Run(() => SumRange(data, 0, data.Length / 2));
        Task<long> half2 = Task.Run(() => SumRange(data, data.Length / 2, data.Length));

        long total = half1.Result + half2.Result; // Asking for the Result of a Task is blocking
        Console.WriteLine($"Two task sum: {total}");

        // Lets parallelize this with Tasks and the TPL Library
        long parallelTotal = 0;

        sw.Restart(); // restarting my stopwatch back to 0 ticks - then begin counting

        Parallel.For(0, data.Length,
            // After we give ir start and end values for the loop  - this is a For loop
            // We give it an accumulator
            () => 0L,
            // body for each loop iteration on a given thread do something
            // i is the loop index, _ discards the ParallelLoopState, local is the current
            // threads subtotal for the sum
            (i, _, local) => local + data[i],
            // LocslFinally: AFTER  athread finishes all its assigned items this is called
            // Adds the Threads local Sum (the thing that starts with a value of 0L (Long))
            // to the global parallelTotal
            local => Interlocked.Add(ref parallelTotal, local) // combine per Thread sum to the outer variable        
        );
        sw.Stop();
        Console.WriteLine($"Parallel sum = {parallelTotal}. {sw.ElapsedTicks} ticks, multi-thread");
    }

    static long SumRange(int[] a, int start, int end)
    {
        long sum = 0;
        for (int i = start; i < end; i++)
        {
            sum += a[i];
        }
        return sum;
    }

    RaceDemo(); // cretaes a race condition

    static void RaceDemo()
    {
        var bank = new Bank();
        Parallel.For(0, 100000, _ => bank.DepositUnsafe(1)); // 100k threads worth of + 1
        Console.WriteLine($"Unsafe balance = {bank.Balance} (expected 1000000)");
        // Our balance is wrong every time - and its a different wrong answer every time
        // This is the worst kind of bug. Because its not deterministic.
    }

    SafeDemo(); // fixed the race

    static void SafeDemo()
    {
        var bank = new Bank();
        Parallel.For(0, 100000, _ => bank.DepositSafe(1)); 
        Console.WriteLine($"SAFE balance = {bank.Balance} (expected 1000000)");
    }

    // Interlocked - Lock free atomic operations against one variable

    InterlockedDemo();

    static void InterlockedDemo()
    {
        long counter = 0;
        // Interlock - faster than a lock when doing single atomic operations
        // if all you need is that - use an interlock over a lock
        Parallel.For(0, 100000, _ => Interlocked.Increment(ref counter));
        Console.WriteLine($"Interlocked = {counter} (expected 100000)");
    }
    
    // Deadlocks and Starvation

    // Deadlock - if two tasks create locks on resources the other ends up needing
    // they can deadlock. In this case they never resolve - our console app
    // would be waiting forever.

    // Starvation - A thread gets blocke by another threads work - and stays alive
    // but cannot progress. Different from deadlock - because the other thread is able to resolve
    // This starved thread persists - potentially starving th ThreadPool

    CancellationDemo();

    // Rather than abruptly killing a thread or having it die via some exception
    // potentially leading to data loss - we can use a cancellation token to ASK a thread to be neded
    // and it will do so once it has the chance to exit gracefully

    static void CancellationDemo()
    {
        // Calling for a CancellationToken, having it auto-cancel after 100ms
        // Side not using: Once we exit the scope were the variable created with using
        // lives in - dispose of it.
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        CancellationToken token = cts.Token; 

        var work = Task.Run(() =>
        {
            for (long i = 0; ; i++)
            {
                token.ThrowIfCancellationRequested();
                if (i % 5000000 == 0) { /*some simulated work */}
            }
        }, token);

        try
        {
            work.Wait(); // The task is going - we want to have our code wait for it here
        } // Exception filtering - for when exceptions are thrown by other exceptions.
        catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
        {
            Console.WriteLine("Work was cancelled cooperatively");
        }
    }

    ExceptionDemo();

    static void ExceptionDemo()
    {
        // Our task start up here when we call run...
        var t = Task.Run(() => throw new InvalidOperationException("oops - but in a task"));
         
        // Counter-intuitively, an exception inside a task DOESNT crash on the spot
        // Wed imagine that line 279 is where the exception is thrown. Its actually
        // thrown during the t.Wait() below
        try
        {
            t.Wait();
        }
        catch (AggregateException ex)
        {   // Agreggate exceptions themselves are kind of weird
            // One task can have several faults - so they get thrown inside an AgreggateException 
            Console.WriteLine($"Caught: {ex.InnerException!.Message}");
        }
    }

    // Async / await - related to but not the same as a thread
    await AsyncDemo();

    static async Task AsyncDemo()
    {
        Console.WriteLine($"Before await on thread #{Environment.CurrentManagedThreadId}");
        await Task.Delay(50);
        Console.WriteLine($"After await on thread #{Environment.CurrentManagedThreadId}");
    }



}

