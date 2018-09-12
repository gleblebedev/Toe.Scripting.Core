using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Toe.Scripting.Defines
{
    public class Operands : IReadOnlyList<string>
    {
        private static readonly string[] emptyArray = new string[0];
        public static readonly Operands Empty = new Operands(emptyArray);
        private readonly string[] _values;

        public Operands()
        {
            _values = emptyArray;
        }

        public Operands(params string[] operands)
        {
            ValidateOperands(operands);
            _values = operands;
        }

        private static void ValidateOperands(string[] operands)
        {
            for (var index = 0; index < operands.Length; index++)
            {
                var operand = operands[index];
                if (operand == null)
                    throw new ArgumentException("Operand can't be null", nameof(operand)+"["+index+"]");
                if (string.IsNullOrWhiteSpace(operand))
                    throw new ArgumentException("Operand can't be empty", nameof(operand) + "[" + index + "]");
            }
        }

        public Operands(IEnumerable<string> operands)
        {
            _values = operands.ToArray();
            ValidateOperands(_values);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return ((IList<string>) _values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public int Count => _values.Length;

        public string this[int index] => _values[index];

        public int IndexOf(string operand)
        {
            for (var index = 0; index < _values.Length; index++)
                if (_values[index] == operand)
                    return index;

            return -1;
        }

        public override string ToString()
        {
            return string.Join(", ", _values);
        }
    }
}