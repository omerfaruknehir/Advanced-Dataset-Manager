using System;
using System.Collections.Generic;
using System.Dynamic;

namespace AdvancedDatasetManager
{
    public class DataSetBag : DynamicObject
    {
        public Dictionary<string, dynamic> data { get; private set; } = new Dictionary<string, dynamic>();

        public DataSetBag(Dictionary<string, dynamic> data)
        {
            this.data = data;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;

            if (!data.ContainsKey(binder.Name))
                return false;

            result = data[binder.Name];
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!data.ContainsKey(binder.Name))
                data.Add(binder.Name, value);
            data[binder.Name] = value;

            return true;
        }

        public void SetMember(string memberName, object value)
        {
            if (!data.ContainsKey(memberName))
                data.Add(memberName, value);
            data[memberName] = value;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Delegate del = (Delegate)data[binder.Name];
            result = del.DynamicInvoke(args);

            return true;
        }
    }
}
