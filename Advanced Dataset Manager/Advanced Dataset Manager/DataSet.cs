using System;
using System.IO;
using System.Collections.Generic;
using System.Dynamic;

namespace AdvancedDatasetManager
{
    public class DataSet : DynamicObject
    {
        public string path;
        public Dictionary<string, dynamic> data { get; private set; } = new Dictionary<string, dynamic>();
        bool isLoaded = false;

        public DataSet(string datasetPath)
        {
            if (datasetPath == null || !File.Exists(datasetPath))
                throw new Exception("Worng Dataset Path (TODO)");
            path = datasetPath;
        }

        public void SaveData()
        {
            //  TODO
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!isLoaded)
            {
                data = DataSetManager.ReadDataSet(path);
                isLoaded = true;
            }

            result = null;

            if (!data.ContainsKey(binder.Name))
                return false;

            result = data[binder.Name];
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!isLoaded)
            {
                data = DataSetManager.ReadDataSet(path);
                isLoaded = true;
            }

            if (!data.ContainsKey(binder.Name))
                data.Add(binder.Name, value);
            data[binder.Name] = value;

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (!isLoaded)
            {
                data = DataSetManager.ReadDataSet(path);
                isLoaded = true;
            }

            Delegate del = (Delegate)data[binder.Name];
            result = del.DynamicInvoke(args);

            return true;
        }
    }
}
