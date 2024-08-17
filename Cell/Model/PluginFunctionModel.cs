﻿
using Cell.Common;

namespace Cell.Model
{
    /// <summary>
    /// Serializable object that represents a plugin function, which is a method that can be called from a cell.
    /// </summary>
    public class PluginFunctionModel : PropertyChangedBase
    {
        private const string codeHeader = "using System; using System.Linq; using System.Collections.Generic; using Cell.Model; using Cell.ViewModel; using Cell.Model.Plugin; using Cell.Plugin;\n\nnamespace Plugin { public class Program { public static ";
        private const string codeFooter = "\n}}}";
        private const string methodHeader = " PluginMethod(PluginContext c, CellModel cell) {\n";

        public PluginFunctionModel(string name, string code, string returnType)
        {
            Name = name;
            ReturnType = returnType;
            Code = code;
        }

        public PluginFunctionModel() { }

        public string Name
        {
            get { return _name; }
            set 
            { 
                if (_name == value) return;
                _name = value; 
                NotifyPropertyChanged(nameof(Name));
            }
        }
        private string _name = string.Empty;

        public string ReturnType
        {
            get { return _returnType; }
            set { if (_returnType == value) return; _returnType = value; NotifyPropertyChanged(nameof(ReturnType)); }
        }
        private string _returnType = string.Empty;

        public string Code 
        { 
            get => _code;
            set
            {
                var fingerprint = value.GetHashFromString();
                if (fingerprint == Fingerprint) return;
                _code = value;
                Fingerprint = fingerprint;
                NotifyPropertyChanged(nameof(Code));
            }
        }
        private string _code = string.Empty;

        public ulong Fingerprint { get; private set; }

        public string FullCode => codeHeader + ReturnType + methodHeader + Code + codeFooter;
    }
}