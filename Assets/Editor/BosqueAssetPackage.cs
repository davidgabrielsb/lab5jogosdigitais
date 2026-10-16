using System.IO;
using UnityEditor;
using UnityEngine;
public static class BosqueAssetPackage
{
    [MenuItem("Bosque/Exportar pacote de assets")]
    public static void Export()
    {
        string output=Path.GetFullPath(Path.Combine(Application.dataPath,"..","Builds","Lume-Assets.unitypackage"));
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        AssetDatabase.ExportPackage(new[]{"Assets/Resources","Assets/Animations","Assets/Scripts"},output,ExportPackageOptions.Recurse|ExportPackageOptions.IncludeDependencies);
        Debug.Log("BOSQUE_PACKAGE_OK: "+output);
    }
}
