using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language {

    public class SymbolTable {
        private static Stack<SymbolTable> SymbolTables = new Stack<SymbolTable>();

        public static SymbolTable CreateGlobalScope () {
            SymbolTable.SymbolTables.Push(new SymbolTable());
            return SymbolTable.SymbolTables.Peek();
        }

        public static SymbolTable EnterScope () {
            SymbolTable parentScope = SymbolTable.SymbolTables.Peek();
            SymbolTable.SymbolTables.Push(new SymbolTable(parentScope));
            return SymbolTable.SymbolTables.Peek();
        }

        public static void ExitScope () {
            SymbolTable.SymbolTables.Pop();
        }

        public static SymbolTable CurrentScope { get { return SymbolTable.SymbolTables.Peek(); } }

        public static bool InScope (string symbol, DataType type) {
            SymbolTable currentScope = SymbolTable.SymbolTables.Peek();
            DataType definedType;
            try {
                definedType = currentScope.Symbols[symbol];
            } catch (KeyNotFoundException) {
                return false;
            }
            return definedType == type;
        }

        public Dictionary<string, DataType> Symbols { get; set; }

        public Dictionary<string, CallableType> Callables { get; set; }

        private Dictionary<string, DataType> _currentScopeSymbols;

        public SymbolTable() {
            Symbols = new Dictionary<string, DataType>();
            Callables = new Dictionary<string, CallableType>();

            _currentScopeSymbols = new Dictionary<string, DataType>();
        }

        public SymbolTable(SymbolTable parent) {
            Symbols = new Dictionary<string, DataType>(parent.Symbols);
        }

        public bool addSymbol(string symbol, DataType type) {
            if (_currentScopeSymbols.ContainsKey(symbol)) 
                return false;
           
            Symbols[symbol] = type;
            _currentScopeSymbols[symbol] = type;
            return true;
        }
    }

}
