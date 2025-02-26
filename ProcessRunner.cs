using Svn2GitConsole.Interfaces;
using System.Diagnostics;
using System.Text;

using Console = System.Console;
using Environment = System.Environment;

namespace Svn2GitConsole
{
    public class ProcessRunner : IProcessRunner
    {
        private readonly string? _workingDirectory;

        private readonly int _timeoutSeconds;

        private readonly int _maxOutputLines;

        public ProcessRunner(string? workingDirectory=null, int timeoutSeconds = 300,
                             int maxOutputLines = 1000)
        {
            _workingDirectory = workingDirectory;
            _timeoutSeconds = timeoutSeconds;
            _maxOutputLines = maxOutputLines;
        }

        public string Run(string command, bool exitOnError = true, bool printOutput = false)
        {
            var startInfo = new ProcessStartInfo
            {
                //FileName = "bash", // unix
                FileName = "cmd.exe",
                Arguments = $"/c \"{command}\"",
                WorkingDirectory = string.IsNullOrWhiteSpace(_workingDirectory)
                                       ? Directory.GetCurrentDirectory()
                                       : _workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            var outputLines = new List<string>();

            using (var process = new Process { StartInfo = startInfo })
            {
                try
                {
                    process.Start();

                    // Wait for the process to actually start
                    while (process.Id == 0)
                    {
                        Thread.Sleep(10);
                    }

                    var outputTask = Task.Run(
                        () =>
                            {
                                int lineNumber = 1;
                                while (!process.StandardOutput.EndOfStream)
                                {
                                    string line = process.StandardOutput.ReadLine();
                                    if (printOutput)
                                    {
                                        Console.WriteLine($"{lineNumber,4}: {line}");
                                    }
                                    if (outputLines.Count < _maxOutputLines)
                                    {
                                        outputLines.Add(line);
                                    }

                                    lineNumber++;
                                }
                            });

                    var errorTask = Task.Run(
                        () =>
                            {
                                int lineNumber = 1;
                                while (!process.StandardError.EndOfStream)
                                {
                                    string errorLine = process.StandardError.ReadLine();
                                    Console.Error.WriteLine($"E{lineNumber,3}: {errorLine}");
                                    lineNumber++;
                                }
                            });

                    Task.WaitAll(outputTask, errorTask);

                    if (!process.WaitForExit(_timeoutSeconds * 1000))
                    {
                        process.Kill();
                        throw new TimeoutException($"Git command timed out after {_timeoutSeconds} seconds");
                    }

                    if (exitOnError && process.ExitCode != 0)
                    {
                        throw new Exception($"Git command failed with exit code {process.ExitCode}.");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error executing command: {command}\n{ex.Message}");
                    if (exitOnError)
                    {
                        throw;
                    }
                }
            }
            return string.Join(Environment.NewLine, outputLines);
        }
        //private static string RunGitCommand(
        //string arguments,
        //string workingDirectory = "",
        //int timeoutSeconds = 300,
        //int maxOutputLines = 1000) // Limit the number of lines to return
        //{
        //    Console.WriteLine($"Running Git command: git {arguments}");
        //    var processInfo = new ProcessStartInfo("git", arguments)
        //    {
        //        UseShellExecute = false,
        //        CreateNoWindow = true,
        //        RedirectStandardOutput = true,
        //        RedirectStandardError = true,
        //        WorkingDirectory = string.IsNullOrWhiteSpace(workingDirectory)
        //                                                     ? Directory.GetCurrentDirectory()
        //                                                     : workingDirectory
        //    };

        //    using (var process = new Process { StartInfo = processInfo })
        //    {
        //        process.Start();

        //        // Wait for the process to actually start
        //        while (process.Id == 0)
        //        {
        //            Thread.Sleep(10);
        //        }

        //        var outputLines = new List<string>();
        //        var outputTask = Task.Run(
        //            () =>
        //            {
        //                int lineNumber = 1;
        //                while (!process.StandardOutput.EndOfStream)
        //                {
        //                    string line = process.StandardOutput.ReadLine();
        //                    Console.WriteLine($"{lineNumber,4}: {line}");
        //                    if (outputLines.Count < maxOutputLines)
        //                    {
        //                        outputLines.Add(line);
        //                    }

        //                    lineNumber++;
        //                }
        //            });

        //        var errorTask = Task.Run(
        //            () =>
        //            {
        //                int lineNumber = 1;
        //                while (!process.StandardError.EndOfStream)
        //                {
        //                    string errorLine = process.StandardError.ReadLine();
        //                    Console.WriteLine($"E{lineNumber,3}: {errorLine}");
        //                    lineNumber++;
        //                }
        //            });

        //        Task.WaitAll(outputTask, errorTask);

        //        if (!process.WaitForExit(timeoutSeconds * 1000))
        //        {
        //            process.Kill();
        //            throw new TimeoutException($"Git command timed out after {timeoutSeconds} seconds");
        //        }

        //        if (process.ExitCode != 0)
        //        {
        //            throw new Exception($"Git command failed with exit code {process.ExitCode}.");
        //        }

        //        return string.Join(Environment.NewLine, outputLines);
        //    }
        //}
    }
}
