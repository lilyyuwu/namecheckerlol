using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NickChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"
Choose the Checker mode.
(1) All -> Will show all nicks, available for nick change, available for new account creation, unavailable and banned accounts.
(2) Creation -> Will show only the nicks available for new account creations.
(3) Available -> Will show only the nicks available for new account creations, nick changes, and possibly banned accounts.
(4) Close -> Will show only the nicks that are close to becoming available (customizable number of days).
");
            int mode = int.Parse(Console.ReadLine());
            Console.WriteLine();

            using (StreamWriter g = new StreamWriter("output.txt"))
            {
                g.WriteLine("STARTING--------------------------------------------");
            }

            int daysCustom = 0;
            int daysbanned = -150;

            if (mode == 4)
            {
                Console.WriteLine("What is the minimum number of days a nick must have to be available?\n$ ");
                daysCustom = int.Parse(Console.ReadLine());
                Console.WriteLine();
            }

            if (mode > 4)
            {
                throw new Exception("Modes only go up to 4.");
            }

            Console.WriteLine("Starting...\n");

            string[] usernames = File.ReadAllLines("usernames.txt", Encoding.UTF8);

            foreach (string username in usernames)
            {
                if (username.Length < 3)
                {
                    Console.WriteLine($"{username} has less than 3 letters (use space in these cases).");
                    continue;
                }
                else if (username.Length > 16)
                {
                    Console.WriteLine($"{username} has more than 16 letters.");
                    continue;
                }

                string data = "";
                int daysleft = 0;
                int status = 0;
                int lvl = 0;

                while (true)
                {
                    string url = $"https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-name/{username}?api_key={{APIKEYHERE}}";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Timeout = 1000;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode != HttpStatusCode.TooManyRequests)
                    {
                        Stream dataStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        string responseFromServer = reader.ReadToEnd();
                        reader.Close();
                        response.Close();

                        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);

                        if (result.revisionDate > 0)
                        {
                            if (result.summonerLevel < 5)
                            {
                                long epoch30m = 15778458;
                                lvl = -7;
                            }
                            else
                            {
                                lvl = result.summonerLevel;
                                long epoch = 15778458 + ((lvl - 5) * 2629743);
                                long epoch30m = Math.Min(epoch, 78892290);
                                long lastgame = result.revisionDate;
                                long availableday = lastgame / 1000 + epoch30m;
                                long currenttime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                long delta = availableday - currenttime;
                                daysleft = (int)(delta / (24 * 60 * 60));
                                data = DateTimeOffset.FromUnixTimeSeconds(availableday).ToString("ddd, dd MMM yyyy HH:mm:ss");
                            }
                        }

                        status = (int)response.StatusCode;

                        break;
                    }
                }

                switch (mode)
                {
                    case 1:
                        if (daysleft == 0)
                        {
                            Console.WriteLine($"{username} is available for account creation.");
                            using (StreamWriter z = new StreamWriter("output.txt", true))
                            {
                                z.WriteLine($"{username} is available for account creation.");
                            }
                        }
                        else if (daysleft < daysbanned)
                        {
                            Console.WriteLine($"{username} may be available. If it results as unavailable, it is a banned account and the nick will not expire.");
                            using (StreamWriter z = new StreamWriter("output.txt", true))
                            {
                                z.WriteLine($"{username} may be available. If it results as unavailable, it is a banned account and the nick will not expire.");
                            }
                        }
                        else if (daysleft < 0)
                        {
                            Console.WriteLine($"{username} is available for nick change.");
                            using (StreamWriter z = new StreamWriter("output.txt", true))
                            {
                                z.WriteLine($"{username} is available for nick change.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"{username}   will be available on: ");
                            using (StreamWriter z = new StreamWriter("output.txt", true))
                            {
                                z.WriteLine($"{username} will be available on:{data} |  {daysleft} days");
                            }
                        }
                        break;
                    case 2:
                        if (daysleft == 0)
                        {
                            Console.WriteLine($"{username} is available for account creation.");
                            using (StreamWriter z = new StreamWriter("output.txt", true))
                            {
                                z.WriteLine($"{username} is available for account creation.");
                            }
                        }
                        break;
                    case 3:
                        if (daysleft == 0)
                        {
                            Console.WriteLine($"{username} is available for account creation.");
                            using (StreamWriter z = new StreamWriter("output.txt", true))
                            {
                                z.WriteLine($"{username} is available for account creation.");
                            }
                        }
                        else if (daysleft < daysbanned)
                        {
                            Console.WriteLine($"{username} may be available. If it results as unavailable, it is a banned account and the nick will not expire.");
                            using (StreamWriter z = new StreamWriter("output.txt", true))
                            {
                                z.WriteLine($"{username} may be available. If it results as unavailable, it is a banned account and the nick will not expire.");
                            }
                        }
                        else if (daysleft < 0)
                        {
                            Console.WriteLine($"{username} is available for nick change.");

using (StreamWriter z = new StreamWriter("output.txt", true))
                            {
                                z.WriteLine($"{username} is available for nick change.");
                            }
                        }
                        break;
                    case 4:
                        if (lvl == -8)
                        {
                            break;
                        }
                        else if (daysCustom >= daysleft && daysleft >= -2)
                        {
                            Console.WriteLine($"{username} will be available on: {data} |  {daysleft} days");
                            using (StreamWriter z = new StreamWriter("output.txt", true))
                            {
                                z.WriteLine($"{username} will be available on:{data} |  {daysleft} days");
                            }
                        }
                        break;
                    default:
                        throw new Exception("Invalid mode selected.");
                }
            }
            using (StreamWriter g = new StreamWriter("output.txt", true))
            {
                g.WriteLine("FINISHED--------------------------------------------.");
            }
            Console.WriteLine("\nFinished.");
        }
    }
}


