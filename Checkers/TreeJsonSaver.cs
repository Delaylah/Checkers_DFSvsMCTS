using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckersBoard
{
    public static class TreeJsonSaver
    {
        public static void Save<T>(string name, Node<T> root)
        {
            var folder = @"C:\Users\mersa\Desktop\TreeVisualze\json";
            var model = map(root);
            var json = JsonConvert.SerializeObject(model, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            var path = Path.Combine(folder, name + ".json");
            File.WriteAllText(path, json);
        }

        private static NodeJsonModel map<T>(Node<T> node)
        {
            return new NodeJsonModel
            {
                Name = node.RenderName(),
                Children = node.Children?.Select(c => map(c as Node<T>)).ToList()
            };
        }
    }
}
