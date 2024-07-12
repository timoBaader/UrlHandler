using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

class Program
{
    private static Dictionary<int, string> browserProfiles = new Dictionary<int, string>();
    private static int timeout;
    private static int defaultBrowser;

    static void Main(string[] args)
    {
        // URL from command line arguments
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: PROGRAMM.EXE <url>");
            return;
        }

        string url = args[0];

        // Load configuration
        LoadConfiguration();

        // Show browser selection
        ShowBrowserSelection(url);
    }

    static void LoadConfiguration()
    {
        string[] lines = File.ReadAllLines("PROGRAMM.INI");
        foreach (var line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
            {
                continue;
            }

            if (trimmedLine.StartsWith("TimeOut"))
            {
                timeout = int.Parse(trimmedLine.Split('=')[1].Trim());
            }
            else if (trimmedLine.StartsWith("DefaultBrowser"))
            {
                defaultBrowser = int.Parse(trimmedLine.Split('=')[1].Trim());
            }
            else if (char.IsDigit(trimmedLine[0]))
            {
                int key = int.Parse(trimmedLine.Split('=')[0].Trim());
                string command = trimmedLine.Split('=')[1].Trim().Replace("\"", "");
                browserProfiles[key] = command;
            }
        }
    }

    static void ShowBrowserSelection(string url)
    {
        Console.WriteLine("Select a browser to open the URL:");
        foreach (var profile in browserProfiles)
        {
            Console.WriteLine($"{profile.Key}: {profile.Value}");
        }

        Console.WriteLine($"Default browser will be used in {timeout} seconds.");

        int selectedBrowser = -1;
        Timer timer = new Timer(
            (state) =>
            {
                if (selectedBrowser == -1)
                {
                    LaunchBrowser(defaultBrowser, url);
                }
            },
            null,
            timeout * 1000,
            Timeout.Infinite
        );

        if (
            int.TryParse(Console.ReadLine(), out selectedBrowser)
            && browserProfiles.ContainsKey(selectedBrowser)
        )
        {
            timer.Dispose();
            LaunchBrowser(selectedBrowser, url);
        }
        else
        {
            Console.WriteLine("Invalid selection. Using default browser.");
            LaunchBrowser(defaultBrowser, url);
        }
    }

    static void LaunchBrowser(int browserKey, string url)
    {
        if (browserProfiles.TryGetValue(browserKey, out string command))
        {
            string commandWithUrl = $"{command} \"{url}\"";
            Console.WriteLine($"Launching: {commandWithUrl}");
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = url,
                    UseShellExecute = true
                }
            );
        }
        else
        {
            Console.WriteLine("Browser profile not found.");
        }
    }
}
