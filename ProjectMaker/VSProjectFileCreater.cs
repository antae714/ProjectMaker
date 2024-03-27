using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

internal class VSProjectFileCreater
{
    public struct Configurations
    {
        public string Name { get; set; }
        public string Platform { get; set; }
    }

    public static Configurations[] configurations =
    {
    new Configurations { Name = "Debug", Platform = "x64" },
    new Configurations { Name = "Release", Platform = "x64" }
    };

    public static VSProject CreateProject(Module module)
    {
        VSProject project = new VSProject();
		project.module = module;


		MakeProjectFile(project, module);
        MakeProjectFilterFile(project, module);
        MakeAutoDefines(project, module);

        return project;

    }


    public static void MakeProjectFile(VSProject ProjectFile, Module module)
    {
        StringBuilder sb = new StringBuilder();

        // XML declaration
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");

        // Project start tag
        sb.AppendLine("<Project DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");

        sb.AppendLine("\t<ItemGroup Label=\"ProjectConfigurations\">");

        foreach (var config in configurations)
        {
            sb.AppendLine($"\t\t<ProjectConfiguration Include=\"{config.Name}|{config.Platform}\">");
            sb.AppendLine($"\t\t\t<Configuration>{config.Name}</Configuration>");
            sb.AppendLine($"\t\t\t<Platform>{config.Platform}</Platform>");
            sb.AppendLine($"\t\t</ProjectConfiguration>");
        }

        sb.AppendLine("\t</ItemGroup>");

        // PropertyGroup for Globals
        sb.AppendLine($"\t<PropertyGroup Label=\"Globals\">");
        sb.AppendLine($"\t\t<VCProjectVersion>17.0</VCProjectVersion>");
        sb.AppendLine($"\t\t<Keyword>Win32Proj</Keyword>");
        sb.AppendLine($"\t\t<ProjectGuid>{{{module.GUID.ToString("B").ToUpperInvariant()}}}</ProjectGuid>");
        sb.AppendLine($"\t\t<RootNamespace>{module.Name}</RootNamespace>");
        sb.AppendLine($"\t\t<WindowsTargetPlatformVersion>10.0</WindowsTargetPlatformVersion>");
        sb.AppendLine($"\t</PropertyGroup>");

        // Import elements
        sb.AppendLine("\t<Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.Default.props\" />");

        foreach (var config in configurations)
        {
            // PropertyGroup elements for configurations
            bool IsRelease = config.Name == "Release";
            sb.AppendLine($"\t<PropertyGroup Condition=\"'$(Configuration)|$(Platform)'=='{config.Name}|{config.Platform}'\" Label=\"Configuration\">");
            string Output = (module.BuildOutput == E_BuildOutput.Application) ? "Application" : (IsRelease ? "StaticLibrary" : "DynamicLibrary");
            sb.AppendLine($"\t\t<ConfigurationType>{Output}</ConfigurationType>");
            string UseDebugLibraries = IsRelease ? "false" : "true";
            sb.AppendLine($"\t\t<UseDebugLibraries>{UseDebugLibraries}</UseDebugLibraries>");
            sb.AppendLine($"\t\t<PlatformToolset>v143</PlatformToolset>");
            if(IsRelease) sb.AppendLine($"\t\t<WholeProgramOptimization>true</WholeProgramOptimization>");
            sb.AppendLine($"\t\t<CharacterSet>Unicode</CharacterSet>");
            sb.AppendLine($"\t</PropertyGroup>");
            // Add similar PropertyGroup elements for other configurations...
        }

        // Import elements
        sb.AppendLine("\t<Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.props\" />");

        sb.AppendLine($"\t<ImportGroup Label=\"ExtensionSettings\">");
        sb.AppendLine($"\t</ImportGroup>");

        sb.AppendLine($"\t<ImportGroup Label=\"Shared\">");
        sb.AppendLine($"\t</ImportGroup>");


        foreach (var config in configurations)
        {
            // ImportGroup elements for PropertySheets
            sb.AppendLine($"\t<ImportGroup Label=\"PropertySheets\" Condition=\"'$(Configuration)|$(Platform)'=='{config.Name}|{config.Platform}'\">");
            sb.AppendLine($"\t\t<Import Project=\"$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props\" Condition=\"exists('$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props')\" Label=\"LocalAppDataPlatform\" />");
            sb.AppendLine($"\t</ImportGroup>");
            // Add similar ImportGroup elements for other configurations...

        }

        sb.AppendLine("\t<PropertyGroup Label=\"UserMacros\"/>");

        foreach (var config in configurations)
        {
            // PropertyGroup elements for configuration-specific settings
            sb.AppendLine($"\t<PropertyGroup Condition=\"'$(Configuration)|$(Platform)'=='{config.Name}|{config.Platform}'\">");


            //    <IncludePath>$(SolutionDir)Source\Core\;$(IncludePath)</IncludePath>
            sb.Append("    <IncludePath>");


            sb.Append($"$(SolutionDir)Source\\{module.Name}\\;");
            sb.Append($"$(SolutionDir)Source\\{module.Name}\\Private\\;");
            sb.Append($"$(SolutionDir)Source\\{module.Name}\\Public\\;");
            sb.Append($"$(SolutionDir)Intermediate\\Includes\\{module.Name}\\;");

            sb.AppendLine("$(IncludePath)</IncludePath>");

            sb.Append($"\t\t<ExternalIncludePath>");

            foreach (var item in module.LinkModuleName)
            {
                //sb.Append($"$(SolutionDir){item}\\;");
                //sb.Append($"$(SolutionDir){item}\\Private\\;");
                sb.Append($"$(SolutionDir)Source\\{item}\\Public\\;");
            }
            if (module.ExternalDllPaths.Count != 0)
            {
                foreach (var item in module.ExternalDllPaths)
                {
                    sb.Append($"{item}");
                }

            }


            sb.AppendLine($"$(ExternalIncludePath)</ExternalIncludePath>");
            //sb.AppendLine($"<ExternalIncludePath>$(SolutionDir)Core\\;$(ExternalIncludePath)</ExternalIncludePath>");
            //sb.AppendLine($"<SourcePath>$(SourcePath)</SourcePath>");
            sb.AppendLine($"\t\t<LibraryPath>$(SolutionDir)Binaries\\{config.Name};$(LibraryPath)</LibraryPath>");
            sb.AppendLine($"\t\t<OutDir>$(SolutionDir)Binaries\\{config.Name}\\</OutDir>");
            sb.AppendLine($"\t\t<IntDir>$(SolutionDir)Intermediate\\Build\\{module.Name}\\{config.Name}\\</IntDir>");
            sb.AppendLine($"\t</PropertyGroup>");
            // Add similar PropertyGroup elements for other configurations...

        }


        foreach (var config in configurations)
        {
            // ItemDefinitionGroup elements
            bool IsRelease = config.Name == "Release";
            sb.AppendLine($"\t<ItemDefinitionGroup Condition=\"'$(Configuration)|$(Platform)'=='{config.Name}|{config.Platform}'\">");

            sb.AppendLine($"\t<ClCompile>");
            sb.AppendLine($"\t\t<WarningLevel>Level3</WarningLevel>");
            if (IsRelease)
            {
                sb.AppendLine($"\t\t<FunctionLevelLinking>true</FunctionLevelLinking>");
                sb.AppendLine($"\t\t<IntrinsicFunctions>true</IntrinsicFunctions>");
            }
            sb.AppendLine($"\t\t<SDLCheck>true</SDLCheck>");
            string PreprocessorDefinitions = IsRelease ? "NDEBUG;" : "_DEBUG;";
            PreprocessorDefinitions += module.BuildOutput == E_BuildOutput.Application ?
                $"_CONSOLE;%(PreprocessorDefinitions)" :
                $"{module.Name.ToUpper()}_EXPORTS;_WINDOWS;_USRDLL;%(PreprocessorDefinitions)";


            sb.AppendLine($"\t\t<PreprocessorDefinitions>{PreprocessorDefinitions}</PreprocessorDefinitions>");
            sb.AppendLine($"\t\t<ConformanceMode>false</ConformanceMode>");
			sb.AppendLine($"\t\t<ForcedIncludeFiles>$(SolutionDir){ProjectFile.AutoDefinitionsRelativeDirectory};%(ForcedIncludeFiles)</ForcedIncludeFiles>");

			sb.AppendLine($"\t\t<LanguageStandard>stdcpp20</LanguageStandard>");
            sb.AppendLine($"\t</ClCompile>");
			//ForcedIncludeFiles
			//ConformanceMode

			sb.AppendLine($"\t<Link>");


            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///콘솔 플젝용
            string subsystem = module.BuildOutput == E_BuildOutput.Application ? "CONSOLE" : "Windows";
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            sb.AppendLine($"\t\t<SubSystem>{subsystem}</SubSystem>");

            if (!IsRelease)
            {
                sb.AppendLine($"\t\t<GenerateDebugInformation>true</GenerateDebugInformation>");
                sb.AppendLine($"\t\t<EnableUAC>false</EnableUAC>");
                if(module.LinkModuleName.Count != 0)
                {
                    sb.AppendLine($"\t\t<DelayLoadDLLs>%(DelayLoadDLLs)</DelayLoadDLLs>");
                }

            }
            else
            {
                sb.AppendLine($"\t\t<EnableCOMDATFolding>true</EnableCOMDATFolding>");
                sb.AppendLine($"\t\t<OptimizeReferences>true</OptimizeReferences>");
                sb.AppendLine($"\t\t<GenerateDebugInformation>true</GenerateDebugInformation>");
                sb.AppendLine($"\t\t<EnableUAC>false</EnableUAC>");
            }

            if (module.LinkModuleName.Count != 0)
            {
                sb.Append($"\t\t<AdditionalDependencies>");
                foreach (var item in module.LinkModuleName)
                {
                    sb.Append($"{item}.lib;");
                }
                sb.AppendLine($"%(AdditionalDependencies)</AdditionalDependencies>");
            }

            sb.AppendLine($"\t</Link>");


            sb.AppendLine($"    <PreBuildEvent>");
            sb.AppendLine($"      <Command>$(SolutionDir)ProjectMaker.exe -prebuild</Command>");
            sb.AppendLine($"    </PreBuildEvent>");


            sb.AppendLine($"\t</ItemDefinitionGroup>");
            // Add similar ItemDefinitionGroup elements for other configurations...
        }

        string RelaiveDirectory = ModuleScanner.GetRelativePath(module.RootDirectory, module.Directory);


        // ItemGroup elements for source files
        sb.AppendLine("\t<ItemGroup>");
        sb.AppendLine($"\t\t<ClInclude Include=\"../Includes\\{module.Name}\\{module.Name}pch.h\" />");
        foreach (var item in module.Headers)
        {
            sb.AppendLine($"\t\t<ClInclude Include=\"../../{RelaiveDirectory}{item}\" />");
        }
        // Add more ClInclude elements for other header files...
        sb.AppendLine("\t</ItemGroup>");

        // ItemGroup elements for compile files
        sb.AppendLine("\t<ItemGroup>");
        foreach (var item in module.Sources)
        {
            sb.AppendLine($"\t\t<ClCompile Include=\"../../{RelaiveDirectory}{item}\" />");
        }
        


        sb.AppendLine("\t</ItemGroup>");

        // Import element for Microsoft.Cpp.targets
        sb.AppendLine("\t<Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.targets\" />");

        // ImportGroup for ExtensionTargets
        sb.AppendLine("\t<ImportGroup Label=\"ExtensionTargets\">");
        //sb.AppendLine("\t\t<!-- Add ExtensionTargets imports here -->");
        sb.AppendLine("\t</ImportGroup>");

        // Project end tag
        sb.AppendLine("</Project>");

        ProjectFile.ProjectContent = sb.ToString();
        ProjectFile.module = module;
        return;
    }


    public static void MakeProjectFilterFile(VSProject ProjectFile, Module module)
    {
        StringBuilder sb = new StringBuilder();

        var sourceFiles = module.Sources.Select(filePath => new { FilePath = filePath, Filter = Path.GetDirectoryName(filePath) });
        var headerFiles = module.Headers.Select(filePath => new { FilePath = filePath, Filter = Path.GetDirectoryName(filePath) });
        var uniqueFilters = new HashSet<string>();

        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<Project ToolsVersion=\"Current\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");

		sb.AppendLine("  <ItemGroup>");


        foreach (var filter in sourceFiles.Select(file => file.Filter).Concat(headerFiles.Select(file => file.Filter)))
        {
            if (string.IsNullOrEmpty(filter)) continue;
            string currentFilter = "";
            foreach (var part in filter.Split(Path.DirectorySeparatorChar))
            {
                currentFilter += part + Path.DirectorySeparatorChar;
                uniqueFilters.Add(currentFilter);
            }
        }

        foreach (var filter in uniqueFilters)
        {
            string trimmedFilter = filter.TrimEnd(Path.DirectorySeparatorChar);
            sb.AppendLine($"    <Filter Include=\"{trimmedFilter}\">");
            sb.AppendLine($"    </Filter>");
        }
        sb.AppendLine($"    <Filter Include=\"AutomaticGeneration\">");
        sb.AppendLine($"    </Filter>");

        sb.AppendLine($"  </ItemGroup>");

        sb.AppendLine("  <ItemGroup>");
		foreach (var sourceFile in sourceFiles)
		{
			sb.AppendLine($"    <ClCompile Include=\"../../Source/{module.Name}/{sourceFile.FilePath}\">");
			if (sourceFile.Filter != "")
			{
				sb.AppendLine($"      <Filter>{sourceFile.Filter}</Filter>");
			}
			sb.AppendLine($"    </ClCompile>");
        }

        sb.AppendLine("  </ItemGroup>");
		sb.AppendLine("  <ItemGroup>");

		foreach (var headerFile in headerFiles)
		{
			sb.AppendLine($"    <ClInclude Include=\"../../Source/{module.Name}/{headerFile.FilePath}\">");
			if (headerFile.Filter != "")
			{
				sb.AppendLine($"      <Filter>{headerFile.Filter}</Filter>");
			}
			sb.AppendLine($"    </ClInclude>");
        }


        sb.AppendLine("  </ItemGroup>");
		sb.AppendLine("</Project>");
		ProjectFile.ProjectFilterContent = sb.ToString();
    }

    public static void MakeAutoDefines(VSProject ProjectFile, Module module)
    {
        string moduleNameUpper = module.Name.ToUpper();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"");
        //?
        sb.AppendLine($"#define WIN32_LEAN_AND_MEAN             // 거의 사용되지 않는 내용을 Windows 헤더에서 제외합니다.");
        sb.AppendLine($"#include <windows.h>");
        sb.AppendLine($"");

        sb.AppendLine($"#define {moduleNameUpper}MODULEAPI __declspec(dllexport)");
        //MakeModuleAPI(sb, moduleNameUpper);

        foreach (var linkModuleName in module.LinkModuleName)
        {
            if (string.IsNullOrEmpty(linkModuleName)) continue;
            sb.AppendLine($"#define {linkModuleName.ToUpper()}MODULEAPI __declspec(dllimport)");
            //MakeModuleAPI(sb, linkModuleName.ToUpper());
        }

        sb.AppendLine($"");

        ProjectFile.AutoDefinitions = sb.ToString();
    }

    public static string ConvertFilePathToFilterPath(string filePath, string projectDirectory)
    {
        // 파일 경로를 상대 경로로 변환
        //string relativePath = ModuleScanner.GetRelativePath(projectDirectory, filePath);

        // 파일 경로의 디렉토리 부분을 필터 경로로 사용
        string filterPath = Path.GetDirectoryName(filePath);

        // 필터 경로의 디렉토리 구분자를 역슬래시로 변경하여 윈도우 경로 형식으로 만듦
        filterPath = filterPath.Replace(Path.DirectorySeparatorChar, '\\');

        return filterPath;
    }


    public static void CreateSolutionContents(VSSolution solution)
    {
        List<VSProject> projects = solution.projects;

        StringBuilder sb = new StringBuilder();


		sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
        sb.AppendLine("# Visual Studio Version 17");
        sb.AppendLine("VisualStudioVersion = 17.9.34622.214");
        sb.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");

        // Write project entries
        foreach (var project in projects)
        {
            string relativePath = ModuleScanner.GetRelativePath(solution.FilePath, project.module.ProjectDirectory + project.module.Name + ".vcxproj");
            sb.AppendLine($"Project(\"8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942\") = \"{project.module.Name}\", \"{relativePath}\", \"{{{project.module.GUID}}}\"");
            if (project.module.Name == "Application")
            {
                sb.AppendLine("\tProjectSection(ProjectDependencies) = postProject");
                foreach (var dependencyProject in projects)
                {
                    sb.AppendLine($"\t\t{dependencyProject.module.GUID} = {dependencyProject.module.GUID}");
                }
                sb.AppendLine("\tEndProjectSection");
            }
            else if(project.module.LinkModuleName.Count > 0)
            {
				sb.AppendLine("\tProjectSection(ProjectDependencies) = postProject");
				foreach (var dlls in project.module.LinkModuleName)
				{
					var temp = from projectEle in projects
							   where projectEle.module.Name == dlls
							   select projectEle.module.GUID;

                    foreach (var item in temp)
					{
						sb.AppendLine($"\t\t{item} = {item}");
					}
				}
				sb.AppendLine("\tEndProjectSection");
			}
            sb.AppendLine("EndProject");
        }

		// Write global section
		sb.AppendLine("Global");
        sb.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
        sb.AppendLine("\t\tDebug|x64 = Debug|x64");
        sb.AppendLine("\t\tRelease|x64 = Release|x64");
        sb.AppendLine("\tEndGlobalSection");

        // Write project configuration platforms
        sb.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
        foreach (var project in projects)
        {
            foreach (var config in configurations)
            {
                sb.AppendLine($"\t\t{project.module.GUID}.{config.Name}.ActiveCfg = {config.Platform}");
                sb.AppendLine($"\t\t{project.module.GUID}.{config.Name}.Build.0 = {config.Platform}");
            }
        }
        sb.AppendLine("\tEndGlobalSection");

        // Write solution properties
        sb.AppendLine("\tGlobalSection(SolutionProperties) = preSolution");
        sb.AppendLine("\t\tHideSolutionNode = FALSE");
        sb.AppendLine("\tEndGlobalSection");

        // Write solution extensibility globals
        sb.AppendLine("\tGlobalSection(ExtensibilityGlobals) = postSolution");
        sb.AppendLine("\t\tSolutionGuid = {FBB33CB4-AD65-4F24-B309-B8CAA292F9F1}");
        sb.AppendLine("\tEndGlobalSection");
        sb.AppendLine("EndGlobal");

        solution.SolutionFileContent = sb.ToString();
        
    }

}