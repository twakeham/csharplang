using System;
using System.Collections.Generic;


namespace Language {
    class Parser {

        private string _filename;
        private string _code;

        private int _cursor;
        private int _line = 1;
        private int _position = 1;

        private string[] _factorOperators = { "*", "/" };
        private string[] _termOperators = { "+", "-" };
        private string[] _expressionOperators = { "<=", ">=", "==", "!=", "<", ">" };
        private string[] _dataTypes = { "int", "float", "string", "bool" };
        private Dictionary<string, DataType> _dataTypeMapping = new Dictionary<string, DataType>() {
            { "string", DataType.StringType },
            { "int", DataType.IntegerType },
            { "float", DataType.FloatType },
            { "bool", DataType.BooleanType },
            { "callable", DataType.CallableType }
        };

        private Stack<Node> _stack;

        public const string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string NotLetter = "0";
        public const string Numbers = "0123456789";
        public const string NotNumber = "a";
        public const string Identifier = Letters + Numbers + "_";

        public Parser(string filename) {
            _stack = new Stack<Node>();

            _filename = filename;
            _code = System.IO.File.ReadAllText(filename);
            // strip pesky windows line endings
            _code = _code.Replace("\r", "");
        }

        public Node parse () {
            ProgramNode rootNode = program();
            rootNode.staticTypeCheck();
            return rootNode;
        }

        private bool advance(int characters) {
            _cursor += characters;
            _code = _code.Slice(characters);
            Console.WriteLine("####" + _code.Slice(0, 1));
            _position++;
            
            if (peek() == "\n" || peek() == " " || peek() == "\t") {
                if (peek() == "\n") {
                    _line++;
                    _position = 1;
                    advance(1);
                } else {
                    advance(1);
                }
                return true;
            }
            return false;
        }

        private void expect(string match) {
            if (_code.StartsWith(match)) {
                advance(match.Length);
                return;
            }
            Log.Error(String.Format("Expected {0}, got {1}", match, _code.Slice(0, 1)), _filename, _line, _position);
        }

        private bool accept(string match) {
            if(_code.StartsWith(match)) {
                advance(match.Length);
                return true;
            }
            return false;
        }

        private string oneOf(string[] options) {
            foreach(string option in options) {
                if (accept(option))
                    return option;
            }
            return null;
        }

        private string match(string characters) {
            string character = peek();
            if (character == null)
                return null;
            if (characters.Contains(character)) {
                advance(1);
                return character;
            }
            return null;
        }

        private string peek() {
            return _code.Slice(0, 1);
        }

        private ProgramNode program() {
            SymbolTable.CreateGlobalScope();
            statementList();
            StatementListNode statementListNode = (StatementListNode)_stack.Pop();
            ProgramNode program = new ProgramNode(statementListNode.Statements, _filename);
            return program;
        }

        private void block() {
            expect("{");
            SymbolTable.EnterScope();
            statementList();
            SymbolTable.ExitScope();
            expect("}");
        }

        private void statementList () {
            int line = _line;
            int position = _position;

            List<Node> statements = new List<Node>();
            while (statement()) {
                statements.Add(_stack.Pop());
            }

            StatementListNode node = new StatementListNode(statements, _filename, line, position);
            _stack.Push(node);
        }

        private bool statement() {
            if (ifStatement())
                return true;

            if (whileStatement())
                return true;

            if (printStatement())
                return true;

            if (identifierDefinition())
                return true;

            if (assignmentStatement())
                return true;

            if (peek() == "}" || peek() == null) {
                return false;
            }

            expression();
            return true;
            
        }

        private bool ifStatement() {
            int line = _line;
            int position = _position;

            if (!accept("if"))
                return false;
            
            expression();
            Node predicate = _stack.Pop();
            block();
            Node trueBranch = _stack.Pop();
            Node falseBranch = null;
            if (accept("else")) {
                block();
                falseBranch = _stack.Pop();
            }
            IfStatementNode node = new IfStatementNode(predicate, trueBranch, falseBranch, _filename, line, position);
            _stack.Push(node);

            return true;
        }

        private bool whileStatement() {
            int line = _line;
            int position = _position;

            if (!accept("while"))
                return false;

            expression();
            Node predicate = _stack.Pop();
            block();
            Node repeat = _stack.Pop();
            WhileStatementNode node = new WhileStatementNode(predicate, repeat, _filename, line, position);
            _stack.Push(node);

            return true;
        }

        private bool printStatement() {
            int line = _line;
            int position = _position;

            if (!accept("print"))
                return false;
            expression();
            Node printExpression = _stack.Pop();
            PrintStatementNode node = new PrintStatementNode(printExpression, _filename, line, position);
            _stack.Push(node);

            return true;
        }

        private bool identifierDefinition() {
            int line = _line;
            int position = _position;

            if (!parameter())
                return false;

            IdentifierDefinitionNode identifier = (IdentifierDefinitionNode)_stack.Pop();
            Node rvalue = null;

            // default value
            if (accept("=")) {
                expression();
                rvalue = _stack.Pop();
                identifier.Value = rvalue;
            }

            // function definition
            else if (accept("(")) {
                
                // create new scope here so that parameters don't leak out
                SymbolTable.EnterScope();
                List<Node> parameters = parameterList();
                block();
                SymbolTable.ExitScope();

                Node functionBody = _stack.Pop();

                CallableType type = new CallableType(identifier.Type, parameters, functionBody);
                SymbolTable.CurrentScope.Callables[identifier.Symbol.Identifier] = type;

                FunctionDefinitionNode node = new FunctionDefinitionNode(type, identifier.Symbol, _filename, line, position);
                _stack.Push(node);

                return true;

            }

            _stack.Push(identifier);
            return true;
        }

        private List<Node> parameterList() {
            List<Node> parameters = new List<Node>();
            if (parameter())
                parameters.Add(_stack.Pop());

            while(accept(",")) {
                parameter();
                parameters.Add(_stack.Pop());
            }

            expect(")");

            return parameters;
        }

        private bool parameter() {
            int line = _line;
            int position = _position;

            string type = oneOf(_dataTypes);
            if (type == null)
                return false;

            if (!identifier())
                Log.Error("Expected identifier", _filename, _line, _position);

            Node lvalue = _stack.Pop();
            DataType dataType = _dataTypeMapping[type];
            IdentifierDefinitionNode node = new IdentifierDefinitionNode(dataType, lvalue, null, _filename, line, position);
            _stack.Push(node);

            return true;
        }

        private bool assignmentStatement() {
            int line = _line;
            int position = _position;

            if (!accept("set"))
                return false;
            identifier();
            Node symbol = _stack.Pop();
            expect("=");
            expression();
            Node value = _stack.Pop();
            AssignmentNode node = new AssignmentNode(symbol, value, _filename, line, position);
            _stack.Push(node);

            return true;
        }

        private void expression () {
            term();

            string op = oneOf(_expressionOperators);
            while (op != null) {
      
                term();

                Node rightOperand = _stack.Pop();
                Node leftOperand = _stack.Pop();

                Node node = null;

                switch (op) {
                    case "<":
                        node = new LessThanNode(leftOperand, rightOperand, _filename, _line, _position - 1);
                        break;

                    case "<=":
                        node = new LessThanOrEqualNode(leftOperand, rightOperand, _filename, _line, _position - 2);
                        break;

                    case ">":
                        node = new GreaterThanNode(leftOperand, rightOperand, _filename, _line, _position - 1);
                        break;

                    case ">=":
                        node = new GreaterThanOrEqualNode(leftOperand, rightOperand, _filename, _line, _position - 2);
                        break;

                    case "==":
                        node = new EqualNode(leftOperand, rightOperand, _filename, _line, _position - 2);
                        break;

                    case "!=":
                        node = new NotEqualNode(leftOperand, rightOperand, _filename, _line, _position - 2);
                        break;
                }

                _stack.Push(node);
                op = oneOf(_expressionOperators);
            }
        }

        private void term() {
            /// term = factor [('+' | '-') factor]
            factor();

            string op = oneOf(_termOperators);
            while (op != null) {
                int line = _line;
                int position = _position;

                factor();

                Node rightOperand = _stack.Pop();
                Node leftOperand = _stack.Pop();

                Node node;
                if (op == "+") {
                    node = new AdditionNode(leftOperand, rightOperand, _filename, _line, _position - 1);
                } else {
                    node = new SubtractionNode(leftOperand, rightOperand, _filename, _line, _position - 1);
                }

                _stack.Push(node);
                op = oneOf(_termOperators);
            }
        }

        private void factor() {
            /// factor = exponent [('*' | '/') exponent]
            exponent();

            string op = oneOf(_factorOperators);
            while (op != null) {
                int line = _line;
                int position = _position;

                exponent();

                Node rightOperand = _stack.Pop();
                Node leftOperand = _stack.Pop();

                Node node;
                if (op == "*") {
                    node = new MultiplyNode(leftOperand, rightOperand, _filename, _line, _position - 2);
                } else {
                    node = new DivideNode(leftOperand, rightOperand, _filename, _line, _position - 2);
                }

                _stack.Push(node);
                op = oneOf(_factorOperators);
            }
        }

        private void exponent() {
            /// exponent = component ['**' component]
            component();
            while(accept("^")) {
                int line = _line;
                int position = _position;

                component();

                Node rightOperand = _stack.Pop();
                Node leftOperand = _stack.Pop();
                ExponentialNode node = new ExponentialNode(leftOperand, rightOperand, _filename, _line, _position - 2);
                _stack.Push(node);
            }
        }

        private void component() {
            /// component = prefixOperator? ( number | identifier | '"' [^"] '"' | '(' expression ')' ) postFixOperator?
            PrefixOperatorNode prefix = null;
            if (prefixOperator())
                prefix = (PrefixOperatorNode)_stack.Pop();

            if (number() || characterString()) {
                if (prefix != null) {
                    prefix.Target = _stack.Pop();
                    _stack.Push(prefix);
                }
                return;
            } 
            
            else if (identifier()) {
                if (functionCall()) {
                    FunctionCallNode node = (FunctionCallNode) _stack.Pop();
                    IdentifierNode identifierNode = (IdentifierNode) _stack.Pop();
                    node.Identifier = identifierNode.Identifier;
                    _stack.Push(node);
                }
                if (prefix != null) {
                    prefix.Target = _stack.Pop();
                    _stack.Push(prefix);
                }
                return;
            }

            expect("(");
            expression();
            expect(")");
        }

        private bool prefixOperator() {
            if (castOperator())
                return true;

            return false;
        }

        private bool castOperator() {
            int line = _line;
            int position = _position;

            if (!accept("<"))
                return false;

            string type = oneOf(_dataTypes);
            if (type == null)
                Log.Error("Expected type", _filename, line, position);

            expect(">");

            DataType dataType = _dataTypeMapping[type];
            TypeCastNode node = new TypeCastNode(dataType, null, _filename, line, position);
            _stack.Push(node);

            return true;
        }

        private bool functionCall() {
            int line = _line;
            int position = _position;

            if (!accept("("))
                return false;

            List<Node> parameterList;

            if (accept(")")) {
                parameterList = new List<Node>();
            } else {
                parameterList = expressionList();
                expect(")");
            }

            FunctionCallNode node = new FunctionCallNode(null, parameterList, SymbolTable.CurrentScope, _filename, line, position);
            _stack.Push(node);

            return true;
        }

        private List<Node> expressionList() {
            List<Node> expressions = new List<Node>();
            expression();
            expressions.Add(_stack.Pop());

            while (accept(",")) {
                expression();
                expressions.Add(_stack.Pop());
            }

            return expressions;
        }

        private bool number() {
            /// number = digits+ ('.' digits+)?
            string digits = null;
            int line = _line;
            int position = _position;

            while (Numbers.Contains(peek() ?? NotNumber)) {
                digits = (digits ?? "") + peek();
                if (advance(1))
                    break;
            }

            // floating point part
            if (accept(".")) {
                digits += ".";
                while (Numbers.Contains(peek() ?? NotNumber)) {
                    digits = (digits ?? "") + peek();
                    if (advance(1))
                        break;
                }
            }

            if (digits != null) {
                Node node;
                if (digits.Contains(".")) {
                    node = new FloatNode(digits, _filename, line, position);
                } else {
                    node = new IntegerNode(digits, _filename, line, position);
                }
                _stack.Push(node);
                return true;
            }
            return false;
        }

        private bool identifier() {
            /// identifier = letter [letter | digits | '_']
            string characters = null;
            int line = _line;
            int position = _position;
            
            // ident has to start with a letter
            if (!Letters.Contains(peek() ?? NotLetter)) return false;
            while(Identifier.Contains(peek() ?? NotLetter)) {
                characters = (characters ?? "") + peek();
                if (advance(1))
                    break;
            }
            if (characters != null) {
                IdentifierNode node = new IdentifierNode(characters, SymbolTable.CurrentScope, _filename, line, position);
                _stack.Push(node);
                return true;
            }
            return false;
        }

        private bool characterString() {
            string characters = "";
            int line = _line;
            int position = _position;

            if (!accept("\""))
                return false;

            while (!accept("\"")) {
                string character = peek();
                if (character == null)
                    Log.Error("EOF scanning string", _filename, _line, _position);
                characters += character;
                advance(1);
            }
            StringNode node = new StringNode(characters, _filename, line, position);
            _stack.Push(node);

            return true;
        }

    }
}
