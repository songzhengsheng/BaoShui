using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace OracleBase.HelpClass
{
    /// <summary>
    /// 反射帮助类
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// 创建对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullName">命名空间.类型名</param>
        /// <returns></returns>
        public static T CreateInstance<T>(string fullName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly(); // 获取当前程序集 
            //dynamic obj = assembly.CreateInstance(fullName); // 创建类的

            string path = fullName + "," + assembly;//命名空间.类型名,程序集
            Type o = Type.GetType(path);//加载类型
            object obj = Activator.CreateInstance(o, true);//根据类型创建实例
            return (T)obj;//类型转换并返回
        }

    }

}