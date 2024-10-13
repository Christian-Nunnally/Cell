using Cell.Core.Common;

namespace Cell.Model
{
    /// <summary>
    /// Serializable object that represents a plugin function, which is a method that can be called from a cell.
    /// </summary>
    public class CellFunctionModel : PropertyChangedBase
    {
        /// <summary>
        /// A null function that can be used as a placeholder.
        /// </summary>
        public static readonly CellFunctionModel Null = new() { Name = "Null", Code = "" };
        private string _code = string.Empty;
        private string _name = string.Empty;
        private string _returnType = string.Empty;
        /// <summary>
        /// Creates a new instance of <see cref="CellFunctionModel"/>. Used by deserialization.
        /// </summary>
        public CellFunctionModel()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="CellFunctionModel"/>
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="code">The initial code the function should have. Can be empty to start blank.</param>
        /// <param name="returnType">The return type of the function, normally 'void' or 'object'.</param>
        public CellFunctionModel(string name, string code, string returnType)
        {
            Name = name;
            ReturnType = returnType;
            Code = code;
        }

        /// <summary>
        /// The raw code of the function as it is executed, without any syntax conversions applied.
        /// </summary>
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

        /// <summary>
        /// A hash of the code that can be stored by another object and later compared to this value to determine if the code has changed.
        /// </summary>
        public ulong Fingerprint { get; private set; }

        /// <summary>
        /// The name of the function.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// The return type of the function.
        /// </summary>
        public string ReturnType
        {
            get => _returnType;
            set
            {
                if (_returnType == value) return;
                _returnType = value;
                NotifyPropertyChanged(nameof(ReturnType));
            }
        }
    }
}
