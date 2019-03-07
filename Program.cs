using MicroBatchFramework;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace csjo
{
    public class CsJo : BatchBase
    {
        public void Convert(
            [Option("a", "array.")]string arr = "",
            [Option("o", "object.")]string obj = "",
            [Option("p", "pretty print json.")]bool pretty = false)
        {
            // check if array and object both are not set
            if (arr != "" && obj != "")
            {
                throw new CsJoException("you can't set both array and object.");
            }

            // print json
            if (arr != "")
            {
                this.Context.Logger.LogInformation(this.ConvertArrayToJson(arr, pretty));
            }
            else if (obj != "")
            {
                this.Context.Logger.LogInformation(this.ConvertObjToJson(obj, pretty));
            }
        }

        [Command("version")]
        public void ShowVersion(
            [Option("p", "pretty print json.")]bool pretty = false
        )
        {
            Console.WriteLine(JsonConvert.SerializeObject(new Dictionary<string, object>() {
                    { "Program", "csjo" },
                    { "Description", "This is inspired by jpmens/jo and skanehira/gjo" },
                    { "Author", "orange634nty" },
                    { "Repo", "https://github.com/orange634nty/csjo" },
                    { "Version", "1.0.0" }
            }, pretty ? Formatting.Indented : Formatting.None));
        }

        private string ConvertArrayToJson(string arr, bool pretty)
        {
            return JsonConvert.SerializeObject(
                arr.Split(" ").Select(a => this.ConvertValue(a)),
                pretty ? Formatting.Indented : Formatting.None
            );
        }

        private string ConvertObjToJson(string obj, bool pretty)
        {
            var res = new Dictionary<string, object>();
            foreach (string el in obj.Split(" "))
            {
                string[] keyValue = el.Split("=");
                if (keyValue.Length == 2)
                {
                    res.Add(keyValue[0], this.ConvertValue(keyValue[1]));
                }
                else
                {
                    throw new CsJoException($"wrong format : {el}");
                }
            }
            return JsonConvert.SerializeObject(res, pretty ? Formatting.Indented : Formatting.None);
        }

        private dynamic ConvertValue(string value)
        {
            // object
            if (value.StartsWith("{") && value.EndsWith("}"))
            {
                return JsonConvert.DeserializeObject(value);
            }

            // array
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                return JsonConvert.DeserializeObject(value);
            }

            // try to parse string to int
            try
            {
                return Int32.Parse(value);
            }
            catch (FormatException)
            {
                return value;
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }

    public class CsJoException : Exception
    {
        public CsJoException(string message) : base(message) { }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder().RunBatchEngineAsync<CsJo>(args);
        }
    }
}
