using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using Microsoft.Build.Evaluation;
using System.Linq;

internal class VSSolution
{

    public VSSolution(string _filePath)
    {
        FilePath = _filePath;
    }
    public string SolutionFileContent { get; set; }
    public string FilePath { get; private set; }

    public List<VSProject> projects;
    public void Save()
    {
        string directoyName = Path.GetDirectoryName(FilePath) + Path.DirectorySeparatorChar;
        foreach (var project in projects)
        {
            project.Save(directoyName);
        }


        using (StreamWriter writer = new StreamWriter(FilePath))
        {
            writer.Write(SolutionFileContent);
        }
    }

}
internal class VSProject
{
    public string ProjectContent;
    public string ProjectFilterContent;
	public string AutoDefinitions;
	public string AutoDefinitionsRelativeDirectory
    {
        get
        {
            return $"{ProjectMaker.IntermediateIncludesDirectory}{module.Name}\\AutoDefinitions{module.Name}.h";

		}
    }

	public Module module;



    public Dictionary<string, string> Configurations { get; set; }
    public void Save(string SoultionDirectory)
    {
        // 경로가 존재하지 않으면 디렉터리 생성

        Directory.CreateDirectory(module.ProjectDirectory);
        Directory.CreateDirectory($"{SoultionDirectory}{ProjectMaker.IntermediateIncludesDirectory}{module.Name}\\");

        using (StreamWriter writer = new StreamWriter($"{module.ProjectDirectory}{module.Name}.vcxproj"))
        {
            writer.Write(ProjectContent);
        }
		using (StreamWriter writer = new StreamWriter($"{module.ProjectDirectory}{module.Name}.vcxproj.filters"))
		{
			writer.Write(ProjectFilterContent);
		}
        using (StreamWriter writer = new StreamWriter($"{SoultionDirectory}{AutoDefinitionsRelativeDirectory}", false, Encoding.Unicode))
        {
            writer.Write(AutoDefinitions);
        }
    }
	public IEnumerable<Module> GetLinkModule(List<VSProject> projects)
	{
		return from linkModuleName in module.LinkModuleName
			   from projectElement in projects
			   where projectElement.module.Name == linkModuleName
			   select projectElement.module;

	}

	public IEnumerable<Guid> GetLinkModuleGuids(List<VSProject> projects)
	{
        return from linkModule in GetLinkModule(projects)
		       select linkModule.GUID;

	}


}