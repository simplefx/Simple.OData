using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Simple.NExtLib
{
    public class ResourceLoader
    {
        public async static Task<string> LoadFileAsStringAsync(string folderName, string resourceName)
        {
            var resourceMap = Windows.ApplicationModel.Resources.Core.ResourceManager.Current.MainResourceMap;
            var resourceFile = await resourceMap.GetSubtree("Files/" + folderName)
                .GetValue(resourceName)
                .GetValueAsFileAsync();
            return await FileIO.ReadTextAsync(resourceFile);
        }

        public static string LoadResourceFileAsString(string folderName, string resourceName)
        {
            var content = LoadFileAsStringAsync(folderName, resourceName);
            content.Wait();
            return content.Result;
        }
    }
}