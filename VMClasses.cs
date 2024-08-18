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
                string propertyName = vmProp.PropertyInfo!.Name;
                string filterValue = vmProp.Filter;

                if (vmProp.PropertyInfo.PropertyType == typeof(string))
                {
                    query = query.Where($"{propertyName}.Contains(@0)", filterValue);
                }
                else if (vmProp.PropertyInfo.PropertyType == typeof(int) || vmProp.PropertyInfo.PropertyType == typeof(int?))
                {
                    if (int.TryParse(filterValue, out int intValue))
                    {
                        query = query.Where($"{propertyName} == @0", intValue);
                    }
                }
                else if (vmProp.PropertyInfo.PropertyType == typeof(decimal) || vmProp.PropertyInfo.PropertyType == typeof(decimal?))
                {
                    if (decimal.TryParse(filterValue, out decimal decimalValue))
                    {
                        query = query.Where($"{propertyName} == @0", decimalValue);
                    }
                }
                else if (vmProp.PropertyInfo.PropertyType == typeof(bool) || vmProp.PropertyInfo.PropertyType == typeof(bool?))
                {
                    if (bool.TryParse(filterValue, out bool boolValue))
                    {
                        query = query.Where($"{propertyName} == @0", boolValue);
                    }
                }
                else if (vmProp.PropertyInfo.PropertyType == typeof(DateTime) || vmProp.PropertyInfo.PropertyType == typeof(DateTime?))
                {
                    if (DateTime.TryParse(filterValue, out DateTime dateTimeValue))
                    {
                        query = query.Where($"{propertyName} == @0", dateTimeValue);
                    }
                }
                else
                {
                    query = query.Where($"{propertyName}.ToString() == @0", filterValue);
                }
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
