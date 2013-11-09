using System;
using Fasterflect;
using System.Reflection;
using System.Linq;

namespace ObjectSerializer
{
	public class ClassSerializer : ISerializer
	{
		public ClassSerializer (Serializers serializer, Type type)
		{ 
			this.type = type;
			var properties = type.Properties (Flags.InstancePublic)
				.Where (n => n.CanRead && n.CanWrite)
				.ToArray ();
			this.getters = properties.Select (n => n.DelegateForGetPropertyValue ()).ToArray ();
			this.setters = properties.Select (n => n.DelegateForSetPropertyValue ()).ToArray ();
			this.serializers = properties.Select (n => serializer.FromDeclared (n.PropertyType)).ToArray ();
		}
		Type type;
		MemberGetter[] getters;
		MemberSetter[] setters;
		ISerializer[] serializers;

		#region implemented abstract members of SpecificSerializer
		public void Serialize (System.IO.Stream stream, object item)
		{
			for (int i=0; i<getters.Length; i++)
				serializers [i].Serialize (stream, getters[i](item));
		}
		public object Deserialize (System.IO.Stream stream)
		{
			var obj = type.CreateInstance ();
			for (int i=0; i<setters.Length; i++)
				setters [i](obj, serializers [i].Deserialize (stream));
			return obj;
		}
		#endregion
	}
}