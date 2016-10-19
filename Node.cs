using System;
using System.Collections.Generic;


namespace Language {

    public class Node {
        public string Filename { get; set; }
        public int Line { get; set; }
        public int Position { get; set; }

        public Node (string filename, int line, int position) {
            Filename = filename;
            Line = line;
            Position = position;
        }

        public virtual string toSyntaxGraph () {
            return string.Format("[{0}]", this.GetType().Name);
        }

        public virtual DataType staticTypeCheck() {
            return DataType.NoneType;
        }

        public virtual void interpret() { }
    }

    public class IntegerNode : Node {
        public string Number { get; set; }

        public IntegerNode (string number, string filename, int line, int position) : base(filename, line, position) {
            Number = number;
        }

        public override string toSyntaxGraph () {
            return string.Format("[{0} {1}]", this.GetType().Name, Number);
        }

        public override DataType staticTypeCheck() {
            return DataType.IntegerType;
        }
    }

    public class FloatNode : Node {
        public string Number { get; set; }

        public FloatNode (string number, string filename, int line, int position) : base(filename, line, position) {
            Number = number;
        }

        public override string toSyntaxGraph () {
            return string.Format("[{0} {1}]", this.GetType().Name, Number);
        }

        public override DataType staticTypeCheck() {
            return DataType.FloatType;
        }
    }

    public class StringNode : Node {
        public string Characters { get; set; }

        public StringNode (string characters, string filename, int line, int position) : base(filename, line, position) {
            Characters = characters;
        }

        public override string toSyntaxGraph () {
            return string.Format("[{0} \"{1}\"]", this.GetType().Name, Characters);
        }

        public override DataType staticTypeCheck() {
            return DataType.StringType;
        }
    }

    public class IdentifierNode : Node {
        public string Identifier { get; set; }
        public SymbolTable Scope { get; set; }

        public IdentifierNode (string identifier, SymbolTable scope, string filename, int line, int position) : base(filename, line, position) {
            Identifier = identifier;
            Scope = scope;
        }

        public override string toSyntaxGraph () {
            return string.Format("[{0} {1}]", this.GetType().Name, Identifier);
        }

        public override DataType staticTypeCheck() {
            DataType symbolType = DataType.NoneType;
            try {
                symbolType = Scope.Symbols[Identifier];
            } catch (KeyNotFoundException) {
                Log.Error(String.Format("Variable {0} used before defined", Identifier), Filename, Line, Position);
            }
            return symbolType;
        }
    }

    public class FunctionCallNode : IdentifierNode {

        public List<Node> Parameters { get; set; }

        public FunctionCallNode (string identifier, List<Node> parameters, SymbolTable scope, string filename, int line, int position) : base(identifier, scope, filename, line, position) {
            Parameters = parameters;
        }

        public override string toSyntaxGraph () {
            string parameters = "";
            foreach (Node parameter in Parameters) {
                parameters += parameter.toSyntaxGraph();
            }
            return string.Format("[{0}() [PARAMS {1}]]", Identifier, parameters);
        }

        public override DataType staticTypeCheck () {
            DataType type = base.staticTypeCheck();
            // check if callable
            Console.WriteLine(type.ToString());
            if (type != DataType.CallableType) 
                Log.Error(String.Format("{0} is not callable", Identifier), Filename, Line, Position);

            CallableType callable = Scope.Callables[Identifier];

            // check parameter count matches defined parameter count
            if (Parameters.Count != callable.ParameterList.Count)
                Log.Error("Parameter count mismatch", Filename, Line, Position);

            // check parameter types match
            for (int index = 0; index < Parameters.Count; index++) {
                DataType parameterType = Parameters[index].staticTypeCheck();
                DataType definedType = callable.ParameterList[index].staticTypeCheck();
                if (parameterType != callable.ParameterList[index].staticTypeCheck())
                    Log.Error(String.Format("Parameter type mismatch {0}->{1}", definedType.ToString(), parameterType.ToString()), Filename, Line, Position);
            }

            return callable.ReturnType;

        }

    }

    public class BinaryOperatorNode : Node {
        public Node LeftOperand { get; set; }
        public Node RightOperand { get; set; }

        public BinaryOperatorNode (Node leftOperand, Node rightOperand, string filename, int line, int position) : base(filename, line, position) {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }

        public override string toSyntaxGraph () {
            return string.Format("[{0} {1} {2}]", this.GetType().Name, LeftOperand.toSyntaxGraph(), RightOperand.toSyntaxGraph());
        }

        public override DataType staticTypeCheck() {
            DataType leftType = LeftOperand.staticTypeCheck();
            DataType rightType = RightOperand.staticTypeCheck();

            if (leftType == DataType.StringType || rightType == DataType.StringType) {
                Log.Error(String.Format("Type mismatch - {0}->{1}", leftType.ToString(), rightType.ToString()), Filename, Line, Position);
            }

            if (leftType == DataType.FloatType || rightType == DataType.FloatType) {
                if (leftType == DataType.IntegerType) {
                    Log.Warning("Implicit IntType->FloatType cast", Filename, Line, Position);
                    TypeCastNode castNode = new TypeCastNode(DataType.FloatType, LeftOperand, LeftOperand.Filename, LeftOperand.Line, LeftOperand.Position);
                    LeftOperand = castNode;
                }
                if (rightType == DataType.IntegerType) {
                    Log.Warning("Implicit IntType->FloatType cast", Filename, Line, Position);
                    TypeCastNode castNode = new TypeCastNode(DataType.FloatType, RightOperand, RightOperand.Filename, RightOperand.Line, RightOperand.Position);
                    RightOperand = castNode;
                }
                return DataType.FloatType;
            }

            return DataType.IntegerType;
        }
    }

    public class ExponentialNode : BinaryOperatorNode {
        public ExponentialNode(Node leftOperand, Node rightOperand, string filename, int line, int position) : base(leftOperand, rightOperand, filename, line, position) { }
    }

    public class MultiplyNode : BinaryOperatorNode {
        public MultiplyNode (Node leftOperand, Node rightOperand, string filename, int line, int position) : base(leftOperand, rightOperand, filename, line, position) { }
    }

    public class DivideNode : BinaryOperatorNode {
        public DivideNode (Node leftOperand, Node rightOperand, string filename, int line, int position) : base(leftOperand, rightOperand, filename, line, position) { }
    }

    public class AdditionNode : BinaryOperatorNode {
        public AdditionNode (Node leftOperand, Node rightOperand, string filename, int line, int position) : base(leftOperand, rightOperand, filename, line, position) { }
    }

    public class SubtractionNode : BinaryOperatorNode {
        public SubtractionNode(Node leftOperand, Node rightOperand, string filename, int line, int position) : base(leftOperand, rightOperand, filename, line, position) { }
    }

    public class ComparisonNode : BinaryOperatorNode {
        public ComparisonNode(Node leftOperand, Node rightOperand, string filename, int line, int position) : base(leftOperand, rightOperand, filename, line, position) { }

        public override DataType staticTypeCheck() {
            base.staticTypeCheck();
            return DataType.BooleanType;
        }
    }

    public class GreaterThanNode : ComparisonNode {
        public GreaterThanNode (Node leftOperand, Node rightOperand, string filename, int line, int position) : base(leftOperand, rightOperand, filename, line, position) { }
    }

    public class GreaterThanOrEqualNode : ComparisonNode {
        public GreaterThanOrEqualNode (Node leftOperand, Node rightOperand, string filename, int line, int position) : base(leftOperand, rightOperand, filename, line, position) { }
    }

    public class LessThanNode : ComparisonNode {
        public LessThanNode (Node leftOperand, Node rightOperand, string filename, int line, int position) : base(leftOperand, rightOperand, filename, line, position) { }
    }

    public class LessThanOrEqualNode : ComparisonNode {
        public LessThanOrEqualNode (Node leftOperand, Node rightOperand, string filename, int line, int position) : base(leftOperand, rightOperand, filename, line, position) { }
    }

    public class EqualNode : ComparisonNode {
        public EqualNode (Node leftOperand, Node rightOperand, string filename, int line, int position) : base(leftOperand, rightOperand, filename, line, position) { }

        public override DataType staticTypeCheck() {
            DataType leftType = LeftOperand.staticTypeCheck();
            DataType rightType = RightOperand.staticTypeCheck();

            if (leftType == DataType.StringType && rightType == DataType.StringType) {
                return DataType.StringType;
            }

            else if (leftType == DataType.FloatType || rightType == DataType.FloatType) {
                return DataType.FloatType;
            }

            else if (leftType == DataType.IntegerType && rightType == DataType.IntegerType) {
                return DataType.IntegerType;
            }

            Log.Error("Type mismatch", Filename, Line, Position);
            return DataType.NoneType;
        }
    }

    public class NotEqualNode : EqualNode {
        public NotEqualNode(Node leftOperand, Node rightOperand, string filename, int line, int position) : base(leftOperand, rightOperand, filename, line, position) { }
    }

    public class PrefixOperatorNode : Node {
        public Node Target { get; set; }

        public PrefixOperatorNode(Node target, string filename, int line, int position) : base(filename, line, position) { }
    }

    public class TypeCastNode : PrefixOperatorNode {
        public DataType TargetDataType { get; set; }

        public TypeCastNode(DataType targetDataType, Node target, string filename, int line, int position) : base(target, filename, line, position) {
            TargetDataType = targetDataType;
            Target = target;
        }

        public override string toSyntaxGraph() {
            return String.Format("[CAST({0}) {1}]", TargetDataType.ToString(), Target.toSyntaxGraph());
        }

        public override DataType staticTypeCheck() {
            DataType targetType = Target.staticTypeCheck();
            if (targetType == DataType.StringType)
                Log.Error(String.Format("Unable to cast from StringType to {0}", TargetDataType.ToString()), Filename, Line, Position);
            return TargetDataType;
        }
    }

    public class StatementNode : Node {
        public StatementNode (string filename, int line, int position) : base(filename, line, position) { }
    }

    public class StatementListNode : Node {
        public List<Node> Statements { get; set; }

        public StatementListNode (List<Node> statements, string filename, int line, int position) : base(filename, line, position) {
            Statements = statements;
        }

        public override string toSyntaxGraph () {
            string output = "";
            foreach (Node statement in Statements) {
                output += statement.toSyntaxGraph() + " ";
            }
            //output += "]";
            return output;
        }

        public override DataType staticTypeCheck() {
            foreach(Node statement in Statements) {
                statement.staticTypeCheck();
            }
            return DataType.NoneType;
        }
    }

    public class IfStatementNode : StatementNode {
        public Node Predicate { get; set; }
        public Node TrueBranch { get; set; }
        public Node FalseBranch { get; set; }

        public IfStatementNode (Node predicate, Node trueBranch, Node falseBranch, string filename, int line, int position) : base(filename, line, position) {
            Predicate = predicate;
            TrueBranch = trueBranch;
            FalseBranch = falseBranch;
        }

        public override string toSyntaxGraph () {
            return String.Format("[IF [CONDITION {0}] [THEN {1}] [ELSE {2}]]", Predicate.toSyntaxGraph(), TrueBranch.toSyntaxGraph(), FalseBranch?.toSyntaxGraph() ?? "NULL");
        }

        public override DataType staticTypeCheck() {
            if (Predicate.staticTypeCheck() != DataType.BooleanType) 
                Log.Error("Type mismatch - predicate must evaluate to boolean", Filename, Line, Position);
            TrueBranch.staticTypeCheck();
            FalseBranch?.staticTypeCheck();

            return DataType.NoneType;
        }
    }

    public class WhileStatementNode : StatementNode {
        public Node Predicate { get; set; }
        public Node Repeat { get; set; }

        public WhileStatementNode (Node predicate, Node repeat, string filename, int line, int position) : base(filename, line, position) {
            Predicate = predicate;
            Repeat = repeat;
        }

        public override string toSyntaxGraph () {
            return String.Format("[WHILE [CONDITION {0}] [DO {1}]]", Predicate.toSyntaxGraph(), Repeat.toSyntaxGraph());
        }

        public override DataType staticTypeCheck() {
            if (Predicate.staticTypeCheck() != DataType.BooleanType) 
                Log.Error("Type mismatch - predicate must evaluate to boolean", Filename, Line, Position);
            Repeat.staticTypeCheck();

            return DataType.NoneType;
        }
    }

    public class PrintStatementNode : StatementNode {

        public Node Expression { get; set; }

        public PrintStatementNode (Node expression, string filename, int line, int position) : base(filename, line, position) {
            Expression = expression;
        }

        public override string toSyntaxGraph () {
            return String.Format("[PRINT {0}]", Expression.toSyntaxGraph());
        }

        public override DataType staticTypeCheck() {
            Expression.staticTypeCheck();
            return DataType.NoneType;
        }
    }

    public class IdentifierDefinitionNode : StatementNode {
        public DataType Type { get; set; }
        public IdentifierNode Symbol { get; set; }
        public Node Value { get; set; }

        public IdentifierDefinitionNode(DataType type, Node symbol, Node value, string filename, int line, int position) : base(filename, line, position) {
            Type = type;
            Symbol = (IdentifierNode)symbol;
            Value = value;

            if (!SymbolTable.CurrentScope.addSymbol(Symbol.Identifier, Type))
                Log.Error("Identifier already exists in this scope", Filename, Line, Position);
        }

        public override string toSyntaxGraph () {
            return String.Format("[VAR [= {0} {1}]]", Symbol.toSyntaxGraph(), Value?.toSyntaxGraph() ?? "NULL");
        }

        public override DataType staticTypeCheck() {
            if ((Value?.staticTypeCheck() ?? Type) != Type)
                Log.Error("Type mismatch - rvalue not equal to lvalue", Filename, Line, Position);
            return Type;
        }
    }

    public class FunctionDefinitionNode : IdentifierDefinitionNode {

        public CallableType CallableType { get; set; }

        public FunctionDefinitionNode (CallableType callableType, Node symbol, string filename, int line, int position) : base(DataType.CallableType, symbol, null, filename, line, position) {
            CallableType = callableType;
        }

        public override string toSyntaxGraph () {
            string parameters = "";
            foreach (Node parameter in CallableType.ParameterList) {
                parameters += parameter.toSyntaxGraph();
            }
            return String.Format("[{0}(..) [PARAMS {1}] [EXEC {2}] [RET {3}]]", Symbol.Identifier, parameters, CallableType.FunctionBody.toSyntaxGraph(), CallableType.ReturnType.ToString());
        }
    }


    public class AssignmentNode : StatementNode {
        public IdentifierNode Symbol { get; set; }
        public Node Value { get; set; }

        public AssignmentNode(Node identifier, Node value, string filename, int line, int position) : base(filename, line, position) {
            Symbol = (IdentifierNode)identifier;
            Value = value;
        }
   
        public override string toSyntaxGraph() {
            return String.Format("[ASSIGN [= {0} {1}]]", Symbol.toSyntaxGraph(), Value.toSyntaxGraph());
        }

        public override DataType staticTypeCheck() {
            DataType type = DataType.NoneType;
            try {
                type = Symbol.Scope.Symbols[Symbol.Identifier];
            } catch (KeyNotFoundException) {
                Log.Error("Variable referenced before assignment", Filename, Line, Position);
            }

            if (type != Value.staticTypeCheck())
                Log.Error("Assignment type mismatch", Filename, Line, Position);

            return DataType.NoneType;
        }
    }


    public class ProgramNode : StatementListNode {

        public ProgramNode(List<Node> statements, string filename) : base(statements, filename, 1, 1) { }

    }

}