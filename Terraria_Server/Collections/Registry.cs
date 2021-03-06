﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using Terraria_Server.Logging;
using Terraria_Server.Misc;

namespace Terraria_Server.Collections
{
    public class Registry<T> where T : IRegisterableEntity
    {
        protected Dictionary<int, List<T>> typeLookup = new Dictionary<int, List<T>>();
        protected Dictionary<string, T> nameLookup = new Dictionary<string, T>();

        protected string DEFINITIONS = "Terraria_Server.Definitions.";

        private readonly T defaultValue;

        public Registry ()
        {
            this.defaultValue = Activator.CreateInstance<T>();
        }
        
		public void Load (string filePath)
		{
			var document = new XmlDocument ();
			document.Load (Assembly.GetExecutingAssembly().GetManifestResourceStream(DEFINITIONS + filePath));
			var nodes = document.SelectNodes ("/*/*");
			var ser = new XmlSerializer (typeof(T));
			
			foreach (XmlNode node in nodes)
			{
				try
				{
					var rdr = new XmlNodeReader (node);
					var t = (T) ser.Deserialize (rdr);
					
					//ProgramLog.Debug.Log ("Created entity {0}, {1}", t.Type, t.Name);
					
					t.Name = String.Intern (t.Name);
					//Networking.StringCache.Add (System.Text.Encoding.ASCII.GetBytes (t.Name), t.Name);
					Networking.StringCache.Add (t.Name);
					
					if (typeLookup.ContainsKey(t.Type))
					{
						List<T> values;
						if (typeLookup.TryGetValue(t.Type, out values))
						{
							values.Add(t);
						}
					}
					else
					{
						List<T> values = new List<T>();
						values.Add(t);
						typeLookup.Add(t.Type, values);
					}
					
					if (!nameLookup.ContainsKey(t.Name))
					{
						nameLookup.Add(t.Name, t);
					}
				}
				catch (Exception e)
				{
					ProgramLog.Log (e, "Error adding element");
					ProgramLog.Error.Log ("Element was:\n" + node.ToString());
				}
				
			}
		}

        public T Default
        {
            get
            {
                return CloneAndInit(defaultValue);
            }
        }

        public virtual T Create(int type, int index = 0)
        {
            List<T> values;
            if (typeLookup.TryGetValue(type, out values))
            {
                if (values.Count > 0)
                {
                    return CloneAndInit(values[index]);
                }
            }
            return CloneAndInit(defaultValue);
        }

        public void SetDefaults (T obj, int type)
        {
            List<T> values;
            if (typeLookup.TryGetValue(type, out values))
            {
                if (values.Count > 0)
                {
                    obj.CopyFieldsFrom (values[0]);
                    return;
                }
                else
                    throw new ApplicationException ("Registry.SetDefaults(T, int): type " + type + " not found.");
            }
            throw new ApplicationException ("Registry.SetDefaults(T, int): type " + type + " not found.");
        }
        
        public void SetDefaults (T obj, string name)
        {
            T value;
            if (nameLookup.TryGetValue (name, out value))
            {
                obj.CopyFieldsFrom (value);
                return;
            }
            throw new ApplicationException ("Registry.SetDefaults(T, string): type '" + name + "' not found.");
        }

        public T Create(string name)
        {
            T t;
            if (nameLookup.TryGetValue(name, out t))
            {
                return CloneAndInit(t);
            }
            return CloneAndInit(defaultValue);
        }


        public T FindClass(string name)
        {
            List<T> values;
            foreach (int type in typeLookup.Keys)
            {
                if (typeLookup.TryGetValue(type, out values))
                {
                    foreach (T value in values)
                    {
                        if (value.Name.ToLower().Replace(" ", "").Trim() == name.ToLower().Replace(" ", "").Trim()) //Exact :3
                        {
                            //CloneAndInit(values[i]);
                            return value;
                        }
                    }
                }
            }
            return Default;
        }

        private static T CloneAndInit(T t)
        {
            T cloned = (T) t.Clone();
            if (cloned.Type != 0)
            {
                cloned.Active = true;
            }
            return cloned;
        }
        
		public T GetTemplate (int type)
		{
			List<T> values;
			if (typeLookup.TryGetValue (type, out values))
			{
				return values[0];
			}
			return default(T);
		}
    }
}
