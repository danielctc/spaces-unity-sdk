using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

[InitializeOnLoad]
public class DependencyInstaller
{
    private static bool isInitialized = false;
    private static List<string> requiredPackages = new List<string>
    {
        "com.photon.fusion"
    };

    static DependencyInstaller()
    {
        if (!isInitialized)
        {
            isInitialized = true;
            EditorApplication.delayCall += CheckAndInstallDependencies;
        }
    }

    private static async void CheckAndInstallDependencies()
    {
        foreach (string packageName in requiredPackages)
        {
            if (!IsPackageInstalled(packageName))
            {
                Debug.Log($"Installing required package: {packageName}");
                await InstallPackage(packageName);
            }
        }
    }

    private static bool IsPackageInstalled(string packageName)
    {
        var listRequest = Client.List();
        while (!listRequest.IsCompleted)
        {
            // Wait for the request to complete
        }

        if (listRequest.Status == StatusCode.Success)
        {
            foreach (var package in listRequest.Result)
            {
                if (package.name == packageName)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static async Task InstallPackage(string packageName)
    {
        var request = Client.Add(packageName);
        while (!request.IsCompleted)
        {
            await Task.Yield();
        }

        if (request.Status == StatusCode.Success)
        {
            Debug.Log($"Successfully installed {packageName}");
        }
        else
        {
            Debug.LogError($"Failed to install {packageName}: {request.Error.message}");
        }
    }
} 