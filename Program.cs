using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

class Program
{
    //loading the proxy file , it should be placed in the same folder and with the name of proxies.txt
    //paste your proxies in this text file.
    static string[] proxies = File.ReadAllLines("proxies.txt");
    
    // target url , proxies are tested on this website
    static string target;
    
    // just a variable for selecting the proxy type , which user is going to provide
    static string type;
    static int goods = 0;
    
    // just some variables for maintaining the algorithm
    static int total = proxies.Length;
    static int remaining = total;
    
    //just some variable for showing the elapsed time
    static bool running = true;
    static int second = 0;
    static int TimeoutInSeconds;
    
    //just an object for locking the I/O , prevents multithreading on the text file which could result in an error 
    private static readonly object fileLock = new object();
    
    static async Task Main(string[] args)
    {
        // the displayed message when you open the app
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Loaded {0} Proxies", total);
        Console.WriteLine("Proxy Type ? (http - https - socks4 - socks5)");
        
        // the type of the proxy which user provides
        type = Console.ReadLine();
        
        // proxies are going to be tested on this website
        Console.WriteLine("What is the target ?");
        target = Console.ReadLine();
        
        Console.WriteLine("How many Threads ? ");
        int Totalthread = Convert.ToInt32(Console.ReadLine());
        
        //timeout of proxies , based on user input
        Console.Write("Timeout in seconds: \n");
        TimeoutInSeconds = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine();
        
        // some variable for maintaining the algorithm ( wish i just used task factory in combination with semaphore)
        int EachThread = total / Totalthread; // each thread checks this many proxies
        int remainder = total % Totalthread; // remainder if total is not evenly divisible by totalthread
        
        // adding each thread in a list , so we can use Task.WhenAll on the list later on.
        List<Task> tasks = new List<Task>();
        
        // just a thread for displaying the timer in the title
        Thread thead = new Thread(() =>
        {
            while (running)
            {
                second++;
                int minute = second / 60;
                int remainingseconds = second % 60;
                Console.Title = ($"total : {total} Remaning {remaining} : Goods : {goods} ----- {minute} minute and {remainingseconds} seconds elapsed");
                //Console.Title = $"{minute} minute and {remainingseconds} seconds elapsed";
                Thread.Sleep(1000);
            }
        });
        thead.Start();
        
        //i recommend not going over 100 threads , you do you
        for (int num = 0; num < Totalthread; num++)
        {
            int start = num * EachThread;
            int end = start + EachThread;
            
            // distribute the remainder among the first few threads
            if (num < remainder)
            {
                end++; // threads are going to check extra proxies , if total is not divisible by 50
            }
            
            // fixing out of index errors
            end = Math.Min(end, total);
            
            Task task = Task.Run(async () => await Check(start, end, num + 1));
            tasks.Add(task);
        }
        
        // wait for all the threads to do their job
        await Task.WhenAll(tasks);
        running = false;
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        if (second > 61)
        {
            Console.WriteLine("\n{0} Proxies Checked in {1} minute and {2} second", total - remaining, second / 60, second % 60);
            Console.WriteLine("{0} Proxies are Working", goods);
        }
        else
        {
            Console.WriteLine("\n{0} Proxies Checked in {1} second", total - remaining, second);
            Console.WriteLine("{0} Proxies are Working", goods);
        }
        
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n Done , Good Luck!");
        Console.ReadLine();
    }
    
    // each thread logic and the actual checking 
    static async Task Check(int min, int max, int id)
    {
        while (min < max)
        {
            //set the raw proxy and its settings
            
            try
            {
                var proxy = new WebProxy
                {
                    Address = new Uri($"{type}://{proxies[min]}"),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false,
                };
                
                
                
                //implement the WebProxy object into httpclienthandler
                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = proxy,
                };
                
                // disable ssl verification
                httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                
                
                // this is needed for skipping bad proxies , timeout wont work without this 
                // cancellationTokenSource generates cancel Tokens , also has built in CancelAfter method
                // which helps for speicfying a timer for canceling the Task
                
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(TimeoutInSeconds));
                    
                    HttpClient http = new HttpClient(httpClientHandler, true);
                    
                    // skip based on the timeout
                    HttpResponseMessage response = await http.GetAsync(target, cancellationTokenSource.Token);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        
                            File.AppendAllText("details.txt", $"good{proxies[min]}\n");
                            File.AppendAllText("goods.txt", $"{proxies[min]}\n");
                            goods++;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(proxies[min]);
                        
                    }
                    
                    else
                    {
                        lock (fileLock)
                        {
                            File.AppendAllText("details.txt", $"Bad : {proxies[min]} \n");
                        }
                    }
                }
            }
            //when proxy must be skipped
            // thrown when the proxy has took more than the specified timeout.
            catch (OperationCanceledException)
            {
                lock (fileLock)
                {
                    File.AppendAllText("details.txt", $"Timedout : {proxies[min]} \n");
                }
            }
            //catching random errors
            catch (Exception ex)
            {
                lock (fileLock)
                {
                    File.AppendAllText("details.txt", $"Error : {proxies[min]}: {ex.Message} \n");
                }
            }
            
            remaining--;
            min++;
        }
    }
}
