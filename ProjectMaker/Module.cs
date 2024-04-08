using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


class Module
{
    public struct SerializedData
    {
        public List<string> LinkModuleName { get; set; }
        public List<string> ExternalDllNames { get; set; }
        public List<string> ExternalDllPaths { get; set; }
        public string BuildOutput { get; set; }
    }

    public SerializedData SerializeData = new SerializedData();
    public string Name { get; set; }
    public string RootDirectory { get; set; }
    public string Directory { get; set; }
    public string ProjectDirectory { get; set; }
    public string[] Sources { get; set; }
    public string[] Headers { get; set; }
    public Guid GUID { get; set; }
    public E_BuildOutput BuildOutput { get; set; }

    public List<string> LinkModuleDirectory = new List<string>();
    public List<string> LinkModuleName => SerializeData.LinkModuleName;
    public List<string> ExternalDllNames => SerializeData.ExternalDllNames;
    public List<string> ExternalDllPaths => SerializeData.ExternalDllPaths;

    public void DeserializeData(string moduleFilePath)
    {
        using (StreamReader reader = new StreamReader(moduleFilePath))
        {
            string json = reader.ReadToEnd();

            SerializedData data = JsonSerializer.Deserialize<SerializedData>(json);
            if (string.IsNullOrEmpty(data.BuildOutput)) data.BuildOutput = "Library";

            if (data.LinkModuleName == null)
                data.LinkModuleName = new List<string>();

            if (data.ExternalDllNames == null)
                data.ExternalDllNames = new List<string>();

            if (data.ExternalDllPaths == null)
                data.ExternalDllPaths = new List<string>();
            
            SerializeData = data;

            BuildOutput = (E_BuildOutput)Enum.Parse(typeof(E_BuildOutput), data.BuildOutput);
        }
    }
}


enum E_BuildOutput
{
    NONE = 0,

    Library,
    Application,

    MAX
}
