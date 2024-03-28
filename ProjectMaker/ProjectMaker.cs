using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;

internal class ProjectMaker
{
    static public string IntermediateIncludesDirectory = "Intermediate\\Includes\\";

    static void Main(string[] args)
    {
        string command;

        if (args.Length == 0)
        {
            command = "-setup";
        }
        else
        {
            command = args[0];
        }

        switch (command.ToLower())
        {
            case "-setup":

                string basePath;

                if (args.Length > 1 && Directory.Exists(args[1]))
                {
                    basePath = args[1];
                }
                else
                {
                    Console.WriteLine("Invalid or missing directory argument. Using current directory instead.");
                    basePath = Directory.GetCurrentDirectory();
                }

                ProjectMake(basePath);

                Console.WriteLine("끝");
                Console.ReadKey();
                break;
            case "-prebuild":
                PreBuild();
                break;
            default:
                Console.WriteLine("잘못된 인수입니다. -Setup 또는 -PreBuild를 사용하세요.");
                break;
        }


    }

    static void ProjectMake(string basePath)
    {
        List<VSProject> projects = new List<VSProject>();
        ModuleScanner scanner = new ModuleScanner();
        scanner.ScanModules(basePath);

        foreach (var module in scanner.Modules)
        {
            VSProject project = new VSProject();
            projects.Add(VSProjectFileCreater.CreateProject(module));
        }
        VSSolution solution = new VSSolution(Path.Combine(basePath, $"{Path.GetFileName(basePath)}.sln"));
        solution.projects = projects;
        VSProjectFileCreater.CreateSolutionContents(solution);
        solution.Save();
    }

    static void PreBuild()
    {
        Console.WriteLine("-PreBuild합니다. 추가 예정");
    }


}