using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Some.Random.Namespace
{
	/// <summary>
    /// Helper class to generate ToString() methods.
    /// Use it like this: string methodBody = ToStringHelper.Generate(typeof(YourType));
    /// </summary>
    public static class ToStringHelper
    {
        public static string Generate(Type type)
        {            
            MemberInfo[] memberInfos = type.GetMembers();

            const string methodString = @"public override string ToString()
        {{
            return {0};
        }}";

            const string outer = "GetType().Name + string.Format(\"{{{{{0}}}}}\", {1}{2})";

            List<MemberInfo> properties = memberInfos.Where(mi => mi.MemberType == MemberTypes.Property).ToList();

            int count = 0;
            var inners = new List<string>();
            foreach (var property in properties)
            {
                string inner = string.Format("{0}={{{1}}}", property.Name, count);
                inners.Add(inner);
                count++;
            }

            string firstHalf = string.Join(", ", inners);
            string secondHalf = string.Join(", ", properties.Select(p => p.Name));

            string body = string.Format(outer, firstHalf, "\n\t\t\t\t", secondHalf);

            return string.Format(methodString, body); 
        }
    }
}