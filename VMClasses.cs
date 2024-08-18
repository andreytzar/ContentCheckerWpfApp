using ContentCheckerWpfApp.Models.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using ContentCheckerWpfApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;


namespace ContentCheckerWpfApp
{
    internal class VMClasses
    {

    }

    public class VMPropertyInfo
    {
        public PropertyInfo? PropertyInfo { get; set; }
        public bool Show { get; set; }=true;
        public string Filter { get; set; } = string.Empty;
        public override string? ToString() => PropertyInfo?.Name;
        public static List<VMPropertyInfo> GetListVMPropInfo(List<PropertyInfo> properties)
        {
            var list = new List<VMPropertyInfo>();
            foreach (PropertyInfo property in properties) 
            {
                list.Add(new() { PropertyInfo = property });
            }
            return list;
        }
        public static IQueryable<object> ApplyFilters(IQueryable<object> query, List<VMPropertyInfo> vmProps)
        {    
            foreach (var vmProp in vmProps.Where(x => !string.IsNullOrEmpty(x.Filter)))
            {
                string filterCondition = $"{vmProp.ToString()}.Contains(@0)";
                query = query.Where(filterCondition, vmProp.Filter);
            }
            string select =string.Join(", ", vmProps.Where(x=>x.Show).Select(x => x.ToString()).ToArray());
            return query.Select($"new({select})").Cast<object>(); 
        }

    }

    public class VMTable
    {
        public Type? Type { get; set; }
        public string? Name { get => Type?.Name; }
        public override string? ToString() => Name;
        public static List<VMTable> VMTables = new List<VMTable>() { new() { Type = typeof(Site) }, new() { Type = typeof(Page) }, new() { Type = typeof(Link) } };
    }

}
