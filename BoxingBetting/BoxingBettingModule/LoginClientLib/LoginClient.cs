using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Timers;

namespace LoginClientLib
{
    public class LoginClient
    {
        private string Username { get; set; }
        private string Password { get; set; }

        private Timer twentyFourHourRefreshTimer { get; set; } = new Timer(1000 * 60 * 60 * 24);  //24 hours
        public string SessionToken { get; set; }
        private static string appKey = "r9pFKKDlrpQknNvB";
        private static string certsPath = @"C:\Users\jewar\source\repos\BoxingBetting\BoxingBettingModule\BoxingBettingModule\sslcerts\certs2\Jan2020.p12";

        public LoginClient(string user, string password)
        {
            Username = user;
            Password = password;
            string[] args = new string[4] { Username, appKey, certsPath, Password };

            SessionToken = GetSessionToken(args);

            twentyFourHourRefreshTimer.Elapsed += TwentyFourHourRefreshTimer_Elapsed;
            twentyFourHourRefreshTimer.Start();
        }

        private void TwentyFourHourRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SessionToken = GetSessionToken(new string[4] { Username, appKey, certsPath, Password });
            if (SessionToken != null)
            {
                twentyFourHourRefreshTimer.Stop();
                twentyFourHourRefreshTimer.Start();
            }
            else
            {
                throw new Exception("Unable to get session token. Try Logging in Again");
            }
        }

        private static string getPassword()
        {
            string pass = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write(" * ");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            while (key.Key != ConsoleKey.Enter);

            return pass;
        }

        public string GetSessionToken(string[] args)
        {
            if (args.Count() != 4)
            {
                Console.WriteLine("This sample takes 3 arguments: <username> <appkey> <pathtocertfile>");
                Console.WriteLine("You will then be prompted for the password for this account.");
                return null;
            }
            else
            {
                string username = args.First();
                string appKey = args.ElementAt(1);
                string certFilename = args.ElementAt(2);
                string password = args.ElementAt(3);

                AuthClient client = new AuthClient(appKey);
                try
                {
                    Models.LoginResponse resp = client.doLogin(username, password, certFilename);
                    Console.WriteLine("Response Type: " + resp.LoginStatus);
                    if (resp.LoginStatus == "SUCCESS")
                    {
                        Console.WriteLine("Obtained the session token: " + resp.SessionToken);
                        return resp.SessionToken.ToString();
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (CryptographicException e)
                {
                    Console.WriteLine("Could not load the certificate: " + e.Message);
                    return null;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("The Betfair Login endpoint returned an HTTP Error: " + e.Message);
                    return null;
                }
                catch (WebException e)
                {
                    Console.WriteLine("An error occurred whilst attempting to make the request: " + e.Message);
                    return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An Error Occurred: " + e.Message);
                    return null;
                }
            }
        }

        public string GetNewSessionToken()
        {
            AuthClient client = new AuthClient(appKey);
            try
            {
                Models.LoginResponse resp = client.doLogin(Username, Password, certsPath);
                Console.WriteLine("Response Type: " + resp.LoginStatus);
                if (resp.LoginStatus == "SUCCESS")
                {
                    Console.WriteLine("Obtained the session token: " + resp.SessionToken);
                    SessionToken = resp.SessionToken;
                    return resp.SessionToken.ToString();
                }
                else
                {
                    return null;
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Could not load the certificate: " + e.Message);
                return null;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("The Betfair Login endpoint returned an HTTP Error: " + e.Message);
                return null;
            }
            catch (WebException e)
            {
                Console.WriteLine("An error occurred whilst attempting to make the request: " + e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("An Error Occurred: " + e.Message);
                return null;
            }
        }
    }
}