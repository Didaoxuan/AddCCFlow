using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BP.Tools
{
    public static class CloneHelper
    {
		/// <summary>
		/// 深度克隆
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <returns></returns>
		public static object CloneDeeply(this object obj)
		{
			object o = System.Activator.CreateInstance(obj.GetType()); //实例化一个T类型对象

			PropertyInfo[] propertyInfos = obj.GetType().GetProperties(); //获取T对象的所有公共属性

			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				object propertyValue = propertyInfo.GetValue(obj, null);

				if (propertyValue != null)
				{
					//判断值是否为空，如果空赋值为null
					if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
					{
						//如果convertsionType为nullable类，声明一个NullableConverter类，该类提供从Nullable类到基础基元类型的转换
						NullableConverter nullableConverter = new NullableConverter(propertyInfo.PropertyType);

						//将convertsionType转换为nullable对的基础基元类型
						propertyInfo.SetValue(o, Convert.ChangeType(propertyInfo.GetValue(obj), nullableConverter.UnderlyingType), null);
					}
					else
					{
						//支持克隆
						if (propertyValue is ICloneable)
						{
							//将convertsionType转换为nullable对的基础基元类型
							propertyInfo.SetValue(o, Convert.ChangeType((propertyValue as ICloneable).Clone(), propertyInfo.PropertyType), null);
						}
						else
						{
							object value;
							if (propertyValue is IList) //是列表对象
								value = CloneList(propertyValue as IList, propertyInfo.PropertyType);
							else
								value = propertyValue;

							propertyInfo.SetValue(o, value, null);
						}
					}
				}
			}
			return obj;
		}
		/// <summary>
		/// 克隆对象列表
		/// </summary>
		/// <param name="list"></param>
		/// <param name="listType"></param>
		/// <returns></returns>
		private static IList CloneList(IList list, Type listType)
		{
			IList ret = (IList)System.Activator.CreateInstance(listType);
			foreach (object obj in list)
			{
				ret.Add(obj.CloneDeeply());
			}
			return ret;
		}
	}
}
