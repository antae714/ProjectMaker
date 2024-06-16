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

        string basePath;

        if (args.Length > 1 && Directory.Exists(args[1]))
        {
            basePath = args[1];
        }
        else
        {
            Console.WriteLine("현재 경로를 사용합니다.");
            basePath = Directory.GetCurrentDirectory();
        }
        
        switch (command.ToLower())
        {
            case "-setup":

                ProjectMake(basePath);

                Console.WriteLine("끝");
                Console.ReadKey();
                break;
            
            case "-prebuild":
                PreBuild();
                
                break;
            case "-postbuild":
                PostBuild();
            
                break;
            default:
                Console.WriteLine("잘못된 인수입니다. -Setup 또는 -PreBuild를 사용하세요.");
                break;
        }


    }

    static void ProjectMake(string basePath)
    {
        VSProjectFileCreater projectCreater = new VSProjectFileCreater();

        ModuleScanner scanner = new ModuleScanner();
        scanner.ScanModules(basePath);

		projectCreater.CreateProjects(scanner.Modules);
		projectCreater.CreateSolutionContents(basePath).Save();

	}

    static void PreBuild()
    {
        //Console.WriteLine("-PreBuild. 추가 예정");
    }
    static void PostBuild()
    {
        //Console.WriteLine("-PostBuild. 추가 예정");
    }

}