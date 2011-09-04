﻿using System.IO;
using System.Threading;
using System.Collections.Generic;
using System;
using Terraria_Server.Logging;

namespace Terraria_Server.Misc
{
    public class PropertiesFile
    {
        private const char EQUALS = '=';

        private Dictionary<String, String> propertiesMap;

        private string propertiesPath = String.Empty;
		
		public int Count
		{
			get { return propertiesMap.Count; }
		}

        public PropertiesFile(string propertiesPath)
        {
            propertiesMap = new Dictionary<String, String>();
            this.propertiesPath = propertiesPath;
        }

        public void Load() {
            //Verify that the properties file exists and we can create it if it doesn't.
            if (!File.Exists(propertiesPath))
            {
                File.WriteAllText(propertiesPath, String.Empty);
            }

            propertiesMap.Clear();
            StreamReader reader = new StreamReader(propertiesPath);
            try
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    int setterIndex = line.IndexOf(EQUALS);
                    if (setterIndex > 0 && setterIndex < line.Length)
                    {
                        propertiesMap.Add(line.Substring(0, setterIndex), line.Substring(setterIndex + 1));
                    }
                }
            }
            finally
            {
                reader.Close();
            }
        }

        /* This is for Plugins, Because they are referenced to the old one
         * Other words: To avoid Plugin breaks.
         * 
         * [Changed for Commands needing to save properties and Log is spammed]
         */
        [Obsolete("Out of date, new Parameters added")] 
        public void Save()
        {
            Save(true);
        }

        public void Save(bool log = true)
        {
            var tmpName = propertiesPath + ".tmp" + (uint) (DateTime.UtcNow.Ticks % uint.MaxValue);
            var writer = new StreamWriter (tmpName);
            try
            {
                foreach (KeyValuePair<String, String> pair in propertiesMap)
                {
                    if (pair.Value != null)
                        writer.WriteLine(pair.Key + EQUALS + pair.Value);
                }
            }
            finally
            {
                writer.Close();
            }

            if (log)
            {
                try
                {
                    File.Replace(tmpName, propertiesPath, null, true);
                    ProgramLog.Log("Saved file \"{0}\".", propertiesPath);
                }
                catch (IOException e)
                {
                    ProgramLog.Log("Save to \"{0}\" failed: {1}", propertiesPath, e.Message);
                }
                catch (SystemException e)
                {
                    ProgramLog.Log("Save to \"{0}\" failed: {1}", propertiesPath, e.Message);
                }
            }
            
        }

        public string getValue(string key)
        {
            if (propertiesMap.ContainsKey(key))
            {
                return propertiesMap[key];
            }
            return null;
        }

        public string getValue(string key, string defaultValue)
        {
            string value = getValue(key);
            if (value == null || value.Trim().Length < 0)
            {
                setValue(key, defaultValue);
                return defaultValue;
            }
            return value;
        }

        public int getValue(string key, int defaultValue)
        {
            int result;
            if (Int32.TryParse(getValue(key), out result))
            {
                return result;
            }

            setValue(key, defaultValue);
            return defaultValue;
        }

        public bool getValue(string key, bool defaultValue)
        {
            bool result;
            if (Boolean.TryParse(getValue(key), out result))
            {
                return result;
            }

            setValue(key, defaultValue);
            return defaultValue;
        }

        public void setValue(string key, string value)
        {
            propertiesMap[key] = value;
        }

        protected void setValue(string key, int value)
        {
            setValue(key, value.ToString());
        }

        protected void setValue(string key, bool value)
        {
            setValue(key, value.ToString());
        }
    }
}
