using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


internal class ModuleScanner
{
    public List<Module> Modules { get; } = new List<Module>();

    public void ScanModules(string rootDirectory)
    {
        var moduleFiles = Directory.EnumerateFiles(rootDirectory, "*.module", SearchOption.AllDirectories).ToList();

        foreach (string moduleFile in moduleFiles)
        {
            string moduleDirectory = Path.GetDirectoryName(moduleFile) + Path.DirectorySeparatorChar;

            Modules.Add(CreateModule(rootDirectory, moduleDirectory));
        }
        PostScanModules();

    }



    public void PostScanModules()
    {
        foreach (var module in Modules)
        {
            var linkedModules = Modules.Where(m => module.LinkModuleName.Contains(m.Name));
            foreach (var linkedModule in linkedModules)
            {
                module.LinkModuleDirectory.Add(linkedModule.Directory);
            }
        }
    }

        private Module CreateModule(string RootDirectory, string Directory)
    {
        var files = System.IO.Directory.EnumerateFiles(Directory, "*", SearchOption.AllDirectories);

        Module module = new Module();

        List<string> sources = new List<string>();
        List<string> headers = new List<string>();

        foreach (string file in files)
        {
            string extension = Path.GetExtension(file);
            if (extension.Equals(".cpp", StringComparison.OrdinalIgnoreCase))
            {
                sources.Add(GetRelativePath(Directory, file));
            }
            else if (extension.Equals(".h", StringComparison.OrdinalIgnoreCase))
            {
                headers.Add(GetRelativePath(Directory, file));
            }
        }
        module.RootDirectory = RootDirectory + Path.DirectorySeparatorChar;
        module.Directory = Directory;
        module.Sources = sources.ToArray();
        module.Headers = headers.ToArray();
        module.Name = Path.GetFileName(Directory.TrimEnd(Path.DirectorySeparatorChar));
        module.GUID = CalculateMD5Hash(module.Name);
        module.ProjectDirectory = $"{RootDirectory}\\Intermediate\\ProjectFiles\\";
        string moduleFilePath = module.Directory + module.Name + ".module";
        module.DeserializeData(moduleFilePath);

        return module;
    }

    public static string GetRelativePath(string rootDirectory, string filePath)
    {
        Uri rootUri = new Uri(rootDirectory);
        Uri fileUri = new Uri(filePath);
        return rootUri.MakeRelativeUri(fileUri).ToString();
    }

    static Guid CalculateMD5Hash(string input)
    {
        // MD5 해시 생성
        using (MD5 md5 = MD5.Create())
        {
            // 문자열을 바이트 배열로 변환하여 해싱
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            byte[] Hash = MD5.Create().ComputeHash(inputBytes);
            Hash[6] = (byte)(0x30 | (Hash[6] & 0x0f)); // 0b0011'xxxx Version 3 UUID (MD5)
            Hash[8] = (byte)(0x80 | (Hash[8] & 0x3f)); // 0b10xx'xxxx RFC 4122 UUID
            Array.Reverse(Hash, 0, 4);
            Array.Reverse(Hash, 4, 2);
            Array.Reverse(Hash, 6, 2);
            return new Guid(Hash);
        }
    }
}
