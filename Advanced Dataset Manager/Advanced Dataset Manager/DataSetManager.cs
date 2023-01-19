using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;

namespace AdvancedDatasetManager
{
    public static class DataSetManager
    {
        public static readonly Dictionary<string, string> dataTypes = new Dictionary<string, string>()
        {
            { "str", "String" },
            { "flt", "Float" },
            { "int", "Integer" },
            { "dec", "Decimal" },
            { "lst", "Array" },
            { "lnk", "Dataset Link" },
            { "bag", "Dataset Bag" },
            { "pth", "Path" },
            { "var", "Dynamic" },
        };

        public static Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();

        public static string DataPath;
        public static void Init(string dataPath, string name)
        {
            DataPath = dataPath;
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            if (!File.Exists(dataPath + "/dataset.ads"))
            {
                File.Create(dataPath + "/dataset.ads");

                File.WriteAllText(dataPath + "/dataset.ads", $"[DataSet]\nstr name = {name}");
            }
            else
            {

                data = ReadDataSet(dataPath + "/dataset.ads");
                //foreach (var item in output.Keys)
                //{
                //    Console.WriteLine(item + " = " + output[item]);
                //}
            }
        }

        public static string DetectType(string data)
        {
            if (string.IsNullOrEmpty(data))
                throw new Exception("Cannot detect type of empty string (TODO)");

            data = data.Trim();
            float fout;
            decimal dout;
            int iout;
            if (data.StartsWith("\"") && data.EndsWith("\""))
                return "str";
            else if (data.StartsWith("[") && data.EndsWith("]"))
                return "lst";
            else if (data.EndsWith("f") && float.TryParse(data.Remove(data.Length - 1, 1), out fout))
                return "flt";
            else if (data.EndsWith("d") && decimal.TryParse(data.Remove(data.Length - 1, 1), out dout))
                return "dec";
            else if (int.TryParse(data.Remove(data.Length - 1, 1), out iout))
                return "int";
            else if (float.TryParse(data.Remove(data.Length - 1, 1), out fout))
                return "flt";
            else if (data.StartsWith("@\"") && data.EndsWith("\"") && data.Remove(data.Length - 1, 1).Trim().EndsWith(".ads"))
                return "lnk";
            else if (data.StartsWith("@\"") && data.EndsWith("\""))
                return "pth";
            throw new Exception("Undefined type! (TODO)");
        }

        public static dynamic ParseValue(string value, string givenType = null, string datasetPath = null)
        {
            if (string.IsNullOrEmpty(datasetPath))
                datasetPath = DataPath;

            if (string.IsNullOrEmpty(givenType))
                givenType = "var";

            var varType = DetectType(value);
            if (varType == "lnk" && givenType == "pth")
                varType = "pth";

            if (givenType != "var" && varType != givenType)
                throw new Exception("Wrong type (TODO)");

            value = value.Trim();

            if (varType == "str")
                return value.Remove(value.Length - 1, 1).Remove(0, 1);
            else if (varType == "int")
                return int.Parse(value);
            else if (varType == "flt")
                return float.Parse(value);
            else if (varType == "dec")
                return decimal.Parse(value);
            else if (varType == "lst")
            {
                return ParseList(value);
            }
            else if (varType == "lnk")
                return new DataSet(Path.Combine(Path.Combine(new FileInfo(datasetPath).Directory.FullName, @value.Remove(value.Length - 1, 1).Remove(0, 2))));
            else if (varType == "pth")
                return @value.Remove(value.Length - 1, 1).Remove(0, 2);
            else
                throw new Exception("Undefined type (TODO)");
        }

        public static dynamic[] ParseList(string parseText)
        {
            parseText = parseText.Trim();
            if (parseText[0] != '[' || parseText[parseText.Length - 1] != ']')
                throw new Exception("Wrong type (TODO)");
            parseText = parseText.Remove(parseText.Length - 1, 1).Remove(0, 1).Trim();
            var vals = parseText.Split(',');
            dynamic[] lst = new dynamic[vals.Length];
            int i = 0;
            foreach (var v in vals)
            {
                lst[i] = ParseValue(v);
                i++;
            }

            return lst;
        }

        public static Dictionary<string, dynamic> ReadDataSet(string datasetPath)
        {
            string[] readData = File.ReadAllLines(datasetPath);
            Dictionary<string, dynamic> output = new Dictionary<string, dynamic>();
            if (readData.Length == 0)
            {
                throw new Exception("Empty file (TODO)");
            }
            if (readData[0].Trim() != "[DataSet]")
            {
                throw new Exception("Wrong file (TODO)");
            }
            readData[0] = "";
            foreach (string lin in readData)
            {
                if (lin.Replace(" ", "") == "")
                    continue;

                string line = lin.Trim();
                if (lin.LastIndexOf("\"") < lin.LastIndexOf(";"))
                {
                    var x = lin.LastIndexOf(';');
                    line = lin.Substring(0, x).Trim();
                    if (line.Replace(" ", "") == "")
                        continue;
                }
                string varDfnt = line.Split('=')[0].Trim();
                while (varDfnt.Contains("  "))
                {
                    varDfnt = varDfnt.Replace("  ", " ");
                }
                var d = line.Split('=')[0];
                line = line.Replace(d, varDfnt + " ");
                if (varDfnt.Split(' ').Length != 2)
                    throw new Exception("Wrong definition (TODO)");
                string varType = varDfnt.Split(' ')[0];
                if (!dataTypes.ContainsKey(varType))
                {
                    throw new Exception("Wrong dataType (TODO)");
                }
                string varName = varDfnt.Split(' ')[1];
                if (line.Split('=').Length == 1)
                {
                    if (varType == "bag")
                    {
                        if (line.Split('.').Length != 1)
                        {
                            if (!output.ContainsKey(varName.Split('.')[0]))
                                throw new Exception("There is no dataset bag named as \"" + varName.Split('.')[0] + "\" (TODO)");
                            else if (output[line.Split('.')[0]] is DataSetBag)
                                throw new Exception("You can't create a dataset bag in a dataset bag (TODO)");
                            else
                                throw new Exception("\"" + varName.Split('.')[0] + "\" isn't a dataset bag, it is a \"" + dataTypes[varType] + "\" (TODO)");
                        }
                        output[varName.Trim()] = new DataSetBag(new Dictionary<string, dynamic>());
                    }
                    else
                        throw new Exception("Wrong definition (TODO)");
                }
                else
                {
                    if (varName.Split('.').Length == 2)
                    {
                        if (!output.ContainsKey(varName.Split('.')[0]))
                            throw new Exception("There is no dataset bag named as \"" + varName.Split('.')[0] + "\" (TODO)");
                        if (output[varName.Split('.')[0]] is DataSetBag)
                        {
                            var bag = (DataSetBag)output[varName.Split('.')[0]];
                            bag.SetMember(varName.Split('.')[1], ParseValue(line.Replace(varDfnt + " =", "").Trim(), varType, datasetPath));
                        }
                        else
                            throw new Exception("\"" + varName.Split('.')[0] + "\" isn't a dataset bag, it is a \"" + dataTypes[varType] + "\" (TODO)");
                    }
                    else if (varName.Split('.').Length == 1)
                    {
                        output[varName.Trim()] = ParseValue(line.Replace(varDfnt + " =", "").Trim(), varType, datasetPath);
                    }
                }
            }
            return output;
        }
    }
}